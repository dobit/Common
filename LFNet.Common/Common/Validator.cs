using System;
using System.Linq;
using System.Text.RegularExpressions;
using LFNet.Common.Security;

namespace LFNet.Common
{
    /// <summary>
    /// 验证类，
    /// 验证是否是有效的类型
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// 判断对象是否为Int32类型的数字
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(object expression)
        {
            if (expression != null)
                return IsNumeric(expression.ToString());

            return false;
        }

        /// <summary>
        /// 判断对象是否为Int32类型的数字
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(string expression)
        {
            if (expression != null)
            {
                string str = expression;
                if (str.Length > 0 && str.Length <= 11 && Regex.IsMatch(str, @"^[-]?[0-9]*[.]?[0-9]*$"))
                {
                    if ((str.Length < 10) || (str.Length == 10 && str[0] == '1') ||
                        (str.Length == 11 && str[0] == '-' && str[1] == '1'))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否为Double类型
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsDouble(object expression)
        {
            if (expression != null)
                return Regex.IsMatch(expression.ToString(), @"^([0-9])[0-9]*(\.\w*)?$");

            return false;
        }

        /// <summary>
        /// 判断给定的字符串数组(strNumber)中的数据是不是都为数值型
        /// </summary>
        /// <param name="strNumber">要确认的字符串数组</param>
        /// <returns>是则返加true 不是则返回 false</returns>
        public static bool IsNumericArray(string[] strNumber)
        {
            if (strNumber == null)
                return false;

            if (strNumber.Length < 1)
                return false;

            return strNumber.All(IsNumeric);
        }


        /// <summary>
        /// 检测是否符合email格式
        /// </summary>
        /// <param name="email">要判断的email字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w\.]+([-]\w+)*@[A-Za-z0-9-_]+[\.][A-Za-z0-9-_]");
        }


        /// <summary>
        /// Determines whether [is valid do email] [the specified STR email].
        /// </summary>
        /// <param name="email">The STR email.</param>
        /// <returns><c>true</c> if [is valid do email] [the specified STR email]; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public static bool IsValidDoEmail(string email)
        {
            return Regex.IsMatch(email,
                                 @"^@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }


        /// <summary>
        /// 检测是否是正确的Url
        /// </summary>
        /// <param name="strUrl">要验证的Url</param>
        /// <returns>判断结果</returns>
        public static bool IsUrl(string strUrl)
        {
            return Regex.IsMatch(strUrl,
                                 @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
        }

        /// <summary>
        /// 判断文件名是否为浏览器可以直接显示的图片文件名
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>是否可以直接显示</returns>
        public static bool IsImgFilename(string filename)
        {
            filename = filename.Trim();
            if (filename.EndsWith(".") || filename.IndexOf(".") == -1)
                return false;

            string extname = filename.Substring(filename.LastIndexOf(".") + 1).ToLower();
            return (extname == "jpg" || extname == "jpeg" || extname == "png" || extname == "bmp" || extname == "gif");
        }

        /// <summary>
        /// 判断是否为base64字符串
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns><c>true</c> if [is base64 string] [the specified STR]; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public static bool IsBase64String(string str)
        {
            //A-Z, a-z, 0-9, +, /, =
            return Regex.IsMatch(str, @"[A-Za-z0-9\+\/\=]");
        }

        /// <summary>
        /// 检测是否有Sql危险字符
        /// </summary>
        /// <param name="str">要判断字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsSafeSqlString(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }

        /// <summary>
        /// 检测是否有危险的可能用于链接的字符串
        /// </summary>
        /// <param name="str">要判断字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsSafeUserInfoString(string str)
        {
            return !Regex.IsMatch(str, @"^\s*$|^c:\\con\\con$|[%,\*" + "\"" + @"\s\t\<\>\&]|游客|^Guest");
        }

        /// <summary>
        /// 判断字符串是否是yy-mm-dd字符串
        /// </summary>
        /// <param name="str">待判断字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsDateString(string str)
        {
            return Regex.IsMatch(str, @"(\d{4})-(\d{1,2})-(\d{1,2})");
        }

        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIPSect(string ip)
        {
            return Regex.IsMatch(ip,
                                 @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){2}((2[0-4]\d|25[0-5]|[01]?\d\d?|\*)\.)(2[0-4]\d|25[0-5]|[01]?\d\d?|\*)$");
        }

        /// <summary>
        /// Determines whether the specified timeval is time.
        /// </summary>
        /// <param name="timeval">The timeval.</param>
        /// <returns><c>true</c> if the specified timeval is time; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public static bool IsTime(string timeval)
        {
            return Regex.IsMatch(timeval, @"^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }

        /// <summary>
        /// 验证是否为正整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInt(string str)
        {
            return Regex.IsMatch(str, @"^[0-9]*$");
        }

        /// <summary>
        /// 检查颜色值是否为3/6位的合法颜色
        /// </summary>
        /// <param name="color">待检查的颜色</param>
        /// <returns></returns>
        public static bool IsColor(string color)
        {
            if (Utils.StrIsNullOrEmpty(color))
                return false;

            color = color.Trim().Trim('#');

            if (color.Length != 3 && color.Length != 6)
                return false;

            //不包含0-9  a-f以外的字符
            if (!Regex.IsMatch(color, "[^0-9a-f]", RegexOptions.IgnoreCase))
                return true;

            return false;
        }


        #region 检测用户资料

        /// <summary>
        /// 是否卡用户//jco(不限制)
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="canLogin">是否可以登录</param>
        /// <returns></returns>
        public static bool IsCardUser(string userName,out bool canLogin)
        {
            Regex reg = new Regex(@"^(jco|jcc)([0-9]{6})$", RegexOptions.IgnoreCase);
            //Regex reg = new Regex(@"^([a-zA-Z][a-zA-Z_0-9]{3,12})$", RegexOptions.IgnoreCase);
            canLogin = userName.StartsWith("jco", StringComparison.OrdinalIgnoreCase);
            return reg.IsMatch(userName);
        }

        /// <summary>
        /// 验证用户名4-12位[a-zA-Z_0-9]
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool CheckUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;
            foreach (string filterName in filterNames)
            {
                if (userName.Contains(filterName))
                    return false;
            }
            bool canlogin;
            if (IsCardUser(userName, out canlogin))
            {
                return false;
            }
            Regex reg = new Regex(@"^([a-zA-Z_0-9]{4,16})$", RegexOptions.IgnoreCase);
            //Regex reg = new Regex(@"^([a-zA-Z][a-zA-Z_0-9]{3,12})$", RegexOptions.IgnoreCase);

            return reg.IsMatch(userName);
        }

        private static string[] filterNames= new string[]{
            "婊子","阴唇","喷精","小穴","精液","兽交","潮吹",
            "肛交","群射","内射","乱伦","三个代表","一党","多党",
            "民主","专政","自慰","私处","打炮","造爱","作爱",
            "做爱","鸡巴","阴茎","阳具","开苞","肛门","阴道",
            "阴蒂","肉棍","肉棒","肉洞","荡妇","阴囊","睾丸",
            "插她","干你","干她","干他","射","精","口","交",
            "屁眼","阴户","阴门","下体","龟头","阴毛","避孕套",
            "你妈逼","大鸡巴","洪志","法轮","法","打倒","民运",
            "六四","台独","李鹏","江泽民","朱镕基","李长春",
            "李瑞环","胡锦涛","魏京生","台湾独立","藏独","西藏独立",
            "疆独","新疆独立","邓小平","革命","李远哲","人大",
            "尉健行","李岚清","黄丽满","于幼军","文字狱","宋祖英",
            "自焚","吸储","张五常","张丕林","温家宝","吴邦国",
            "曾庆红","黄菊","罗干","吴官正","贾庆林","专制",
            "三個代表","一黨","多黨","專政","大紀元","法輪",
            "台獨","李鵬","江澤民","朱鎔基","李長春","李瑞環",
            "胡錦濤","臺灣獨立","藏獨","西藏獨立","疆獨",
            "新疆獨立","鄧小平","李遠哲","高幹","李嵐清","黃麗滿",
            "於幼軍","文字獄","張五常","張丕林","溫家寶","吳邦國",
            "曾慶紅","黃菊","羅幹","賈慶林","專制","八九","保钓",
            "暴政","北大三角","赤匪","赤化","８.9事件","八九",
            "八九学潮","八荣八耻","薄熙来","操B","操逼","操比",
            "操蛋","操你","操你妈","操你媽","操你娘","操死",
            "操他","草你妈","插B","成人表演","成人电影",
            "成人電影","成人激情","成人交友","成人卡通",
            "成人录像","成人论坛","成人論壇","成人漫画",
            "成人配色","成人片","成人书库","成人贴图","成人貼圖",
            "成人图片","成人圖片","成人网站","成人網站","成人文学",
            "成人小说","成人小說","成人杂志","成人雜誌","出售冰毒",
            "出售答案","出售弹药","出售二手走私车","出售发票",
            "出售工字","出售假币","出售假幣","出售雷管",
            "出售雷管炸药自制炸弹","出售枪支","出售槍支",
            "出售手枪","出售手槍","出售银行","出售炸药","出售走私车",
            "处女","处女终结者","处女终结者","春药","春藥","春药",
            "催情粉","催情水","催情药","催情藥","催情液","达赖喇嘛",
            "达赖领奖","打倒共产党洪传","大法师","第五代红人",
            "第五代接班梯队","第五代中央领导人","第一书记",
            "颠覆中国","政权","颠覆","中华人民共和国","裸聊",
            "电车之狼","电动丁关根","丁元","丁子霖","二B",
            "二逼","发抡功","发论公","发论功","反党","反动",
            "反对共产党","反对共产主义","反封锁","反封锁技术",
            "反革命","反攻","反共","反共传单","反共言论","反华",
            "反民主","反人类","反人类罪","反社会","反政府","反中",
            "反中共","干 你 妈","干妳","干妳老母","干妳妈","干妳娘",
            "干你妈","干你妈b","干你妈逼","干你娘","共产专制","共铲党",
            "共党","共黨","共匪","共","狗","共和国之怒","共军","共奴",
            "狗b","狗逼","狗操","狗卵子","狗娘","胡J涛","胡春华",
            "胡的接班人","胡江","胡紧掏","胡紧套","胡锦滔","胡锦淘",
            "胡錦濤","胡进涛","胡景涛","中央","主席","总书记","鸡八",
            "鸡吧","鸡奸","激情假币","李 洪 志","两派争斗","两性狂情",
            "两性淫乱","兩性淫亂","六●四","六彩","六四","轮奸",
            "虐杀","毛厕东","毛厕洞","毛泽东","毛贼","奶头","奶子",
            "嫖鸡","嫖雞","嫖妓","情色","日你","日批","日死你",
            "日他","肉穴","骚逼","骚穴","色-情","有罪","无码",
            "阴道","阴胫","阴毛","小穴","阴水","陰蒂","淫荡",
            "淫蕩","淫奸","淫间道","淫浪","淫乱","淫亂","办证",
            "刻章","冰毒","国母","幸運用戶","抽獎","私服","S F","fuck","bitch","shit"};
        /// <summary>
        /// 检查用户昵称[a-zA-Z_0-9\u4e00-\u9fa5]2-12位
        /// 包含过滤的不合法文字
        /// </summary>
        /// <returns></returns>
        public static bool CheckNickName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            //Regex reg = new Regex(@"^([a-zA-Z_0-9\u4e00-\u9fa5]{4,16})$", RegexOptions.IgnoreCase);
            foreach (string filterName in filterNames)
            {
                if(name.Contains(filterName)) 
                    return false;
            }
            Regex reg = new Regex(@"^([a-zA-Z_0-9\u4e00-\u9fa5]{1,16})$", RegexOptions.IgnoreCase);
            int len = Utils.GetStringLength(name);
            return reg.IsMatch(name) && len  >= 4 && len<=16;
        }

        /// <summary>
        /// 验证用户真实姓名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckRelName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            Regex reg = new Regex(@"^([\u4e00-\u9fa5]{2,5})$", RegexOptions.IgnoreCase);
            return reg.IsMatch(name);
        }

        /// <summary>
        /// 验证用户手机号码
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool CheckTel(string number)
        {
            if (string.IsNullOrEmpty(number)) return false;
            Regex reg = new Regex(@"^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$", RegexOptions.IgnoreCase);
            return reg.IsMatch(number);
        }
        /// <summary>
        /// 验证用户身份证
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool CheckIDCard(string Id)
        {
            if (Id.Length == 18)
            {
                bool check = CheckIDCard18(Id);
                return check;
            }
            else if (Id.Length == 15)
            {
                bool check = CheckIDCard15(Id);
                return check;
            }
            else
            {
                return false;
            }
        }
        #region 身份证号码验证

        /**/
        /// <summary>
        /// 验证15位身份证号
        /// </summary>
        /// <param name="Id">身份证号</param>
        /// <returns>验证成功为True，否则为False</returns>
        private static bool CheckIDCard18(string Id)
        {
            long n = 0;
            if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }
            //string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            //if (address.IndexOf(Id.Remove(2)) == -1)
            //{
            //    return false;//省份验证
            //}
            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = Id.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;//符合GB11643-1999标准
        }

        public static bool CheckCidInfo(string cid)
        {
            string[] aCity = new string[] { null, null, null, null, null, null, null, null, null, null, null, "北京", "天津", "河北", "山西", "内蒙古", null, null, null, null, null, "辽宁", "吉林", "黑龙江", null, null, null, null, null, null, null, "上海", "江苏", "浙江", "安微", "福建", "江西", "山东", null, null, null, "河南", "湖北", "湖南", "广东", "广西", "海南", null, null, null, "重庆", "四川", "贵州", "云南", "西藏", null, null, null, null, null, null, "陕西", "甘肃", "青海", "宁夏", "新疆", null, null, null, null, null, "台湾", null, null, null, null, null, null, null, null, null, "香港", "澳门", null, null, null, null, null, null, null, null, "国外" };
            double iSum = 0;
            string info = "";
            System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"^\d{17}(\d|x)$");
            System.Text.RegularExpressions.Match mc = rg.Match(cid);
            if (!mc.Success)
            {
                return false;
            }
            cid = cid.ToLower();
            cid = cid.Replace("x", "a");
            if (aCity[int.Parse(cid.Substring(0, 2))] == null)
            {
                return false;
            }
            try
            {
                DateTime.Parse(cid.Substring(6, 4) + "-" + cid.Substring(10, 2) + "-" + cid.Substring(12, 2));
            }
            catch
            {
                return false;
            }
            for (int i = 17; i >= 0; i--)
            {
                iSum += (System.Math.Pow(2, i) % 11) * int.Parse(cid[17 - i].ToString(), System.Globalization.NumberStyles.HexNumber);

            }
            if (iSum % 11 != 1)
                return false;

            return true;
        }
        /**/
        /// <summary>
        /// 验证18位身份证号
        /// </summary>
        /// <param name="Id">身份证号</param>
        /// <returns>验证成功为True，否则为False</returns>
        private static bool CheckIDCard15(string Id)
        {
            long n = 0;
            if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            return true;//符合15位身份证标准
        }




        #endregion


        #endregion

        #region
        public static bool Check3DNumber(string number)
        {
            if (string.IsNullOrEmpty(number)) return false;
            Regex reg = new Regex(@"^[0-9]{3,3}$", RegexOptions.IgnoreCase);
            return reg.IsMatch(number);
        }
        public static bool CheckNumber(string number)
        {
            if (string.IsNullOrEmpty(number)) return false;
            Regex reg = new Regex(@"^[0-9]+$", RegexOptions.IgnoreCase);
            return reg.IsMatch(number);
        }
        #endregion

        // 检查用户名格式
        public static bool CheckSingleNumber(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            Regex reg = new Regex(@"^[0-9]{1,1}$", RegexOptions.IgnoreCase);
            return reg.IsMatch(name);
        }
        public static string GetSingleNumber(string name)
        {
            if (string.IsNullOrEmpty(name)) return "1";
            if (name == "0") return "1";
            Regex reg = new Regex(@"^[0-9]{1,9}$", RegexOptions.IgnoreCase);
            if (reg.IsMatch(name))
            {
                return name;
            }
            return "1";
        }
        /// <summary>
        /// 检查字符串是否为数字
        /// </summary>
        /// <param name="str">需要检查的字符串</param>
        /// <returns>是否为数字</returns>
        public static bool CheckIsNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            foreach (char c in str)
            {
                if (!char.IsNumber(c))
                    return false;
            }
            return true;
        }

        // 检查用户名格式
        public static bool CheckName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            Regex reg = new Regex("^([\u4e00-\u9fa5]{2,8}|[\u4e00-\u9fa5a-zA-Z0-9]{3,16})$", RegexOptions.IgnoreCase);
            return reg.IsMatch(name);
        }
        // 检查密码格式
        public static bool CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            Regex reg = new Regex(@"^([\S]{6,16})$"); //new Regex(@"^([a-zA-Z0-9]{6,16})$");
            return reg.IsMatch(password);
        }
        /// <summary>
        /// 验证银行卡号
        /// </summary>
        /// <param name="cardno">银行卡号</param>
        /// <returns>验证是否成功</returns>
        public static bool CheckBankNum(string cardno)
        {
            Int16 i = 0;
            Int16 check = 0;
            foreach (char c in cardno)
            {
                if (!char.IsNumber(c))
                    return false;
                if (i == cardno.Length - 1)
                    break;

                Int16 p = Convert.ToInt16(c - 48);

                if (i % 2 == 0)
                {
                    check += Convert.ToInt16(p * 2 / 10);
                    check += Convert.ToInt16(p * 2 % 10);
                }
                else
                {
                    check += p;
                }
                i++;
            }
            if (i < 14 || i > 20)
                return false;

            if (cardno.Length != 16)
                return true;

            if ((check + Convert.ToInt16(cardno[cardno.Length - 1] - 48)) % 10 == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 检查EMAIL
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool CheckEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            Regex reg = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            return reg.IsMatch(email);
        }

        /// <summary>
        ///  加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            TripleDes l = new TripleDes();
            return l.Encode(str);
        }
        /// <summary>
        ///  解密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DecrypString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            TripleDes l = new TripleDes();
            return l.Decode(str);
        }

        /// <summary>
        /// 获取页面验证字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetCheckData(string key)
        {
            string s = key +Utils.GetRealIP() + "joycp.net";
            return SecurityUtil.Md5(s);
        }
        /// <summary>
        /// 验证用户页面请求
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CheckRequest(string key)
        {
            string s = key + Utils.GetRealIP() + "joycp.net";
            s = SecurityUtil.Md5(s);
            if (string.Compare(s, JRequest.GetString("key"), true) != 0)
               return false;
            return true;
        }
    }
}