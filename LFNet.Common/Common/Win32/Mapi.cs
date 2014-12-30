using System;
using System.Runtime.InteropServices;
using System.Security;

namespace LFNet.Common.Win32
{
    /// <summary>
    /// Internal class for calling MAPI APIs
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class Mapi
    {
        public const int MAPI_DIALOG = 8;
        public const int MAPI_E_AMBIGUOUS_RECIPIENT = 0x15;
        public const int MAPI_E_ATTACHMENT_NOT_FOUND = 11;
        public const int MAPI_E_ATTACHMENT_OPEN_FAILURE = 12;
        public const int MAPI_E_ATTACHMENT_WRITE_FAILURE = 13;
        public const int MAPI_E_BAD_RECIPTYPE = 15;
        public const int MAPI_E_BLK_TOO_SMALL = 6;
        public const int MAPI_E_DISK_FULL = 4;
        public const int MAPI_E_FAILURE = 2;
        public const int MAPI_E_INSUFFICIENT_MEMORY = 5;
        public const int MAPI_E_INVALID_EDITFIELDS = 0x18;
        public const int MAPI_E_INVALID_MESSAGE = 0x11;
        public const int MAPI_E_INVALID_PARAMETER = 0x3e6;
        public const int MAPI_E_INVALID_RECIPS = 0x19;
        public const int MAPI_E_INVALID_SESSION = 0x13;
        public const int MAPI_E_LOGIN_FAILURE = 3;
        public const int MAPI_E_MESSAGE_IN_USE = 0x16;
        public const int MAPI_E_NETWORK_FAILURE = 0x17;
        public const int MAPI_E_NO_LIBRARY = 0x3e7;
        public const int MAPI_E_NO_MESSAGES = 0x10;
        public const int MAPI_E_NOT_SUPPORTED = 0x1a;
        public const int MAPI_E_TEXT_TOO_LARGE = 0x12;
        public const int MAPI_E_TOO_MANY_FILES = 9;
        public const int MAPI_E_TOO_MANY_RECIPIENTS = 10;
        public const int MAPI_E_TOO_MANY_SESSIONS = 8;
        public const int MAPI_E_TYPE_NOT_SUPPORTED = 20;
        public const int MAPI_E_UNKNOWN_RECIPIENT = 14;
        public const int MAPI_LOGON_UI = 1;
        public const int MAPI_USER_ABORT = 1;
        public const int SUCCESS_SUCCESS = 0;

        /// <summary>
        /// Gets the error message from the specified error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns>The error message from the error code.</returns>
        public static string GetError(int errorCode)
        {
            switch (errorCode)
            {
                case 1:
                    return "User Aborted.";

                case 2:
                    return "MAPI Failure.";

                case 3:
                    return "Login Failure.";

                case 4:
                    return "MAPI Disk full.";

                case 5:
                    return "MAPI Insufficient memory.";

                case 6:
                    return "MAPI Block too small.";

                case 8:
                    return "MAPI Too many sessions.";

                case 9:
                    return "MAPI too many files.";

                case 10:
                    return "MAPI too many recipients.";

                case 11:
                    return "MAPI Attachment not found.";

                case 12:
                    return "MAPI Attachment open failure.";

                case 13:
                    return "MAPI Attachment Write Failure.";

                case 14:
                    return "MAPI Unknown recipient.";

                case 15:
                    return "MAPI Bad recipient type.";

                case 0x10:
                    return "MAPI No messages.";

                case 0x11:
                    return "MAPI Invalid message.";

                case 0x12:
                    return "MAPI Text too large.";

                case 0x13:
                    return "MAPI Invalid session.";

                case 20:
                    return "MAPI Type not supported.";

                case 0x15:
                    return "MAPI Ambiguous recipient.";

                case 0x16:
                    return "MAPI Message in use.";

                case 0x17:
                    return "MAPI Network failure.";

                case 0x18:
                    return "MAPI Invalid edit fields.";

                case 0x19:
                    return "MAPI Invalid Recipients.";

                case 0x1a:
                    return "MAPI Not supported.";

                case 0x3e6:
                    return "MAPI Invalid parameter.";

                case 0x3e7:
                    return "MAPI No Library.";
            }
            return string.Empty;
        }

        [DllImport("MAPI32.DLL", CharSet=CharSet.Ansi)]
        public static extern int MAPILogon(IntPtr hwnd, string prf, string pw, int flg, int rsv, ref IntPtr sess);
        [DllImport("MAPI32.DLL", CharSet=CharSet.Ansi)]
        public static extern int MAPISendMail(IntPtr session, IntPtr hwnd, MapiMessage message, int flg, int rsv);

        [StructLayout(LayoutKind.Sequential)]
        public class MapiFileDescriptor
        {
            public int Reserved;
            public int Flags;
            public int Position;
            public string PathName;
            public string FileName;
            public int FileType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MapiMessage
        {
            public int Reserved;
            public string Subject;
            public string NoteText;
            public string MessageType;
            public string DateReceived;
            public string ConversationID;
            public int Flags;
            public IntPtr Originator;
            public int RecipientCount;
            public IntPtr Recipients;
            public int FileCount;
            public IntPtr Files;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MapiRecipDesc
        {
            public int Reserved;
            public int RecipientClass;
            public string Name;
            public string Address;
            public int EIDSize;
            public IntPtr EntryID;
        }
    }
}

