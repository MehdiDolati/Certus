//+------------------------------------------------------------------+
//| CertusDateTime.mqh - ISO 8601 date/time utilities               |
//+------------------------------------------------------------------+
#ifndef CERTUS_DATETIME_MQH
#define CERTUS_DATETIME_MQH

//+------------------------------------------------------------------+
//| Get current time as ISO 8601 string                              |
//+------------------------------------------------------------------+
string CertusIsoNow()
{
   return CertusIsoTime(TimeCurrent());
}

//+------------------------------------------------------------------+
//| Convert datetime to ISO 8601 string (UTC)                        |
//+------------------------------------------------------------------+
string CertusIsoTime(datetime dt)
{
   if(dt == 0) dt = TimeCurrent();

   TimeGMT(dt); // Ensure UTC

   int year   = TimeYear(dt);
   int month  = TimeMonth(dt);
   int day    = TimeDay(dt);
   int hour   = TimeHour(dt);
   int minute = TimeMinute(dt);
   int sec    = TimeSeconds(dt);

   string result = IntegerToString(year) + "-";
   result += (month < 10 ? "0" : "") + IntegerToString(month) + "-";
   result += (day < 10 ? "0" : "") + IntegerToString(day) + "T";
   result += (hour < 10 ? "0" : "") + IntegerToString(hour) + ":";
   result += (minute < 10 ? "0" : "") + IntegerToString(minute) + ":";
   result += (sec < 10 ? "0" : "") + IntegerToString(sec) + "Z";

   return result;
}

#endif
