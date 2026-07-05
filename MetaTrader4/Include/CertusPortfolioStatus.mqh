//+------------------------------------------------------------------+
//| CertusPortfolioStatus.mqh - Portfolio status JSON builder        |
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
   json += "\"name\":\"" + AccountName() + "\",";
   json += "\"balance\":" + DoubleToString(AccountBalance(), 2) + ",";
   json += "\"equity\":" + DoubleToString(AccountEquity(), 2) + ",";
   json += "\"margin\":" + DoubleToString(AccountMargin(), 2) + ",";
   json += "\"freeMargin\":" + DoubleToString(AccountFreeMargin(), 2) + ",";
   json += "\"profit\":" + DoubleToString(AccountProfit(), 2);
   json += "},";

   // Strategies (running EAs)
   json += "\"strategies\":[";
   json += CertusBuildStrategiesArray();
   json += "]";

   json += "}";
   return json;
}

//+------------------------------------------------------------------+
//| Build strategies array from running EAs                          |
//+------------------------------------------------------------------+
string CertusBuildStrategiesArray()
{
   string result = "";
   int count = 0;

   // Enumerate all charts to find running EAs
   for(int i = ChartsTotal() - 1; i >= 0; i--)
   {
      long chartId = ChartGetInteger(i, CHART_WINDOW_HANDLE);
      if(chartId == 0) continue;

      string symbol = ChartSymbol(chartId);
      int timeframe = (int)ChartGetInteger(chartId, CHART_PERIOD);

      // Check if our EA is running on this chart
      if(CertusIsEAOnChart(chartId))
      {
         if(count > 0) result += ",";

         string strategyId = CertusGetStrategyId(chartId);
         string strategyName = CertusGetStrategyName(chartId);
         int magicNumber = CertusGetMagicNumber(chartId);

         result += "{";
         result += "\"id\":\"" + strategyId + "\",";
         result += "\"name\":\"" + strategyName + "\",";
         result += "\"active\":true,";
         result += "\"profit\":" + DoubleToString(CertusGetStrategyProfit(chartId), 2) + ",";
         result += "\"totalTrades\":" + IntegerToString(CertusGetStrategyTradeCount(chartId)) + ",";
         result += "\"lastTradeTime\":\"" + CertusGetStrategyLastTradeTime(chartId) + "\"";
         result += "}";

         count++;
      }
   }

   // If no charts found, at least report this EA
   if(count == 0)
   {
      result += "{";
      result += "\"id\":\"" + CertusGetStrategyId(0) + "\",";
      result += "\"name\":\"" + CertusGetStrategyName(0) + "\",";
      result += "\"active\":true,";
      result += "\"profit\":" + DoubleToString(AccountProfit(), 2) + ",";
      result += "\"totalTrades\":" + IntegerToString(CertusGetStrategyTradeCount(0)) + ",";
      result += "\"lastTradeTime\":\"" + CertusGetStrategyLastTradeTime(0) + "\"";
      result += "}";
   }

   return result;
}

//+------------------------------------------------------------------+
//| Check if our EA is running on a specific chart                   |
//+------------------------------------------------------------------+
bool CertusIsEAOnChart(long chartId)
{
   // Check if any EA with our magic number pattern is running
   for(int i = 0; i < OrdersTotal(); i++)
   {
      if(OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
      {
         if(OrderMagicNumber() >= 900000 && OrderMagicNumber() <= 999999)
            return true;
      }
   }
   return false;
}

//+------------------------------------------------------------------+
//| Get strategy identifier for the current EA instance              |
//+------------------------------------------------------------------+
string CertusGetStrategyId(long chartId)
{
   return "EA_" + Symbol() + "_" + IntegerToString(Period()) + "_" + IntegerToString(AccountNumber());
}

//+------------------------------------------------------------------+
//| Get strategy display name                                        |
//+------------------------------------------------------------------+
string CertusGetStrategyName(long chartId)
{
   return "Certus EA - " + Symbol() + " " + CertusPeriodToString(Period());
}

//+------------------------------------------------------------------+
//| Get magic number for this EA instance                            |
//+------------------------------------------------------------------+
int CertusGetMagicNumber(long chartId)
{
   // Use a deterministic magic number based on symbol and timeframe
   return 900000 + (SymbolCRC32() % 100000);
}

//+------------------------------------------------------------------+
//| Calculate CRC32 of symbol name for unique identification         |
//+------------------------------------------------------------------+
int SymbolCRC32()
{
   string s = Symbol();
   int crc = 0;
   for(int i = 0; i < StringLen(s); i++)
   {
      crc += StringGetCharacter(s, i);
      crc = crc ^ (crc << 13);
      crc = crc ^ (crc >> 17);
      crc = crc ^ (crc << 5);
   }
   return MathAbs(crc);
}

//+------------------------------------------------------------------+
//| Get total profit for this strategy                               |
//+------------------------------------------------------------------+
double CertusGetStrategyProfit(long chartId)
{
   double profit = 0;
   int magic = CertusGetMagicNumber(chartId);

   for(int i = 0; i < OrdersTotal(); i++)
   {
      if(OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
      {
         if(OrderMagicNumber() == magic)
            profit += OrderProfit() + OrderSwap() + OrderCommission();
      }
   }
   return profit;
}

//+------------------------------------------------------------------+
//| Get trade count for this strategy                                |
//+------------------------------------------------------------------+
int CertusGetStrategyTradeCount(long chartId)
{
   int count = 0;
   int magic = CertusGetMagicNumber(chartId);

   // Count open trades
   for(int i = 0; i < OrdersTotal(); i++)
   {
      if(OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
      {
         if(OrderMagicNumber() == magic)
            count++;
      }
   }

   // Count history trades
   for(int i = 0; i < OrdersHistoryTotal(); i++)
   {
      if(OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
      {
         if(OrderMagicNumber() == magic)
            count++;
      }
   }

   return count;
}

//+------------------------------------------------------------------+
//| Get last trade time for this strategy                            |
//+------------------------------------------------------------------+
string CertusGetStrategyLastTradeTime(long chartId)
{
   datetime lastTime = 0;
   int magic = CertusGetMagicNumber(chartId);

   for(int i = 0; i < OrdersHistoryTotal(); i++)
   {
      if(OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
      {
         if(OrderMagicNumber() == magic && OrderCloseTime() > lastTime)
            lastTime = OrderCloseTime();
      }
   }

   if(lastTime == 0) lastTime = TimeCurrent();
   return CertusIsoTime(lastTime);
}

//+------------------------------------------------------------------+
//| Convert period to readable string                                |
//+------------------------------------------------------------------+
string CertusPeriodToString(int period)
{
   switch(period)
   {
      case PERIOD_M1:  return "M1";
      case PERIOD_M5:  return "M5";
      case PERIOD_M15: return "M15";
      case PERIOD_M30: return "M30";
      case PERIOD_H1:  return "H1";
      case PERIOD_H4:  return "H4";
      case PERIOD_D1:  return "D1";
      case PERIOD_W1:  return "W1";
      case PERIOD_MN1: return "MN1";
      default:         return "UNKNOWN";
   }
}

#endif
