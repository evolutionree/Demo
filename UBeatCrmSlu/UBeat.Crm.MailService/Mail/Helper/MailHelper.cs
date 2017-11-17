using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace UBeat.Crm.MailService.Mail.Helper
{
    public class MailHelper
    {

        private SmtpClient m_SmtpClient = null;

        /// <summary>
        /// 默认为false。设置在 MailHelper 类内部，发送完邮件后是否自动释放 SmtpClient 实例
        /// Smtp不管是在 MailHelper 内部还是在外部都必须进行主动释放，
        /// 因为：SmtpClient 没有提供 Finalize() 终结器，所以GC不会进行回收，只能使用完后主动进行释放，否则会发生内存泄露问题。
        /// 
        /// 何时将 autoReleaseSmtp 设置为false，就是SmtpClient需要重复使用的情况，即需要使用“相同MailHelper”向“相同Smtp服务器”发送大批量的邮件时。
        /// </summary>
        private bool m_autoDisposeSmtp = false;

        #region  计划邮件数量 和 已执行完成邮件数量

        // 记录和获取在大批量执行异步短信发送时已经处理了多少条记录
        // 1、根据此值手动或自动释放 SmtpClient .实际上没有需要根据此值进行手动释放，因为完全可以用自动释放替换此逻辑
        // 2、根据此值可以自己设置进度
        private long m_CompletedSendCount = 0;
        public long CompletedSendCount
        {
            get { return Interlocked.Read(ref m_CompletedSendCount); }
            private set { Interlocked.Exchange(ref m_CompletedSendCount, value); }
        }

        // 计划邮件数量
        private long m_PrepareSendCount = 0;
        public long PrepareSendCount
        {
            get { return Interlocked.Read(ref m_PrepareSendCount); }
            private set { Interlocked.Exchange(ref m_PrepareSendCount, value); }
        }

        #endregion

        // 是否启用异步发送邮件
        private bool m_IsAsync = false;

        /// <summary>
        /// 在执行异步发送时传递的对象，用于传递给异步发生完成时调用的方法 OnSendCompleted 。
        /// </summary>
        public object AsycUserState { get; set; }

        /// <summary>
        /// 检查此 MailHelper 实例是否已经设置了 SmtpClient
        /// </summary>
        /// <returns>true代表已设置</returns>
        public bool ExistsSmtpClient()
        {
            return m_SmtpClient != null ? true : false;
        }



        /// <summary>
        /// 设置 SmtpClient 实例 和是否自动释放Smtp的唯一入口
        /// 1、将内部 计划数量 和 已完成数量 清零，重新统计以便自动释放SmtpClient
        /// 2、若要对SmtpClent设置SendCompleted事件，请在调用此方法前进行设置
        /// </summary>
        /// <param name="mSmtpClient"> SmtpClient 实例</param>
        /// <param name="autoReleaseSmtp">设置在 MailHelper 类内部，发送完邮件后是否自动释放 SmtpClient 实例</param>
        public void SetSmtpClient(SmtpClient mSmtpClient, bool autoReleaseSmtp)
        {
#if DEBUG
            Debug.WriteLine("设置SmtpClient,自动释放为" + (autoReleaseSmtp ? "TRUE" : "FALSE"));
#endif
            m_SmtpClient = mSmtpClient;
            m_autoDisposeSmtp = autoReleaseSmtp;

            // 将内部 计划数量 和 已完成数量 清零，重新统计以便自动释放SmtpClient  (MailHelper实例唯一的清零地方)
            m_PrepareSendCount = 0;
            m_CompletedSendCount = 0;

            if (m_IsAsync && autoReleaseSmtp)
            {
                // 注册内部释放回调事件.释放对象---该事件不进行取消注册，只在释放SmtpClient时，一起释放   （所以SmtpClient与MailHelper绑定后，就不要再单独使用了）
          //      m_SmtpClient.MessageSent += new EventHandler<MessageSentEventArgs>(MessageSentDispose);
            }
        }
    }
}
