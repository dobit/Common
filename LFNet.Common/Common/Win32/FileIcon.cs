using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LFNet.Common.Win32
{
    /// <summary>
    /// Defines a set of utility methods for extracting icons for files and file extensions.
    /// </summary>
    public static class FileIcon
    {
        /// <summary>
        /// Retrieve the handle to the icon that represents the file and the index of the icon within the system image list. The handle is copied to the hIcon member of the structure specified by psfi, and the index is copied to the iIcon member.
        /// </summary>
        internal const uint SHGFI_ICON = 0x100;
        /// <summary>
        /// Modify SHGFI_ICON, causing the function to retrieve the file's large icon. The SHGFI_ICON flag must also be set.
        /// </summary>
        internal const uint SHGFI_LARGEICON = 0;
        /// <summary>
        /// Modify SHGFI_ICON, causing the function to retrieve the file's small icon. Also used to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system image list that contains small icon images. The SHGFI_ICON and/or SHGFI_SYSICONINDEX flag must also be set.
        /// </summary>
        internal const uint SHGFI_SMALLICON = 1;

        /// <summary>
        /// Creates an array of handles to large or small icons extracted from the specified executable file, DLL, or icon file. 
        /// </summary>
        /// <param name="libName">The name of an executable file, DLL, or icon file from which icons will be extracted.</param>
        /// <param name="iconIndex">The zero-based index of the first icon to extract. If this value is a negative number and either phiconLarge or phiconSmall is not NULL, the function begins by extracting the icon whose resource identifier is equal to the absolute value of nIconIndex. For example, use -3 to extract the icon whose resource identifier is 3.</param>
        /// <param name="largeIcon">An array of icon handles that receives handles to the large icons extracted from the file. If this parameter is NULL, no large icons are extracted from the file.</param>
        /// <param name="smallIcon">An array of icon handles that receives handles to the small icons extracted from the file. If this parameter is NULL, no small icons are extracted from the file.</param>
        /// <param name="nIcons">The number of icons to be extracted from the file.</param>
        /// <returns>If the nIconIndex parameter is -1, the phiconLarge parameter is NULL, and the phiconSmall  parameter is NULL, then the return value is the number of icons contained in the specified file. Otherwise, the return value is the number of icons successfully extracted from the file.</returns>
        [DllImport("Shell32.dll")]
        private static extern int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, uint nIcons);
        /// <summary>
        /// Returns the default icon representation for files with the specified extension.
        /// </summary>
        /// <param name="extension">File extension (including the leading period).</param>
        /// <param name="size">The desired size of the icon.</param>
        /// <returns>The default icon for files with the specified extension.</returns>
        public static Icon FromExtension(string extension, ShellIconSize size)
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension);
            if (key != null)
            {
                string name = Convert.ToString(key.GetValue(null));
                RegistryKey key2 = Registry.ClassesRoot.OpenSubKey(name);
                if (key2 == null)
                {
                    return null;
                }
                RegistryKey key3 = key2.OpenSubKey("DefaultIcon");
                if (key3 == null)
                {
                    RegistryKey key4 = key2.OpenSubKey("CLSID");
                    if (key4 == null)
                    {
                        return null;
                    }
                    key3 = Registry.ClassesRoot.OpenSubKey((@"CLSID\" + Convert.ToString(key4.GetValue(null))) + @"\DefaultIcon");
                    if (key3 == null)
                    {
                        return null;
                    }
                }
                string[] strArray = Convert.ToString(key3.GetValue(null)).Split(new char[] { ',' });
                int iconIndex = (strArray.Length > 1) ? int.Parse(strArray[1]) : 0;
                IntPtr[] ptrArray = new IntPtr[1];
                if (ExtractIconEx(strArray[0], iconIndex, (size == ShellIconSize.LargeIcon) ? ptrArray : null, (size == ShellIconSize.SmallIcon) ? ptrArray : null, 1) > 0)
                {
                    return Icon.FromHandle(ptrArray[0]);
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an icon representation of the specified file.
        /// </summary>
        /// <param name="filename">The path to the file.</param>
        /// <param name="size">The desired size of the icon.</param>
        /// <returns>An icon that represents the file.</returns>
        public static Icon FromFile(string filename, ShellIconSize size)
        {
            SHFILEINFO psfi = new SHFILEINFO();
            SHGetFileInfo(filename, 0, ref psfi, (uint) Marshal.SizeOf(psfi), size);
            return Icon.FromHandle(psfi.hIcon);
        }

        /// <summary>
        /// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
        /// </summary>
        /// <param name="pszPath">A pointer to a null-terminated string of maximum length MAX_PATH that contains the path and file name. Both absolute and relative paths are valid.</param>
        /// <param name="dwFileAttributes">A combination of one or more file attribute flags (FILE_ATTRIBUTE_ values as defined in Winnt.h).</param>
        /// <param name="psfi">The address of a SHFILEINFO structure to receive the file information.</param>
        /// <param name="cbSizeFileInfo">The size, in bytes, of the SHFILEINFO structure pointed to by the psfi parameter.</param>
        /// <param name="uFlags">The flags that specify the file information to retrieve.</param>
        /// <returns>Nonzero if successful, or zero otherwise.</returns>
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, ShellIconSize uFlags);

        /// <summary>
        /// Contains information about a file object.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            /// <summary>
            /// A handle to the icon that represents the file.
            /// </summary>
            public IntPtr hIcon;
            /// <summary>
            /// The index of the icon image within the system image list.
            /// </summary>
            public IntPtr iIcon;
            /// <summary>
            /// An array of values that indicates the attributes of the file object.
            /// </summary>
            public uint dwAttributes;
            /// <summary>
            /// A string that contains the name of the file as it appears in the Windows Shell, or the path and file name of the file that contains the icon representing the file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string szDisplayName;
            /// <summary>
            /// A string that describes the type of file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szTypeName;
        }
    }
}

