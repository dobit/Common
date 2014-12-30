using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;

namespace LFNet.Common
{
    /// <summary>
    /// web请求操作类
    /// </summary>
    /// <remarks></remarks>
    public class JRequest
    {
        /// <summary>
        /// 判断当前页面是否接收到了Post请求
        /// </summary>
        /// <returns>是否接收到了Post请求</returns>
        public static bool IsPost()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("POST");
        }

        /// <summary>
        /// 判断当前页面是否接收到了Get请求
        /// </summary>
        /// <returns>是否接收到了Get请求</returns>
        public static bool IsGet()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("GET");
        }

        /// <summary>
        /// 返回指定的服务器变量信息
        /// </summary>
        /// <param name="strName">服务器变量名</param>
        /// <returns>服务器变量信息</returns>
        public static string GetServerString(string strName)
        {
            if (HttpContext.Current.Request.ServerVariables[strName] == null)
                return "";

            return HttpContext.Current.Request.ServerVariables[strName];
        }

        /// <summary>
        /// 返回上一个页面的地址
        /// </summary>
        /// <returns>上一个页面的地址</returns>
        public static string GetUrlReferrer()
        {
            string retVal = null;
            if (HttpContext.Current.Request.UrlReferrer != null)
                retVal = HttpContext.Current.Request.UrlReferrer.ToString();

            if (retVal == null)
                return "";

            return retVal;
        }

        /// <summary>
        /// 得到当前完整主机头 host[:port]
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFullHost()
        {
            HttpRequest request = HttpContext.Current.Request;
            if (!request.Url.IsDefaultPort)
                return string.Format("{0}:{1}", request.Url.Host, request.Url.Port);

            return request.Url.Host;
        }

        /// <summary>
        /// 得到主机头
        /// </summary>
        /// <returns></returns>
        public static string GetHost()
        {
            return HttpContext.Current.Request.Url.Host;
        }


        /// <summary>
        /// 获取当前请求的原始 URL(URL 中域信息之后的部分,包括查询字符串(如果存在))
        /// </summary>
        /// <returns>原始 URL</returns>
        public static string GetRawUrl()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        /// <summary>
        /// 判断当前访问是否来自浏览器软件
        /// </summary>
        /// <returns>当前访问是否来自浏览器软件</returns>
        public static bool IsBrowserGet()
        {
            string[] browserName = { "ie", "opera", "netscape", "mozilla", "konqueror", "firefox" };
            string curBrowser = HttpContext.Current.Request.Browser.Type.ToLower();
            return browserName.Any(t => curBrowser.IndexOf((string) t) >= 0);
        }

        /// <summary>
        /// 判断是否来自搜索引擎链接
        /// </summary>
        /// <returns>是否来自搜索引擎链接</returns>
        public static bool IsSearchEnginesGet()
        {
            if (HttpContext.Current.Request.UrlReferrer == null)
                return false;

            string[] searchEngine = {
                                        "google", "yahoo", "msn", "baidu", "sogou", "sohu", "sina", "163", "lycos",
                                        "tom",
                                        "yisou", "iask", "soso", "gougou", "zhongsou"
                                    };
            string tmpReferrer = HttpContext.Current.Request.UrlReferrer.ToString().ToLower();
            return searchEngine.Any(t => tmpReferrer.IndexOf(t) >= 0);
        }

        /// <summary>
        /// 获得当前完整Url地址
        /// </summary>
        /// <returns>当前完整Url地址</returns>
        public static string GetUrl()
        {
            return HttpContext.Current.Request.Url.ToString();
        }


        /// <summary>
        /// 获得指定Url参数的值
        /// </summary>
        /// <param name="strName">Url参数</param>
        /// <returns>Url参数的值</returns>
        public static string GetQueryString(string strName)
        {
            return GetQueryString(strName, false);
        }

        /// <summary>
        /// 获得指定Url参数的值
        /// </summary> 
        /// <param name="strName">Url参数</param>
        /// <param name="sqlSafeCheck">是否进行SQL安全检查</param>
        /// <returns>Url参数的值</returns>
        public static string GetQueryString(string strName, bool sqlSafeCheck)
        {
            if (HttpContext.Current.Request.QueryString[strName] == null)
                return "";

            if (sqlSafeCheck && !Validator.IsSafeSqlString(HttpContext.Current.Request.QueryString[strName]))
                return "unsafe string";

            return HttpContext.Current.Request.QueryString[strName];
        }

        /// <summary>
        /// 获得当前页面的名称
        /// </summary>
        /// <returns>当前页面的名称</returns>
        public static string GetPageName()
        {
            string[] urlArr = HttpContext.Current.Request.Url.AbsolutePath.Split('/');
            return urlArr[urlArr.Length - 1].ToLower();
        }

        /// <summary>
        /// 返回表单或Url参数的总个数
        /// </summary>
        /// <returns></returns>
        public static int GetParamCount()
        {
            return HttpContext.Current.Request.Form.Count + HttpContext.Current.Request.QueryString.Count;
        }


        /// <summary>
        /// 获得指定表单参数的值
        /// </summary>
        /// <param name="strName">表单参数</param>
        /// <returns>表单参数的值</returns>
        public static string GetFormString(string strName)
        {
            return GetFormString(strName, false);
        }

        /// <summary>
        /// 获得指定表单参数的值
        /// </summary>
        /// <param name="strName">表单参数</param>
        /// <param name="sqlSafeCheck">是否进行SQL安全检查</param>
        /// <returns>表单参数的值</returns>
        public static string GetFormString(string strName, bool sqlSafeCheck)
        {
            if (HttpContext.Current.Request.Form[strName] == null)
                return "";

            if (sqlSafeCheck && !Validator.IsSafeSqlString(HttpContext.Current.Request.Form[strName]))
                return "unsafe string";

            return HttpContext.Current.Request.Form[strName];
        }

        /// <summary>
        /// 获得Url或表单参数的值, 先判断Url参数是否为空字符串, 如为True则返回表单参数的值
        /// </summary>
        /// <param name="strName">参数</param>
        /// <returns>Url或表单参数的值</returns>
        public static string GetString(string strName)
        {
            return GetString(strName, false);
        }

        /// <summary>
        /// 获得Url或表单参数的值, 先判断Url参数是否为空字符串, 如为True则返回表单参数的值
        /// </summary>
        /// <param name="strName">参数</param>
        /// <param name="sqlSafeCheck">是否进行SQL安全检查</param>
        /// <returns>Url或表单参数的值</returns>
        public static string GetString(string strName, bool sqlSafeCheck)
        {
            if ("".Equals(GetQueryString(strName)))
                return GetFormString(strName, sqlSafeCheck);
            return GetQueryString(strName, sqlSafeCheck);
        }

        /// <summary>
        /// 获得指定Url参数的int类型值
        /// </summary>
        /// <param name="strName">Url参数</param>
        /// <returns>Url参数的int类型值</returns>
        public static int GetQueryInt(string strName)
        {
            return Converter.StrToInt(HttpContext.Current.Request.QueryString[strName], 0);
        }


        /// <summary>
        /// 获得指定Url参数的int类型值
        /// </summary>
        /// <param name="strName">Url参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>Url参数的int类型值</returns>
        public static int GetQueryInt(string strName, int defValue)
        {
            return Converter.StrToInt(HttpContext.Current.Request.QueryString[strName], defValue);
        }


        /// <summary>
        /// 获得指定表单参数的int类型值
        /// </summary>
        /// <param name="strName">表单参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>表单参数的int类型值</returns>
        public static int GetFormInt(string strName, int defValue)
        {
            return Converter.StrToInt(HttpContext.Current.Request.Form[strName], defValue);
        }
        /// <summary>
        /// 获得指定Url或表单参数的int类型值, 先判断Url参数是否为缺省值, 如为True则返回表单参数的值
        /// </summary>
        /// <param name="strName">Url或表单参数</param>
        /// <returns>不存在则返回-1</returns>
        public static  int GetInt(string strName)
        {
            return GetInt(strName, -1);
        }

        /// <summary>
        /// 获得指定Url或表单参数的int类型值, 先判断Url参数是否为缺省值, 如为True则返回表单参数的值
        /// </summary>
        /// <param name="strName">Url或表单参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>Url或表单参数的int类型值</returns>
        public static int GetInt(string strName, int defValue)
        {
            if (GetQueryInt(strName, defValue) == defValue)
                return GetFormInt(strName, defValue);
            return GetQueryInt(strName, defValue);
        }

        /// <summary>
        /// 获得指定Url参数的float类型值
        /// </summary>
        /// <param name="strName">Url参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>Url参数的int类型值</returns>
        public static float GetQueryFloat(string strName, float defValue)
        {
            return Converter.StrToFloat(HttpContext.Current.Request.QueryString[strName], defValue);
        }


        /// <summary>
        /// 获得指定表单参数的float类型值
        /// </summary>
        /// <param name="strName">表单参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>表单参数的float类型值</returns>
        public static float GetFormFloat(string strName, float defValue)
        {
            return Converter.StrToFloat(HttpContext.Current.Request.Form[strName], defValue);
        }

        /// <summary>
        /// 获得指定Url或表单参数的float类型值, 先判断Url参数是否为缺省值, 如为True则返回表单参数的值
        /// </summary>
        /// <param name="strName">Url或表单参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>Url或表单参数的int类型值</returns>
        public static float GetFloat(string strName, float defValue)
        {
            if (GetQueryFloat(strName, defValue) == defValue)
                return GetFormFloat(strName, defValue);
            return GetQueryFloat(strName, defValue);
        }

        /// <summary>
        /// 获得当前页面客户端的IP
        /// </summary>
        /// <returns>当前页面客户端的IP</returns>
        public static string GetIP()
        {
            string result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(result))
                result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(result))
                result = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(result) || !Validator.IsIP(result))
                return "127.0.0.1";

            return result;
        }

        /// <summary>
        /// 保存用户上传的文件
        /// </summary>
        /// <param name="filename">保存路径</param>
        public static void SaveRequestFile(string filename)
        {
            if (HttpContext.Current.Request.Files.Count > 0)
            {
                HttpContext.Current.Request.Files[0].SaveAs(filename);
            }
        }
        /// <summary>
        /// 上传并保存文件 表单别忘记加 enctype="multipart/form-data"
        /// </summary>
        /// <param name="savePath">保存路径 请以/结尾</param>
        /// <param name="supportExts">支持的扩展名 |分割 如：JPG|GIF ,留空上传任意文件</param>
        /// <param name="useOriginalFileName">使用上传的文件名</param>
        /// <param name="overwite">同名文件是否覆盖</param>
        /// <returns></returns>
        public static IList<string> UploadFiles(string savePath, string supportExts, bool useOriginalFileName, bool overwite)
        {
            if (!string.IsNullOrEmpty(supportExts))
            {

                for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                {
                    HttpPostedFile file = HttpContext.Current.Request.Files[i];
                    string extname = FilePathUtil.GetFileExtName(file.FileName);
                    if (string.IsNullOrEmpty(extname))
                    {
                        throw new Exception(string.Format("{0} 文件扩展名不存在", file.FileName));
                    }
                    if (!Utils.InArray(extname.Trim('.'), supportExts, "|", true))
                    {
                        throw new SecurityException(string.Format("没权限上传 {0} 格式的文件 ", extname));
                    }
                }

            }
            string path = savePath;
            if (!path.Contains(":")) path = FilePathUtil.GetMapPath(savePath);
            IList<string> list = new List<string>();
            for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
            {
                HttpPostedFile file = HttpContext.Current.Request.Files[i];
                string extname = FilePathUtil.GetFileExtName(file.FileName);
                string filename = "";
                if (useOriginalFileName)
                {
                    filename = path + file.FileName.Substring(file.FileName.Replace("\\", "/").LastIndexOf("/") + 1);

                }
                else
                {
                    filename = path + DateTime.Now.ToString("yyyyMMddHHmmss") + RNG.Next(10000, 99999) + extname;
                }

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (FilePathUtil.FileExists(filename))
                {
                    if (overwite)
                        File.Delete(filename);
                    else
                    {
                        throw new Exception(string.Format("{0} 已经存在", filename));
                    }
                }

                file.SaveAs(filename);
                list.Add(filename);
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="savePath"></param>
        /// <param name="extension"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool UploadFile(global::System.Web.UI.WebControls.FileUpload file, string savePath, string extension, out string message)
        {
            bool isSuccess = false;
            string filepath = string.Empty;
            string fileName = file.PostedFile.FileName;
            message = string.Empty;
            string saveName = DateTime.Now.ToString("yyyyMMddHHmmss") + RNG.Next(10000, 99999);
            string exten = Path.GetExtension(fileName).ToLower();
            if (!Directory.Exists(savePath + "images/Package/"))
                Directory.CreateDirectory(savePath + "images/Package/");
            filepath = "images/package/" + saveName + exten;
            if (!String.IsNullOrEmpty(extension))
            {
                extension = extension.ToLower();
                if (extension.IndexOf(exten) < 0)
                {
                    message = "只允许上传格式为：" + extension + "的文件";
                    return isSuccess;
                }
            }
            try
            {
                file.SaveAs(savePath + filepath);
                message = filepath;
                isSuccess = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return isSuccess;
        }
    }
}