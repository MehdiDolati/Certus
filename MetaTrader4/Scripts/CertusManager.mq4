//+------------------------------------------------------------------+
//|                                              CertusManager.mq4   |
//|                                  Certus Auto-Activation Script    |
//|                                  Reads certus_activation.json     |
//|                                  and attaches EAs to charts        |
//+------------------------------------------------------------------+
#property copyright "Certus Trading Platform"
#property link      ""
#property version   "1.00"
#property strict

#include <Json.mqh>

//+------------------------------------------------------------------+
//| Script initialization function                                     |
//+------------------------------------------------------------------+
int OnInit()
{
    Print("[CertusManager] Starting auto-activation...");

    // Read the activation config
    string configPath = "Certus/certus_activation.json";
    string configContent = ReadFile(configPath);

    if (configContent == "")
    {
        Print("[CertusManager] ERROR: Could not read config from ", configPath);
        Print("[CertusManager] Please ensure certus_activation.json exists in Files/Certus/");
        return(INIT_FAILED);
    }

    // Parse the JSON config
    if (!ParseConfig(configContent))
    {
        Print("[CertusManager] ERROR: Failed to parse activation config");
        return(INIT_FAILED);
    }

    Print("[CertusManager] Activation complete. Check Experts tab for details.");
    return(INIT_SUCCEEDED);
}

//+------------------------------------------------------------------+
//| Read file content                                                  |
//+------------------------------------------------------------------+
string ReadFile(string filename)
{
    int handle = FileOpen(filename, FILE_READ | FILE_TXT | FILE_ANSI);
    if (handle == INVALID_HANDLE)
    {
        Print("[CertusManager] Cannot open file: ", filename);
        return("");
    }

    string content = "";
    while (!FileIsEnding(handle))
    {
        content += FileReadString(handle) + "\n";
    }
    FileClose(handle);
    return(content);
}

//+------------------------------------------------------------------+
//| Parse config and activate EAs                                      |
//+------------------------------------------------------------------+
bool ParseConfig(string json)
{
    // Simple JSON parsing for the activation config
    // Expected format: { "eas": [ { "name": "...", "symbol": "...", "timeframe": "...", "parameters": {} } ] }

    int easIndex = StringFind(json, "\"eas\"");
    if (easIndex == -1)
    {
        Print("[CertusManager] ERROR: 'eas' array not found in config");
        return(false);
    }

    // Find the opening bracket of the eas array
    int arrayStart = StringFind(json, "[", easIndex);
    if (arrayStart == -1)
    {
        Print("[CertusManager] ERROR: Cannot find eas array start");
        return(false);
    }

    // Find the closing bracket
    int arrayEnd = FindMatchingBracket(json, arrayStart);
    if (arrayEnd == -1)
    {
        Print("[CertusManager] ERROR: Cannot find eas array end");
        return(false);
    }

    string easArray = StringSubstr(json, arrayStart + 1, arrayEnd - arrayStart - 1);

    // Process each EA object
    int pos = 0;
    int eaCount = 0;

    while (pos < StringLen(easArray))
    {
        // Find next EA object
        int objStart = StringFind(easArray, "{", pos);
        if (objStart == -1) break;

        int objEnd = FindMatchingBracket(easArray, objStart);
        if (objEnd == -1) break;

        string eaJson = StringSubstr(easArray, objStart + 1, objEnd - objStart - 1);
        ActivateEA(eaJson);
        eaCount++;

        pos = objEnd + 1;
    }

    Print("[CertusManager] Processed ", eaCount, " EA(s)");
    return(true);
}

