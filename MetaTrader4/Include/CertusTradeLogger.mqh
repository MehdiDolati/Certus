//+------------------------------------------------------------------+
//| CertusTradeLogger.mqh - Trade event logging to trades.json      |
//+------------------------------------------------------------------+
#ifndef CERTUS_TRADELOGGER_MQH
#define CERTUS_TRADELOGGER_MQH

#include "CertusConfig.mqh"
#include "CertusFileWriter.mqh"

//--- Track last known ticket count to detect new trades
static int CertusLastHistoryCount = 0;
static int CertusLastOpenCount = 0;

//+------------------------------------------------------------------+
//| Initialize trade tracking                                         |
//+------------------------------------------------------------------+
void CertusInitTradeTracking()
{
   CertusLastHistoryCount = OrdersHistoryTotal();
   CertusLastOpenCount = OrdersTotal();
}

//+------------------------------------------------------------------+
//| Check for new trade events and log them                          |
//+------------------------------------------------------------------+
void CertusCheckAndLogTrades()
{
   if(!CertusLogTrades) return;

   // Check for newly closed trades (moved to history)
   int currentHistoryCount = OrdersHistoryTotal();
   if(currentHistoryCount > CertusLastHistoryCount)
   {
      // New trades in history - find and log them
      for(int i = CertusLastHistoryCount; i < currentHistoryCount; i++)
      {
         if(OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
         {
            string event = CertusBuildTradeEventJSON("close");
            if(event != "")
               CertusAppendFile("trades.json", event + "\n");
         }
      }
      CertusLastHistoryCount = currentHistoryCount;
   }

   // Check for new open trades
   int currentOpenCount = OrdersTotal();
   if(currentOpenCount > CertusLastOpenCount)
   {
      // New open orders detected
      for(int i = 0; i < currentOpenCount; i++)
      {
         if(OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
         {
            // Check if this is a new order we haven't logged
            if(CertusIsNewOrder(OrderTicket()))
            {
               string event = CertusBuildTradeEventJSON("open");
               if(event != "")
                  CertusAppendFile("trades.json", event + "\n");
            }
         }
      }
      CertusLastOpenCount = currentOpenCount;
   }
}

//+------------------------------------------------------------------+
//| Build JSON for a trade event                                      |
//+------------------------------------------------------------------+
string CertusBuildTradeEventJSON(string event)
{
   string json = "{";

   json += "\"id\":\"" + IntegerToString(OrderTicket()) + "\",";
   json += "\"strategyId\":\"" + CertusGetStrategyIdForOrder() + "\",";
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
//| Get strategy ID for the current order                            |
//+------------------------------------------------------------------+
string CertusGetStrategyIdForOrder()
{
   return "EA_" + OrderSymbol() + "_" + CertusPeriodToStringForOrder() + "_" + IntegerToString(AccountNumber());
}

//+------------------------------------------------------------------+
//| Get period string for current order's symbol                     |
//+------------------------------------------------------------------+
string CertusPeriodToStringForOrder()
{
   // For order-based identification, use a fixed identifier
   return "LIVE";
}

//+------------------------------------------------------------------+
//| Check if an order ticket is new (not previously logged)         |
//+------------------------------------------------------------------+
bool CertusIsNewOrder(int ticket)
{
   // Simple heuristic: check if ticket is recent
   static int lastCheckedTicket = 0;
   if(ticket > lastCheckedTicket)
   {
      lastCheckedTicket = ticket;
      return true;
   }
   return false;
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
