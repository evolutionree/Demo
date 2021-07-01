using System;
using Novell.Directory.Ldap;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class LDAPUtil
    {
        public static string Host { get; private set; }
        public static string BindDN { get; private set; }
        public static string BindPassword { get; private set; }
        public static int Port { get; private set; }
        public static string BaseDC { get; private set; } 

        public static void Register(string host,int port,string bindDn,string bindPassword,string baseDc)
        {
            Host = host;
            Port = port;
            BindDN = bindDn;
            BindPassword = bindPassword;
            BaseDC = baseDc; 
        }

        public static string ValidateDotnetCore(string username, string password)
        {
            var result = string.Empty;
            try
            {
                using (var connection = new LdapConnection { SecureSocketLayer = false })
                {
                    try
                    {
                        connection.Connect(Host, Port);
                    }
                    catch (Exception)
                    {
                        return "AD系统不可访问，请检查是否网络通畅";
                    }
                    try
                    {
                        connection.Bind(BindDN, BindPassword);
                    }
                    catch (Exception)
                    {
                        return "AD系统不可访问，请检查是否管理员账号密码的正确性";
                    }
                    var entities =
                    connection.Search(BaseDC, LdapConnection.SCOPE_SUB,
                        string.Format(@"(&(objectClass=organizationalPerson)(sAMAccountName={0}))", username),
                        new string[] { "sAMAccountName" }, false);
                    string userDn = null;
                    while (entities.HasMore())
                    {
                        var entity = entities.Next();
                        var account = entity.getAttribute("sAMAccountName");
                        if (account != null && account.StringValue == username)
                        {
                            userDn = entity.DN;
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(userDn)) return "AD用户信息错误";
                    try
                    {
                        connection.Bind(userDn, password);
                        return "";
                    }
                    catch (LdapException ex)
                    {
                        return "AD用户密码错误";
                    }
                }
            }
            catch (Exception ex1)
            {
                return "AD验证失败,重新登录";
            }
            finally
            {
            }
        } 
    }
}
