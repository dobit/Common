using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace LFNet.Licensing
{
    public class LicenseConverter
    {
        private const char PlainLicenseKeySymbol = 'P';
        private const char OldLicenseKeySymbol = 'C';
        private const char NewLicenseKeySymbol = 'N';
        public static string LicenseToKey(IEncoder encoder, License license)
        {
            NewLicenseSerializer newLicenseSerializer = new NewLicenseSerializer();
            string arg = newLicenseSerializer.Serialize(license, encoder);
            return 'N' + arg;
        }
        public static string LicenseToKeyOld(IEncoder encoder, License license)
        {
            OldLicenseSerializer oldLicenseSerializer = new OldLicenseSerializer();
            string arg = oldLicenseSerializer.Serialize(license, encoder);
            return 'C' + arg;
        }
        public static License KeyToLicenseUnsafe(IDecoder decoder, string key)
        {
            char c;
            string text = LicenseConverter.ExtractKeyBody(key, out c);
            if (text == null)
            {
                return null;
            }
            char c2 = c;
            if (c2 != 'C')
            {
                switch (c2)
                {
                    case 'N':
                        {
                            NewLicenseSerializer newLicenseSerializer = new NewLicenseSerializer();
                            return newLicenseSerializer.Deserialize(text, decoder);
                        }
                    case 'P':
                        {
                            PlainLicenseSerializer plainLicenseSerializer = new PlainLicenseSerializer();
                            return plainLicenseSerializer.Deserialize(text);
                        }
                }
                return null;
            }
            OldLicenseSerializer oldLicenseSerializer = new OldLicenseSerializer();
            return oldLicenseSerializer.Deserialize(text, decoder);
        }
        public static License KeyToLicense(IDecoder decoder, string key)
        {
            try
            {
                return LicenseConverter.KeyToLicenseUnsafe(decoder, key);
            }
            catch (Exception ex)
            {
                Log.ReportException(ex);
            }
            return null;
        }
        private static string ExtractKeyBody(string key, out char keyType)
        {
            keyType = ' ';
            if (key == null || key.Length < 1)
            {
                return null;
            }
            keyType = char.ToUpper(key[0]);
            string text = key.Substring(1);
            if (text.Length == 0)
            {
                return null;
            }
            return text;
        }
    }

    public sealed class Log
    {
        private static object locks = new object();
        private static string logFileName = null;
        private static TextWriter defaultLogWriter = null;
        private static EventLog eventLog = null;
        private static int exceptionCounter = 0;
        public static string LogFileName
        {
            get
            {
                return Log.logFileName;
            }
        }
        public static int ExceptionCounter
        {
            get
            {
                return Log.exceptionCounter;
            }
        }
        public static void SetWriter(TextWriter writer)
        {
            Log.defaultLogWriter = writer;
        }
        private static void Write(string message, params object[] args)
        {
            string text = string.Format(message, args);
            object obj;
            Monitor.Enter(obj = Log.locks);
            try
            {
                if (Log.defaultLogWriter != null)
                {
                    Log.defaultLogWriter.Write(text);
                    Log.defaultLogWriter.Flush();
                }
                else
                {
                    if (Log.logFileName == null)
                    {
                        Log.logFileName = Log.GetLogFileName();
                        text = Log.GetLogHeader() + text;
                    }
                    using (TextWriter textWriter = new StreamWriter(Log.logFileName, true))
                    {
                        textWriter.Write(text);
                        textWriter.Flush();
                        textWriter.Close();
                    }
                }
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }
        private static void WriteEventLog(string message, EventLogEntryType type)
        {
            try
            {
                object obj;
                Monitor.Enter(obj = Log.locks);
                try
                {
                    if (Log.eventLog == null)
                    {
                        Log.eventLog = new EventLog("Application", ".", "VisualSVN");
                    }
                    Log.eventLog.WriteEntry(message, type, 1000);
                }
                finally
                {
                    Monitor.Exit(obj);
                }
            }
            catch (Exception)
            {
            }
        }
        public static void Info(string message, params object[] args)
        {
            string text = string.Format(message, args);
            Log.Write("{0}\r\n", new object[]
			{
				text
			});
            Log.WriteEventLog(text, EventLogEntryType.Information);
        }
        public static void Error(string message, params object[] args)
        {
            Log.ErrorUnformatted(string.Format(message, args));
        }
        public static void ErrorUnformatted(string text)
        {
            Log.Write("ERROR: {0}\r\n", new object[]
			{
				text
			});
            Log.WriteEventLog(text, EventLogEntryType.Error);
        }
        public static void ReportException(Exception ex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (ex != null)
            {
                stringBuilder.AppendFormat("Unexpected exception: {0}\r\n{1}\r\n", ex.Message, ex.StackTrace);
                ex = ex.InnerException;
            }
            stringBuilder.AppendFormat("StackTrace:\r\n{0}\r\n", Environment.StackTrace);
            string text = stringBuilder.ToString();
            Log.Write("{0}", new object[]
			{
				text
			});
            Log.WriteEventLog(text, EventLogEntryType.Error);
            Log.exceptionCounter++;
        }
        public static void ResetExceptionCounter()
        {
            Log.exceptionCounter = 0;
        }
        private static string GetLogFileName()
        {
            DateTime now = DateTime.Now;
            string path = string.Format("LFNet-{0:0000}-{1:00}-{2:00}-{3:00}-{4:00}-{5}.log", new object[]
			{
				now.Year,
				now.Month,
				now.Day,
				now.Hour,
				now.Minute,
				Process.GetCurrentProcess().Id
			});
            return Path.Combine(Path.GetTempPath(), path);
        }
        private static string GetVersion()
        {
            Assembly assembly = typeof(Log).Assembly;
            Attribute customAttribute = Attribute.GetCustomAttribute(assembly, typeof(ProductVersionAttribute));
            return ((ProductVersionAttribute)customAttribute).Version;
        }
        private static string GetLogHeader()
        {
            return "LFNet " + Log.GetVersion() + Environment.NewLine;
        }
        [Conditional("DEBUG")]
        private static void DebuggerBreak()
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }
    }
}