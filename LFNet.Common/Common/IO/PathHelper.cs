using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LFNet.Common.Extensions;

namespace LFNet.Common.IO
{
    /// <summary>
    /// A class with various path helper methods
    /// </summary>
    public static class PathHelper
    {
        private static readonly char[] _invalidFileNameChars;
        private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();
        private static readonly Regex _uniqueRegex = new Regex(@"\[(?<number>\d+)\]");
        private const string DATA_DIRECTORY = "|DataDirectory|";

        static PathHelper()
        {
            Array.Sort<char>(_invalidPathChars, 0, _invalidPathChars.Length);
            _invalidFileNameChars = Path.GetInvalidFileNameChars();
            Array.Sort<char>(_invalidFileNameChars, 0, _invalidFileNameChars.Length);
        }

        public static string Combine(params object[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            if (paths.Length == 0)
            {
                return string.Empty;
            }
            string str = paths[0].ToString();
            for (int i = 1; i < paths.Length; i++)
            {
                if (paths[i] != null)
                {
                    string item = paths[i].ToString();
                    if (!item.IsNullOrEmpty())
                    {
                        str = Path.Combine(str, item);
                    }
                }
            }
            if (!File.Exists(str))
            {
                return str;
            }
            return Path.GetFullPath(str);
        }

        /// <summary>
        /// Creates the directory if it does not exist.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        public static void CreateDirectory(string path)
        {
            if (Path.HasExtension(path))
            {
                path = Path.GetDirectoryName(path);
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Deletes the directory, subdirectories, and files in path if it exists.
        /// </summary>
        /// <param name="path">The directory path to delete.</param>
        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                DeleteDirectoryInternal(path);
                Directory.Delete(path, true);
            }
        }

        private static void DeleteDirectoryInternal(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            foreach (FileSystemInfo info2 in info.GetFileSystemInfos())
            {
                if (info2.Attributes.IsFlagOn<FileAttributes>(FileAttributes.ReadOnly))
                {
                    info2.Attributes = info2.Attributes.SetFlagOff<FileAttributes>(FileAttributes.ReadOnly);
                }
                if (info2.Attributes.IsFlagOn<FileAttributes>(FileAttributes.Hidden))
                {
                    info2.Attributes = info2.Attributes.SetFlagOff<FileAttributes>(FileAttributes.Hidden);
                }
                if (info2.Attributes.IsFlagOn<FileAttributes>(FileAttributes.Directory))
                {
                    DeleteDirectoryInternal(info2.FullName);
                }
                info2.Delete();
                info2.Refresh();
            }
        }

        /// <summary>
        /// Expand the filename of the data source, resolving the |DataDirectory| macro as appropriate.
        /// </summary>
        /// <param name="sourceFile">The database filename to expand</param>
        /// <returns>The expanded path and filename of the filename</returns>
        public static string ExpandPath(string sourceFile)
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                return sourceFile;
            }
            if (!sourceFile.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(sourceFile);
            }
            string dataDirectory = GetDataDirectory();
            int length = "|DataDirectory|".Length;
            if (sourceFile.Length <= length)
            {
                return dataDirectory;
            }
            string str2 = sourceFile.Substring(length);
            char ch = str2[0];
            if ((ch == Path.DirectorySeparatorChar) || (ch == Path.AltDirectorySeparatorChar))
            {
                str2 = str2.Substring(1);
            }
            return Path.GetFullPath(Path.Combine(dataDirectory, str2));
        }

        /// <summary>
        /// Removes illegal characters from a file name
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="maxLength">The maximum length for the returned file name</param>
        /// <returns>
        /// A string that contains the cleaned file name
        /// </returns>
        public static string GetCleanFileName(string fileName, int maxLength = 20)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            StringBuilder builder = new StringBuilder();
            foreach (char ch in from t in fileName
                where Array.BinarySearch<char>(_invalidFileNameChars, t) < 0
                select t)
            {
                builder.Append(ch);
            }
            if (builder.ToString().Length <= maxLength)
            {
                return builder.ToString();
            }
            return builder.ToString().Substring(0, maxLength);
        }

        /// <summary>
        /// Removes illegal characters from a file path
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>
        /// A string that contains the cleaned file path
        /// </returns>
        public static string GetCleanPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            StringBuilder builder = new StringBuilder();
            foreach (char ch in from t in path
                where Array.BinarySearch<char>(_invalidPathChars, t) < 0
                select t)
            {
                builder.Append(ch);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Gets the data directory for the |DataDirectory| macro.
        /// </summary>
        /// <returns>The DataDirectory path.</returns>
        public static string GetDataDirectory()
        {
            string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(data))
            {
                data = AppDomain.CurrentDomain.BaseDirectory;
            }
            return Path.GetFullPath(data);
        }

        /// <summary>
        /// Creates a unique filename based on an existing filename
        /// </summary>
        /// <param name="fileSpec" type="string">A string containing the fully qualified path that will contain the new file</param>
        /// <returns>A string that contains the fully qualified path of the unique file name</returns>
        public static string GetUniqueName(string fileSpec)
        {
            if (string.IsNullOrEmpty(fileSpec))
            {
                throw new ArgumentNullException("fileSpec");
            }
            string directoryName = Path.GetDirectoryName(fileSpec);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileSpec);
            string extension = Path.GetExtension(fileSpec);
            while (File.Exists(fileSpec))
            {
                Match match = _uniqueRegex.Match(fileNameWithoutExtension);
                if (match.Success)
                {
                    int num = int.Parse(match.Groups["number"].Value);
                    fileNameWithoutExtension = _uniqueRegex.Replace(fileNameWithoutExtension, string.Format("[{0}]", ++num));
                }
                else
                {
                    fileNameWithoutExtension = fileNameWithoutExtension + "[1]";
                }
                fileSpec = fileNameWithoutExtension + extension;
                fileSpec = Path.Combine(directoryName, fileSpec);
            }
            return fileSpec;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromDirectory">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        public static string RelativePathTo(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
            {
                throw new ArgumentNullException("fromDirectory");
            }
            if (toPath == null)
            {
                throw new ArgumentNullException("toPath");
            }
            if ((Path.IsPathRooted(fromDirectory) && Path.IsPathRooted(toPath)) && !string.Equals(Path.GetPathRoot(fromDirectory), Path.GetPathRoot(toPath), StringComparison.OrdinalIgnoreCase))
            {
                return toPath;
            }
            string[] strArray = fromDirectory.Split(new char[] { Path.DirectorySeparatorChar });
            string[] strArray2 = toPath.Split(new char[] { Path.DirectorySeparatorChar });
            int num = Math.Min(strArray.Length, strArray2.Length);
            int num2 = -1;
            for (int i = 0; i < num; i++)
            {
                if (!string.Equals(strArray[i], strArray2[i], StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                num2 = i;
            }
            if (num2 == -1)
            {
                return toPath;
            }
            List<string> list = new List<string>();
            for (int j = num2 + 1; j < strArray.Length; j++)
            {
                if (strArray[j].Length > 0)
                {
                    list.Add("..");
                }
            }
            for (int k = num2 + 1; k < strArray2.Length; k++)
            {
                list.Add(strArray2[k]);
            }
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            return string.Join(Path.DirectorySeparatorChar.ToString(), array);
        }
    }
}

