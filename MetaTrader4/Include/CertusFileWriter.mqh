//+------------------------------------------------------------------+
//| CertusFileWriter.mqh - Atomic file write utilities               |
//+------------------------------------------------------------------+
#ifndef CERTUS_FILEWRITER_MQH
#define CERTUS_FILEWRITER_MQH

#include "CertusConfig.mqh"

//+------------------------------------------------------------------+
//| Ensure output directory exists                                    |
//+------------------------------------------------------------------+
void CertusEnsureDirectory()
{
   if(!FolderCreate(CertusOutputDir, FILE_COMMON))
   {
      // Directory may already exist, that's fine
   }
}

//+------------------------------------------------------------------+
//| Write content to file atomically (temp file + rename)            |
//+------------------------------------------------------------------+
bool CertusWriteFileAtomic(string filename, string content)
{
   string tmpFile = filename + CERTUS_TMP_SUFFIX;

   // Write to temporary file
   int handle = FileOpen(tmpFile, FILE_WRITE | FILE_TXT | FILE_COMMON);
   if(handle == INVALID_HANDLE)
   {
      if(CertusLogLevel >= 1)
         Print("[Certus] WARN: Cannot open temp file: ", tmpFile, " Error: ", GetLastError());
      return false;
   }

   FileWriteString(handle, content);
   FileClose(handle);

   // Delete target file if it exists
   if(FileIsExist(filename, FILE_COMMON))
   {
      if(!FileDelete(filename, FILE_COMMON))
      {
         if(CertusLogLevel >= 1)
            Print("[Certus] WARN: Cannot delete old file: ", filename, " Error: ", GetLastError());
         // Try to clean up temp file
         FileDelete(tmpFile, FILE_COMMON);
         return false;
      }
   }

   // Rename temp to target (atomic on most filesystems)
   if(!FileMove(tmpFile, FILE_COMMON, filename, FILE_COMMON))
   {
      if(CertusLogLevel >= 1)
         Print("[Certus] WARN: Cannot rename temp file: ", tmpFile, " -> ", filename, " Error: ", GetLastError());
      FileDelete(tmpFile, FILE_COMMON);
      return false;
   }

   return true;
}

//+------------------------------------------------------------------+
//| Append content to a file (for trades.json log)                   |
//+------------------------------------------------------------------+
bool CertusAppendFile(string filename, string content)
{
   int handle = FileOpen(filename, FILE_READ | FILE_WRITE | FILE_TXT | FILE_COMMON);
   if(handle == INVALID_HANDLE)
   {
      // File may not exist yet, create it
      handle = FileOpen(filename, FILE_WRITE | FILE_TXT | FILE_COMMON);
      if(handle == INVALID_HANDLE)
      {
         if(CertusLogLevel >= 1)
            Print("[Certus] WARN: Cannot create file: ", filename, " Error: ", GetLastError());
         return false;
      }
   }

   // Seek to end
   FileSeek(handle, 0, SEEK_END);
   FileWriteString(handle, content);
   FileClose(handle);
   return true;
}

//+------------------------------------------------------------------+
//| Read file content as string                                       |
//+------------------------------------------------------------------+
string CertusReadFile(string filename)
{
   if(!FileIsExist(filename, FILE_COMMON))
      return "";

   int handle = FileOpen(filename, FILE_READ | FILE_TXT | FILE_COMMON);
   if(handle == INVALID_HANDLE)
      return "";

   string content = "";
   while(!FileIsEnding(handle))
   {
      content += FileReadString(handle);
   }
   FileClose(handle);
   return content;
}

#endif
