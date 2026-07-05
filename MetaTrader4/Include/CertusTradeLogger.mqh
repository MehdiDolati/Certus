//+------------------------------------------------------------------+
//| CertusTradeLogger.mqh - Trade event logging to trades.json      |
//|                                                                    |
//| Monitors all orders across the account. Logs trade events        |
//| (open, close, modify) by comparing current state to last state. |
//+------------------------------------------------------------------+
#ifndef CERTUS_TRADELOGGER_MQH
#define CERTUS_TRADELOGGER_MQH

#include "CertusConfig.mqh"
#include "CertusFileWriter.mqh"

//--- Track last known order counts
static int CertusLastOpenCount = 0;
static int CertusLastHistoryCount = 0;

//+------------------------------------------------------------------+
//| Initialize trade tracking                                         |
//+------------------------------------------------------------------+
void CertusInitTradeTracking()
{
   CertusLastOpenCount = OrdersTotal();
   CertusLastHistoryCount = OrdersHistoryTotal();
}

//+------------------------------------------------------------------+
//| Check for trade events and log them                              |
//+------------------------------------------------------------------+
void CertusCheckAndLogTrades()
{
   if(!CertusLogTrades) return;

   int currentOpen = OrdersTotal();
   int currentHistory = OrdersHistoryTotal();

   // Check for new history entries (closed trades)
   if(currentHistory > CertusLastHistoryCount)
   {
      // Scan new history entries
      for(int i = CertusLastHistoryCount; i < currentHistory; i++)
      {
         if(OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
         {
            string event = CertusBuildTradeEventJSON("close");
            if(event != "")
            {
               string filename = CertusOutputDir + "/trades.json";
               CertusAppendFile(filename, event + "\n");
               if(CertusLogLevel >= 2)
                  Print("[Certus] Trade logged: ticket=", OrderTicket(), " event=close");
            }
         }
      }
   }

   // Check for new open orders
   if(currentOpen > CertusLastOpenCount)
   {
      // Scan current open orders for new ones
      for(int i = 0; i < currentOpen; i++)
      {
         if(OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
         {
            // Simple check: if this ticket is higher than what we've seen
            static int lastLoggedTicket = 0;
            if(OrderTicket() > lastLoggedTicket)
            {
               string event = CertusBuildTradeEventJSON("open");
               if(event != "")
               {
                  string filename = CertusOutputDir + "/trades.json";
                  CertusAppendFile(filename, event + "\n");
                  if(CertusLogLevel >= 2)
                     Print("[Certus] Trade logged: ticket=", OrderTicket(), " event=open");
                  lastLoggedTicket = OrderTicket();
               }
            }
         }
      }
   }

   CertusLastOpenCount = currentOpen;
   CertusLastHistoryCount = currentHistory;
}

//+------------------------------------------------------------------+
//| Build JSON for a trade event                                      |
//+------------------------------------------------------------------+
string CertusBuildTradeEventJSON(string event)
{
   string json = "{";

   json += "\"id\":\"" + IntegerToString(OrderTicket()) + "\",";
   json += "\"strategyId\":\"" + IntegerToString(OrderMagicNumber()) + "\",";
   json += "\"symbol\":\"" + OrderSymbol() + "\",";
   json += "\"side\":\"" + (OrderType() == OP_BUY ? "buy" : "sell") + "\",";
   json += "\"volume\":" + DoubleToString(OrderLots(), 2) + ",";
   json += "\"openPrice\":" + DoubleToString(OrderOpenPrice(), (int)MarketInfo(OrderSymbol(), MODE_DIGITS)) + ",";

   if(event == "close")
   {
      json += "\"closePrice\":" + DoubleToString(OrderClosePrice(), (int)MarketInfo(OrderSymbol(), MODE_DIGITS)) + ",";
   }
   else
   {
      json += "\"closePrice\":null,";
   }

   json += "\"stopLoss\":" + DoubleToString(OrderStopLoss(), (int)MarketInfo(OrderSymbol(), MODE_DIGITS)) + ",";
   json += "\"takeProfit\":" + DoubleToString(OrderTakeProfit(), (int)MarketInfo(OrderSymbol(), MODE_DIGITS)) + ",";
   json += "\"profit\":" + DoubleToString(OrderProfit(), 2) + ",";
   json += "\"commission\":" + DoubleToString(OrderCommission(), 2) + ",";
   json += "\"swap\":" + DoubleToString(OrderSwap(), 2) + ",";
   json += "\"openTime\":\"" + CertusIsoTime(OrderOpenTime()) + "\",";

   if(event == "close")
   {
      json += "\"closeTime\":\"" + CertusIsoTime(OrderCloseTime()) + "\",";
   }
   else
   {
      json += "\"closeTime\":null,";
   }

   json += "\"comment\":\"" + CertusEscapeJson(OrderComment()) + "\"";

   json += "}";
   return json;
}

//+------------------------------------------------------------------+
//| Escape special characters for JSON strings                       |
//+------------------------------------------------------------------+
string CertusEscapeJson(string s)
{
   string result = "";
   for(int i = 0; i < StringLen(s); i++)
   {
      ushort c = StringGetCharacter(s, i);
      if(c == '"')       result += "\\\"";
      else if(c == '\\') result += "\\\\";
      else if(c == '\n') result += "\\n";
      else if(c == '\r') result += "\\r";
      else if(c == '\t') result += "\\t";
      else result += ShortToString(c);
   }
   return result;
}

#endif
