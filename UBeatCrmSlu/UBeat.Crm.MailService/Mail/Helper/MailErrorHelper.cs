using MailKit;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UBeat.Crm.MailService.Mail.Enum;

namespace UBeat.Crm.MailService.Mail.Helper
{
    public class MailError
    {
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public int Status { get; set; }
        public string ErrorMsg { get; set; }
        public string ErrorTime { get; set; }
    }


    public static class ExceptionTipMsgSwitch
    {

        public static string ExceptionTipMsg(Exception ex)
        {
            IOException ioEx = ex as IOException;
            if (ioEx != null)
            {
                if (ioEx.InnerException != null)
                {
                    SocketException socketException = ioEx.InnerException as SocketException;
                    return socketException.Message;
                }
            }
            ArgumentNullException argNullEx = ex as ArgumentNullException;
            if (argNullEx != null)
            {
                if (argNullEx.ParamName == "host")
                {
                    return "邮箱服务器Host不能为空,请检查配置";//host等于空的报的异常
                }
                if (argNullEx.ParamName == "userName")
                {
                    return "邮箱用户名不能为空,请检查配置";
                }
                if (argNullEx.ParamName == "password")
                {
                    return "邮箱密码不能为空,请检查配置";
                }
            }
            ArgumentOutOfRangeException argOutEx = ex as ArgumentOutOfRangeException;
            if (argOutEx != null)
            {
                if (argOutEx.ParamName == "port")
                {
                    return "邮箱服务器端口溢出,请检查配置";
                }
            }
            ArgumentException argEx = ex as ArgumentException;
            if (argEx != null)
            {
                if (argOutEx.ParamName == "host")
                {
                    return "邮箱服务器Host地址长度不能为0";
                }
            }
            ObjectDisposedException objDisEx = ex as ObjectDisposedException;
            if (objDisEx != null)
            {
                return "邮件服务器服务已经被销毁";
            }
            InvalidOperationException invalidOpEx = ex as InvalidOperationException;
            if (invalidOpEx != null)
            {
                return "邮件服务器已经连接或已经验证";
            }
            OperationCanceledException opCancelEx = ex as OperationCanceledException;
            if (opCancelEx != null)
            {
                return "邮件操作已经被取消";
            }
            SocketException socEx = ex as SocketException;
            if (socEx != null)
            {
                return "尝试连接到邮件服务器失败";
            }
            ProtocolException protocolEx = ex as ProtocolException;
            if (protocolEx != null)
            {
                return "邮件协议异常";
            }
            AuthenticationException authEx = ex as AuthenticationException;
            if (authEx != null)
            {
                return "邮件身份验证失败,请检查配置";
            }

            SaslException saslEx = ex as SaslException;
            if (saslEx != null)
            {
                return "SASL验证失败,请检查配置";
            }
            return "邮件收发服务异常";
        }
    }
}
