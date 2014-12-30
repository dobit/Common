#region 版权信息
/****************************************************
* 文 件 名：Email.cs
* CLR 版本: 4.0.30319.235 
* 创 建 人：dobit
* 创建日期：2011/7/28 16:16:31
* Copyright(c) 2010-2011 北京中民卓彩科技发展有限公司
****************************************************** 
* 修 改 人：
* 修改日期：
* 备注描述：
*  
* 
*******************************************************/

#endregion

using System;
using System.Net.Mail;

namespace LFNet.Common
{
    /// <summary>
    /// 发送EMAIL
    /// </summary>
    public class EmailSender
    {
        /// <summary>
        /// 发送Email
        /// </summary>
        /// <param name="to">对方电子邮件地址</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="isHtml">是否Html</param>
        /// <param name="senderName">发送邮件的账户名称，如卓彩网用户中心</param>
        public static void Send(string to, string title, string content, bool isHtml, string senderName = "卓彩网用户中心")
        {
            MailMessage mm = new MailMessage();

            mm.Body = content;

            mm.BodyEncoding = System.Text.Encoding.UTF8;
            mm.From = new MailAddress("service@joycp.net",senderName);
            mm.IsBodyHtml = true;
            mm.Subject = title;
            mm.IsBodyHtml = isHtml;
            mm.SubjectEncoding = System.Text.Encoding.UTF8;
            mm.To.Add(new MailAddress(to));
            mm.Sender = new MailAddress("service@joycp.net", senderName);

            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Host = "smtp.ym.163.com";
            client.Port = 25;
            client.Credentials = new System.Net.NetworkCredential("service@joycp.net", "joycp.net");
            client.Send(mm);
        }
    }

}