//+------------------------------------------------------------------+
//| Find matching bracket                                              |
//+------------------------------------------------------------------+
int FindMatchingBracket(string text, int startPos)
{
    if (startPos >= StringLen(text)) return(-1);

    string ch = StringSubstr(text, startPos, 1);
    string match = (ch == "{") ? "}" : (ch == "[") ? "]" : "";
    if (match == "") return(-1);

    int depth = 1;
    for (int i = startPos + 1; i < StringLen(text); i++)
    {
        string c = StringSubstr(text, i, 1);
        if (c == ch) depth++;
        if (c == match)
        {
            depth--;
            if (depth == 0) return(i);
        }
    }
    return(-1);
}

//+------------------------------------------------------------------+
//| Activate a single EA                                               |
//+------------------------------------------------------------------+
void ActivateEA(string eaJson)
{
    string name = GetJsonValue(eaJson, "name");
    string symbol = GetJsonValue(eaJson, "symbol");
    string timeframe = GetJsonValue(eaJson, "timeframe");

    if (name == "")
    {
        Print("[CertusManager] WARNING: EA name is empty, skipping");
        return;
    }

    if (symbol == "") symbol = "EURUSD";
    if (timeframe == "") timeframe = "H1";

    int tf = StringToTimeframe(timeframe);

    Print("[CertusManager] Attaching ", name, " to ", symbol, " ", timeframe);

    // Open a new chart for this EA
    long chartId = ChartOpen(symbol, tf);
    if (chartId == 0)
    {
        Print("[CertusManager] ERROR: Failed to open chart for ", symbol);
        return;
    }

    // Wait for chart to be ready
    Sleep(500);

    // Attach the EA to the chart
    int result = ChartSetExpert(chartId, 0, name, "");

    if (result == -1)
    {
        Print("[CertusManager] ERROR: Failed to attach ", name, " to ", symbol);
        Print("[CertusManager] Make sure the EA is installed in MQL4/Experts/");
    }
    else
    {
        Print("[CertusManager] Successfully attached ", name, " to ", symbol);
    }
}

//+------------------------------------------------------------------+
//| Get value from simple JSON object                                  |
//+------------------------------------------------------------------+
string GetJsonValue(string json, string key)
{
    string searchKey = "\"" + key + "\"";
    int keyIndex = StringFind(json, searchKey);
    if (keyIndex == -1) return("");

    // Find the colon after the key
    int colonIndex = StringFind(json, ":", keyIndex + StringLen(searchKey));
    if (colonIndex == -1) return("");

    // Find the value start (skip whitespace)
    int valueStart = colonIndex + 1;
    while (valueStart < StringLen(json) && StringSubstr(json, valueStart, 1) == " ")
        valueStart++;

    if (valueStart >= StringLen(json)) return("");

    string valueChar = StringSubstr(json, valueStart, 1);

    if (valueChar == "\"")
    {
        // String value
        int valueEnd = StringFind(json, "\"", valueStart + 1);
        if (valueEnd == -1) return("");
        return(StringSubstr(json, valueStart + 1, valueEnd - valueStart - 1));
    }
    else
    {
        // Number or other value
        int valueEnd = valueStart;
        while (valueEnd < StringLen(json))
        {
            string c = StringSubstr(json, valueEnd, 1);
            if (c == "," || c == "}" || c == "]" || c == " ") break;
            valueEnd++;
        }
        return(StringSubstr(json, valueStart, valueEnd - valueStart));
    }
}

//+------------------------------------------------------------------+
//| Convert timeframe string to constant                               |
//+------------------------------------------------------------------+
int StringToTimeframe(string tf)
{
    if (tf == "M1")  return(PERIOD_M1);
    if (tf == "M5")  return(PERIOD_M5);
    if (tf == "M15") return(PERIOD_M15);
    if (tf == "M30") return(PERIOD_M30);
    if (tf == "H1")  return(PERIOD_H1);
    if (tf == "H4")  return(PERIOD_H4);
    if (tf == "D1")  return(PERIOD_D1);
    if (tf == "W1")  return(PERIOD_W1);
    if (tf == "MN1") return(PERIOD_MN1);
    return(PERIOD_H1); // Default
}
//+------------------------------------------------------------------+
