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

//+------------------------------------------------------------------+
//| Convert period to readable string                                 |
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
      default:         return "M" + IntegerToString(period);
   }
}

#endif
