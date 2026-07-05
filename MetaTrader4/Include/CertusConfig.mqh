//+------------------------------------------------------------------+
//| CertusConfig.mqh - Configuration constants and input parameters  |
//+------------------------------------------------------------------+
#ifndef CERTUS_CONFIG_MQH
#define CERTUS_CONFIG_MQH

//--- Output directory (relative to MT4 Files/ folder)
input string   CertusOutputDir       = "Certus";

//--- Update interval in seconds for portfolio_status.json
input int      CertusUpdateSeconds   = 5;

//--- Enable trade logging to trades.json
input bool     CertusLogTrades       = true;

//--- Log level: 0=ERROR, 1=WARN, 2=INFO, 3=DEBUG
input int      CertusLogLevel        = 2;

//--- File suffix for atomic writes
#define CERTUS_TMP_SUFFIX ".tmp"

#endif
