using System.Diagnostics;

namespace LFNet.Common.Helpers
{
    public static class DebugHelper
    {
        private const string DEBUG_IDENTIFIER = "#CST#";

        /// <summary>
        /// Writes the object to the Debug console, prefixed with an identifier for debugView filtering
        /// </summary>
        /// <param name="o">Object to write</param>
        public static void Log(object o)
        {
            Debug.Write(o);
        }

        /// <summary>
        /// Writes the object on a new line to the Debug console, prefixed with an identifier for debugView filtering.
        /// </summary>
        /// <param name="o">Object to write</param>
        public static void LogLine(object o)
        {
            Debug.WriteLine(o);
        }
    }
}

