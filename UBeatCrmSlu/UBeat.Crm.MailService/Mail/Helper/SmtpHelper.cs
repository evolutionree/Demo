using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.MailService.Mail.Enum;

namespace UBeat.Crm.MailService.Mail.Helper
{
    // 1、一个SmtpClient一次只能发送一个MailMessage，不管是同步还是异步发送，所以批量发送也会因为这个条件而被阻塞。
    // 2、若要异步发送大批量邮件，方案：应当多个线程、每个线程去使用一个单独的SmtpClient去发送。（但要注意不合理分配资源会更加降低性能）
    // 3、何时使用 SmtpClient.SendAsync() 异步发送呢？是在发件内容、附件、加密等因素造成一条短信发送比较耗时的情况下使用。



    /// <summary>
    /// SmtpClient构造器
    /// 使用注意事项：
    /// 1、非线程安全类
    /// 2、构造的SmtpClient 实例由外部进行Dispose()。SmtpHelper类只简单提供构造，不做释放操作。
    /// 3、SmtpClient 没有提供 Finalize() 终结器，所以GC不会进行回收，只能由外部使用完后进行显示释放，否则会发生内存泄露问题
    /// </summary>
    public class SmtpHelper
    {
        /// <summary>
        /// 返回内部构造的SmtpClient实例
        /// </summary>
        public SmtpClient SmtpClient { get; private set; }

        #region  SmtpHelper 构造函数

        #region SMTP服务器 需要身份验证凭据`

        /// <summary>
        /// 创建 SmtpHelper 实例
        /// </summary>
        /// <param name="host">设置 SMTP 主服务器</param>
        /// <param name="port">端口号</param>
        /// <param name="enableSsl">指定 SmtpClient 是否使用安全套接字层 (SSL) 加密连接。</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public SmtpHelper(string host, int port, bool enableSsl, string userName, string password)
        {
            MailValidatorHelper.ValideStrNullOrEmpty(host, "host");
            MailValidatorHelper.ValideStrNullOrEmpty(userName, "userName");
            MailValidatorHelper.ValideStrNullOrEmpty(password, "password");

            this.InitSmtpEmailConfig(host, port, enableSsl);
            SmtpClient.Authenticate(userName, password);
        }


        #endregion


        /// <summary>
        /// 根据Email类型创建SmtpClient
        /// </summary>
        /// <param name="type">Email类型</param>
        /// <param name="enableSsl">端口号会根据是否支持ssl而不同</param>
        private void InitSmtpEmailConfig(string host, int port, bool enableSsl)
        {
            SmtpClient = new SmtpClient();
            if (!SmtpClient.IsConnected)
            {
                SmtpClient.Timeout = 3000;
                SmtpClient.Connect(host, port, enableSsl);
            }
            SmtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
            SmtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        #endregion

        /// <summary>
        /// 设置SmtpClient.Send() 调用的超时时间。
        /// SmtpClient默认 Timeout =  （100秒=100*1000毫秒）。
        /// 应当根据“邮件大小、附件大小、加密耗时”等因素进行调整
        /// </summary>
        public SmtpHelper SetTimeout(int timeout)
        {
            if (timeout > 0)
            {
                SmtpClient.Timeout = timeout;
            }
            return this;
        }


    }
}
