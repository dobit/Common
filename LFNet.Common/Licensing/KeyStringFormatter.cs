using System.Text;

namespace LFNet.Licensing
{
    public class KeyStringFormatter
    {
        public const int KeyLineWidth = 50;
        public static readonly string OpeningKeySymbols = new string('-', 50);
        public static readonly string ClosingKeySymbols = new string('-', 50);
        public static readonly char[] ReplaceSymbols = new char[]
                                                           {
                                                               '\n',
                                                               '\r',
                                                               '\t',
                                                               ' ',
                                                               '-'
                                                           };
        public static string FormatKey(string pureKey)
        {
            StringBuilder stringBuilder = new StringBuilder(pureKey.Length);
            stringBuilder.Append(KeyStringFormatter.OpeningKeySymbols);
            stringBuilder.Append("\r\n");
            int num = 0;
            while (num + 50 <= pureKey.Length)
            {
                stringBuilder.Append(pureKey.Substring(num, 50));
                stringBuilder.Append("\r\n");
                num += 50;
            }
            stringBuilder.Append(pureKey.Substring(num, pureKey.Length % 50));
            stringBuilder.Append("\r\n");
            stringBuilder.Append(KeyStringFormatter.ClosingKeySymbols);
            return stringBuilder.ToString();
        }
        private static string RemoveChars(string str, char[] charsToRemove)
        {
            StringBuilder stringBuilder = new StringBuilder(str);
            for (int i = 0; i < charsToRemove.Length; i++)
            {
                char c = charsToRemove[i];
                stringBuilder.Replace(c.ToString(), "");
            }
            return stringBuilder.ToString();
        }
        public static string ParseKey(string formattedKey)
        {
            return KeyStringFormatter.RemoveChars(formattedKey, KeyStringFormatter.ReplaceSymbols);
        }
    }
}