using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace LFNet.Common
{
    /// <summary>
    /// 文件路径操作工具类
    /// </summary>
    public static class FilePathUtil
    {
        #region file

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>创建是否成功</returns>
        [DllImport("dbgHelp", SetLastError = true)]
        private static extern bool MakeSureDirectoryPathExists(string name);


        /// <summary>
        /// 建立文件夹
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CreateDir(string name)
        {
            return MakeSureDirectoryPathExists(name);
        }

        /// <summary>
        /// 返回指定目录下的非 UTF8 字符集文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>文件名的字符串数组</returns>
        public static string[] FindNoUtf8File(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var filelist = new StringBuilder();
            var folder = new DirectoryInfo(path);
            FileInfo[] subFiles = folder.GetFiles();

            foreach (FileInfo t in subFiles)
            {
                if (t.Extension.ToLower().Equals(".htm"))
                {
                    var fs = new FileStream(t.FullName, FileMode.Open, FileAccess.Read);
                    bool bUtf8 = IsUtf8(fs);
                    fs.Close();
                    if (!bUtf8)
                    {
                        filelist.Append(t.FullName);
                        filelist.Append("\r\n");
                    }
                }
            }
            return Utils.SplitString(filelist.ToString(), "\r\n");
        }

        //0000 0000-0000 007F - 0xxxxxxx  (ascii converts to 1 octet!)
        //0000 0080-0000 07FF - 110xxxxx 10xxxxxx    ( 2 octet format)
        //0000 0800-0000 FFFF - 1110xxxx 10xxxxxx 10xxxxxx (3 octet format)

        /// <summary>
        /// 判断文件流是否为UTF8字符集
        /// </summary>
        /// <param name="sbInputStream">文件流</param>
        /// <returns>判断结果</returns>
        private static bool IsUtf8(FileStream sbInputStream)
        {
            int i;
            bool bAllAscii = true;
            long iLen = sbInputStream.Length;

            byte cOctets = 0;
            for (i = 0; i < iLen; i++)
            {
                var chr = (byte) sbInputStream.ReadByte();

                if ((chr & 0x80) != 0) bAllAscii = false;

                if (cOctets == 0)
                {
                    if (chr >= 0x80)
                    {
                        do
                        {
                            chr <<= 1;
                            cOctets++;
                        } while ((chr & 0x80) != 0);

                        cOctets--;
                        if (cOctets == 0)
                            return false;
                    }
                }
                else
                {
                    if ((chr & 0xC0) != 0x80)
                        return false;

                    cOctets--;
                }
            }

            if (cOctets > 0)
                return false;

            if (bAllAscii)
                return false;

            return true;
        }

        /// <summary>
        /// 备份文件
        /// </summary>
        /// <param name="sourceFileName">源文件名</param>
        /// <param name="destFileName">目标文件名</param>
        /// <param name="overwrite">当目标文件存在时是否覆盖</param>
        /// <returns>操作是否成功</returns>
        public static bool BackupFile(string sourceFileName, string destFileName, bool overwrite)
        {
            if (!File.Exists(sourceFileName))
                throw new FileNotFoundException(sourceFileName + "文件不存在！");

            if (!overwrite && File.Exists(destFileName))
                return false;

            try
            {
                File.Copy(sourceFileName, destFileName, true);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 备份文件,当目标文件存在时覆盖
        /// </summary>
        /// <param name="sourceFileName">源文件名</param>
        /// <param name="destFileName">目标文件名</param>
        /// <returns>操作是否成功</returns>
        public static bool BackupFile(string sourceFileName, string destFileName)
        {
            return BackupFile(sourceFileName, destFileName, true);
        }


        /// <summary>
        /// 恢复文件
        /// </summary>
        /// <param name="backupFileName">备份文件名</param>
        /// <param name="targetFileName">要恢复的文件名</param>
        /// <param name="backupTargetFileName">要恢复文件再次备份的名称,如果为null,则不再备份恢复文件</param>
        /// <returns>操作是否成功</returns>
        public static bool RestoreFile(string backupFileName, string targetFileName, string backupTargetFileName)
        {
            try
            {
                if (!File.Exists(backupFileName))
                    throw new FileNotFoundException(backupFileName + "文件不存在！");

                if (backupTargetFileName != null)
                {
                    if (!File.Exists(targetFileName))
                        throw new FileNotFoundException(targetFileName + "文件不存在！无法备份此文件！");
                    else
                        File.Copy(targetFileName, backupTargetFileName, true);
                }
                File.Delete(targetFileName);
                File.Copy(backupFileName, targetFileName);
            }
            catch (Exception e)
            {
                throw e;
            }
            return true;
        }

        /// <summary>
        /// Restores the file.
        /// </summary>
        /// <param name="backupFileName">Name of the backup file.</param>
        /// <param name="targetFileName">Name of the target file.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool RestoreFile(string backupFileName, string targetFileName)
        {
            return RestoreFile(backupFileName, targetFileName, null);
        }

        /// <summary>
        /// 获取指定文件的扩展名
        /// </summary>
        /// <param name="fileName">指定文件名</param>
        /// <returns>扩展名</returns>
        public static string GetFileExtName(string fileName)
        {
            if (Utils.StrIsNullOrEmpty(fileName) || fileName.IndexOf('.') <= 0)
                return "";

            fileName = fileName.ToLower().Trim();

            return fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.'));
        }

        /// <summary>
        /// 转换长文件名为短文件名
        /// </summary>
        /// <param name="fullname">The fullname.</param>
        /// <param name="repstring">The repstring.</param>
        /// <param name="leftnum">The leftnum.</param>
        /// <param name="rightnum">The rightnum.</param>
        /// <param name="charnum">The charnum.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ConvertSimpleFileName(string fullname, string repstring, int leftnum, int rightnum,
                                                   int charnum)
        {
            string simplefilename;
            string extname = GetFileExtName(fullname);

            if (Utils.StrIsNullOrEmpty(extname))
                throw new Exception("字符串不含有扩展名信息");

            int dotindex = fullname.LastIndexOf('.');
            string filename = fullname.Substring(0, dotindex);
            int filelength = filename.Length;
            if (dotindex > charnum)
            {
                string leftstring = filename.Substring(0, leftnum);
                string rightstring = filename.Substring(filelength - rightnum, rightnum);
                if (string.IsNullOrEmpty(repstring))
                    simplefilename = leftstring + rightstring + "." + extname;
                else
                    simplefilename = leftstring + repstring + rightstring + "." + extname;
            }
            else
                simplefilename = fullname;

            return simplefilename;
        }

        /// <summary>
        /// 返回文件是否存在
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>是否存在</returns>
        public static bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        //public static string GetFileExtName(string filename)
        //{
        //    string[] array = filename.Trim().Split('.');
        //    Array.Reverse(array);
        //    return array[0].ToString();
        //}

        #endregion

        #region Path

        /// <summary>
        /// 返回URL中结尾的文件名
        /// </summary>		
        public static string GetFilename(string url)
        {
            if (url == null)
            {
                return "";
            }
            string[] strs1 = url.Split(new[] {'/'});
            return strs1[strs1.Length - 1].Split(new[] {'?'})[0];
        }

        /// <summary>
        /// 获得当前绝对路径
        /// </summary>
        /// <param name="strPath">指定的路径</param>
        /// <returns>绝对路径</returns>
        public static string GetMapPath(string strPath)
        {
            if (HttpContext.Current != null)
            {
                strPath = strPath.Replace("\\", "/");
                if (strPath.StartsWith("/")) strPath = "~" + strPath;
                return HttpContext.Current.Server.MapPath(strPath);
            }
            strPath = strPath.Replace("/", "\\");
            if (strPath.StartsWith("\\"))
            {
                //strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
                strPath = strPath.TrimStart('\\');
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
        }

        /// <summary>
        /// 得到请求的真实路径,
        /// 如：http://www.abc.com/a/b/c.asp?xxxxxx; 
        /// </summary>
        /// <returns>返回/a/b/</returns>
        public static string GetTrueRequetPath()
        {
            string path = HttpContext.Current.Request.Path;
            path = path.LastIndexOf("/") != path.IndexOf("/")
                       ? path.Substring(path.IndexOf("/"), path.LastIndexOf("/") + 1)
                       : "/";

            return path;
        }

        /// <summary>
        /// 获取站点根目录URL
        /// </summary>
        /// <returns></returns>
        public static string GetRootUrl(string path)
        {
            int port = HttpContext.Current.Request.Url.Port;
            return string.Format("{0}://{1}{2}{3}",
                                 HttpContext.Current.Request.Url.Scheme,
                                 HttpContext.Current.Request.Url.Host,
                                 (port == 80 || port == 0) ? "" : ":" + port,
                                 path);
        }


        /// <summary>
        /// 在路径后面添加'/'
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string AppendSlashToPathIfNeeded(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            if ((length != 0) && (path[length - 1] != '/'))
            {
                path = path + '/';
            }
            return path;
        }

        #endregion
    }
}