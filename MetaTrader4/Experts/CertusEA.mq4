//+------------------------------------------------------------------+
//| CertusEA.mq4 - MetaTrader 4 Expert Advisor for Certus Platform  |
//|                                                                    |
//| SINGLE EA on ANY chart - monitors ALL trading activity.          |
//| Reads all orders via OrderSelect (MODE_TRADES/MODE_HISTORY).     |
//| Groups orders by magic number to identify strategies.            |
//|                                                                    |
//| Files written:                                                    |
//|   portfolio_status.json - Account + strategy status (overwritten) |
//|   trades.json          - Trade events (append-only)              |
//+------------------------------------------------------------------+
#property copyright "Certus Platform"
#property link      ""
#property version   "1.01"
#property strict

#include "../Include/CertusConfig.mqh"
#include "../Include/CertusFileWriter.mqh"
#include "../Include/CertusDateTime.mqh"
#include "../Include/CertusPortfolioStatus.mqh"
#include "../Include/CertusTradeLogger.mqh"

//--- Timer tracking
static datetime CertusLastUpdateTime = 0;

//+------------------------------------------------------------------+
//| Expert initialization function                                    |
//+------------------------------------------------------------------+
int OnInit()
{
   Print("[Certus] EA initialized on ", Symbol(), " ", CertusPeriodToString(Period()));
   Print("[Certus] Account: ", AccountNumber(), " - ", AccountName());
   Print("[Certus] Monitoring ALL orders across the account");
   Print("[Certus] Output directory: ", CertusOutputDir);
   Print("[Certus] Update interval: ", CertusUpdateSeconds, "s");

   // Create output directory
   CertusEnsureDirectory();

   // Initialize trade tracking
   CertusInitTradeTracking();

   // Set timer for periodic updates
   EventSetTimer(CertusUpdateSeconds);

   // Initial portfolio status write
   CertusLastUpdateTime = TimeCurrent() - CertusUpdateSeconds;
   CertusUpdatePortfolioStatus();

   Print("[Certus] EA ready. Attach to any chart - monitors entire account.");
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
   CertusCheckAndLogTrades();
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
         Print("[Certus] Portfolio status updated");
   }
   else
   {
      if(CertusLogLevel >= 1)
         Print("[Certus] WARN: Failed to write portfolio status");
   }
}

//+------------------------------------------------------------------+
