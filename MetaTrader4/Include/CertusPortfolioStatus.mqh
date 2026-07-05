//+------------------------------------------------------------------+
//| CertusPortfolioStatus.mqh - Portfolio status JSON builder        |
//|                                                                    |
//| Single EA on any chart - reads ALL orders across the account.    |
//| Groups orders by magic number to identify different strategies.  |
//+------------------------------------------------------------------+
#ifndef CERTUS_PORTFOLIO_STATUS_MQH
#define CERTUS_PORTFOLIO_STATUS_MQH

#include "CertusConfig.mqh"

//+------------------------------------------------------------------+
//| Build portfolio status JSON string                                |
//+------------------------------------------------------------------+
string CertusBuildPortfolioStatusJSON()
{
   string json = "{";

   // Timestamp
   json += "\"timestamp\":\"" + CertusIsoNow() + "\",";

   // Portfolio (account info)
   json += "\"portfolio\":{";
   json += "\"id\":\"" + IntegerToString(AccountNumber()) + "\",";
   json += "\"name\":\"" + CertusEscapeJson(AccountName()) + "\",";
   json += "\"balance\":" + DoubleToString(AccountBalance(), 2) + ",";
   json += "\"equity\":" + DoubleToString(AccountEquity(), 2) + ",";
   json += "\"margin\":" + DoubleToString(AccountMargin(), 2) + ",";
   json += "\"freeMargin\":" + DoubleToString(AccountFreeMargin(), 2) + ",";
   json += "\"profit\":" + DoubleToString(AccountProfit(), 2);
   json += "},";

   // Strategies (grouped by magic number)
   json += "\"strategies\":[";

   string strategiesJson = CertusBuildStrategiesFromOrders();
   json += strategiesJson;

   json += "]";

   json += "}";
   return json;
}

//+------------------------------------------------------------------+
//| Scan all orders and group by magic number to build strategies    |
//+------------------------------------------------------------------+
string CertusBuildStrategiesFromOrders()
{
   // Collect unique magic numbers and their stats
   string result = "";
   int magicNumbers[];
   int magicCounts[];
   double magicProfits[];
   datetime magicLastTrade[];
   string magicNames[];
   int strategyCount = 0;

   // Scan open orders
   for(int i = 0; i < OrdersTotal(); i++)
   {
      if(!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
         continue;

      int magic = OrderMagicNumber();
      CertusAccumulateStrategy(magic, magicNumbers, magicCounts, magicProfits, magicLastTrade, magicNames, strategyCount, false);
   }

   // Scan history orders
   for(int i = 0; i < OrdersHistoryTotal(); i++)
   {
      if(!OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
         continue;

      int magic = OrderMagicNumber();
      CertusAccumulateStrategy(magic, magicNumbers, magicCounts, magicProfits, magicLastTrade, magicNames, strategyCount, true);
   }

   // Build JSON array
   for(int i = 0; i < strategyCount; i++)
   {
      if(i > 0) result += ",";

      bool hasOpenOrders = CertusHasOpenOrders(magicNumbers[i]);

      result += "{";
      result += "\"id\":\"" + IntegerToString(magicNumbers[i]) + "\",";
      result += "\"name\":\"" + CertusEscapeJson(magicNames[i]) + "\",";
      result += "\"active\":" + (hasOpenOrders ? "true" : "false") + ",";
      result += "\"profit\":" + DoubleToString(magicProfits[i], 2) + ",";
      result += "\"totalTrades\":" + IntegerToString(magicCounts[i]) + ",";
      result += "\"lastTradeTime\":\"" + CertusIsoTime(magicLastTrade[i]) + "\"";
      result += "}";
   }

   // If no orders found, add a "manual" entry for non-EA activity
   if(strategyCount == 0)
   {
      result += "{";
      result += "\"id\":\"0\",";
      result += "\"name\":\"Manual Trading\",";
      result += "\"active\":" + (OrdersTotal() > 0 ? "true" : "false") + ",";
      result += "\"profit\":" + DoubleToString(AccountProfit(), 2) + ",";
      result += "\"totalTrades\":" + IntegerToString(OrdersTotal()) + ",";
      result += "\"lastTradeTime\":\"" + CertusIsoNow() + "\"";
      result += "}";
   }

   return result;
}

//+------------------------------------------------------------------+
//| Accumulate stats for a magic number                               |
//+------------------------------------------------------------------+
void CertusAccumulateStrategy(int magic, int &magicNumbers[], int &counts[],
                              double &profits[], datetime &lastTrade[],
                              string &names[], int &count, bool isHistory)
{
   // Find existing entry or create new one
   int idx = -1;
   for(int i = 0; i < count; i++)
   {
      if(magicNumbers[i] == magic)
      {
         idx = i;
         break;
      }
   }

   if(idx == -1)
   {
      // New magic number - add entry
      idx = count;
      count++;

      // Resize arrays
      ArrayResize(magicNumbers, count);
      ArrayResize(counts, count);
      ArrayResize(profits, count);
      ArrayResize(lastTrade, count);
      ArrayResize(names, count);

      magicNumbers[idx] = magic;
      counts[idx] = 0;
      profits[idx] = 0;
      lastTrade[idx] = 0;
      names[idx] = CertusMagicToName(magic);
   }

   // Accumulate
   counts[idx]++;
   profits[idx] += OrderProfit() + OrderSwap() + OrderCommission();

   datetime orderTime = isHistory ? OrderCloseTime() : OrderOpenTime();
   if(orderTime > lastTrade[idx])
      lastTrade[idx] = orderTime;
}

//+------------------------------------------------------------------+
//| Convert magic number to readable name                            |
//+------------------------------------------------------------------+
string CertusMagicToName(int magic)
{
   if(magic == 0)
      return "Manual Trading";

   // Try to identify by magic number range
   if(magic >= 900000 && magic <= 999999)
      return "Certus EA";

   // Default: use magic number as identifier
   return "Strategy_" + IntegerToString(magic);
}

//+------------------------------------------------------------------+
//| Check if a magic number has open orders                          |
//+------------------------------------------------------------------+
bool CertusHasOpenOrders(int magic)
{
   for(int i = 0; i < OrdersTotal(); i++)
   {
      if(!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
         continue;

      if(OrderMagicNumber() == magic)
         return true;
   }
   return false;
}

#endif
