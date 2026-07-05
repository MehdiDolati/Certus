//+------------------------------------------------------------------+
//| CertusEA.mq4 - MetaTrader 4 Expert Advisor for Certus Platform  |
//|                                                                    |
//| Collects strategy data and writes to shared JSON files that       |
//| Certus reads via FileSystemWatcher.                               |
//|                                                                    |
//| Files written:                                                    |
//|   portfolio_status.json - Account + strategy status (overwritten) |
//|   trades.json          - Trade events (append-only)              |
//+------------------------------------------------------------------+
#property copyright "Certus Platform"
#property link      ""
#property version   "1.00"
#property strict

#include "CertusConfig.mqh"
#include "CertusFileWriter.mqh"
#include "CertusDateTime.mqh"
#include "CertusPortfolioStatus.mqh"
#include "CertusTradeLogger.mqh"

//--- Timer tracking
static datetime CertusLastUpdateTime = 0;

//+------------------------------------------------------------------+
//| Expert initialization function                                    |
//+------------------------------------------------------------------+
int OnInit()
{
   Print("[Certus] EA initialized on ", Symbol(), " ", CertusPeriodToString(Period()));
   Print("[Certus] Account: ", AccountNumber(), " - ", AccountName());
   Print("[Certus] Output directory: ", CertusOutputDir);
   Print("[Certus] Update interval: ", CertusUpdateSeconds, "s");

   // Create output directory
   CertusEnsureDirectory();

   // Initialize trade tracking
   CertusInitTradeTracking();

   // Set timer for periodic updates
   EventSetTimer(CertusUpdateSeconds);

   // Initial portfolio status write
   CertusLastUpdateTime = TimeCurrent() - CertusUpdateSeconds; // Force immediate write
   CertusUpdatePortfolioStatus();

   Print("[Certus] EA ready. Writing to: ", CertusOutputDir);
   return INIT_SUCCEEDED;
}

//+------------------------------------------------------------------+
//| Expert deinitialization function                                  |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
   EventKillTimer();
   Print("[Certus] EA deinitialized. Reason: ", reason);
}

//+------------------------------------------------------------------+
//| Timer function - periodic portfolio status update                |
//+------------------------------------------------------------------+
void OnTimer()
{
   CertusUpdatePortfolioStatus();
}

//+------------------------------------------------------------------+
//| Tick function - check for trade events                           |
//+------------------------------------------------------------------+
void OnTick()
{
   // Check for trade events on every tick
   CertusCheckAndLogTrades();
}

//+------------------------------------------------------------------+
//| Trade event handler (called when order状态 changes)              |
//+------------------------------------------------------------------+
void OnTradeTransaction(const MqlTradeTransaction &trans,
                        const MqlTradeRequest &request,
                        const MqlTradeResult &result)
{
   if(!CertusLogTrades) return;

   // Log trade events
   switch(trans.type)
   {
      case TRADE_TRANSACTION_DEAL_ADD:
         // New deal executed
         {
            string event = CertusBuildTradeEventJSON("close");
            if(event != "")
            {
               CertusAppendFile(CertusOutputDir + "/trades.json", event + "\n");
               if(CertusLogLevel >= 2)
                  Print("[Certus] Trade logged: deal added");
            }
         }
         break;

      case TRADE_TRANSACTION_ORDER_ADD:
         // New order placed
         {
            string event = CertusBuildTradeEventJSON("open");
            if(event != "")
            {
               CertusAppendFile(CertusOutputDir + "/trades.json", event + "\n");
               if(CertusLogLevel >= 2)
                  Print("[Certus] Trade logged: order added");
            }
         }
         break;

      case TRADE_TRANSACTION_ORDER_UPDATE:
         // Order modified (SL/TP change)
         {
            string event = CertusBuildTradeEventJSON("modify");
            if(event != "")
            {
               CertusAppendFile(CertusOutputDir + "/trades.json", event + "\n");
               if(CertusLogLevel >= 2)
                  Print("[Certus] Trade logged: order modified");
            }
         }
         break;
   }
}

//+------------------------------------------------------------------+
//| Update portfolio status file                                      |
//+------------------------------------------------------------------+
void CertusUpdatePortfolioStatus()
{
   datetime now = TimeCurrent();
   if(now - CertusLastUpdateTime < CertusUpdateSeconds)
      return;

   string json = CertusBuildPortfolioStatusJSON();
   string filename = CertusOutputDir + "/portfolio_status.json";

   if(CertusWriteFileAtomic(filename, json))
   {
      CertusLastUpdateTime = now;
      if(CertusLogLevel >= 3)
         Print("[Certus] Portfolio status updated: ", filename);
   }
   else
   {
      if(CertusLogLevel >= 1)
         Print("[Certus] WARN: Failed to write portfolio status");
   }
}

//+------------------------------------------------------------------+
