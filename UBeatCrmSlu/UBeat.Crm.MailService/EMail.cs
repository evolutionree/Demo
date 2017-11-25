using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.MailService.Mail.Helper;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Threading;
using System.Threading.Tasks;
using UBeat.Crm.MailService.Mail.Enum;
using MailKit.Net.Pop3;
using MailKit.Net.Imap;
using MailKit.Search;

namespace UBeat.Crm.MailService
{
    public class EMail
    {

        public async Task<MimeMessage> SendMessageAsync(string host, int port, string userAccount, string userPassword, MimeMessage message, bool enableSsl = true, bool isPostFile = false)
        {
            return await Task.Run(() =>
             {
                 using (SmtpClient client = new SmtpHelper(host, port, enableSsl, userAccount, userPassword).SmtpClient)
                 {
                     try
                     {
                         client.Send(message);
                         return message;
                     }
                     catch (Exception ex)
                     {
                         client.Disconnect(true);
                         throw ex;
                     }
                     finally
                     {
                         if (client.IsConnected)
                             client.Disconnect(true);
                     }
                 }
             });
        }

        public async Task<List<MimeMessage>> ImapRecMessageAsync(string host, int port, string userAccount, string userPassword, SearchQuery searchQuery, bool enableSsl)
        {
            return await Task.Run(() =>
            {
                using (ImapClient client = new ImapHelper(host, port, enableSsl, userAccount, userPassword).ImapClient)
                {
                    var inBox = client.GetFolder("INBOX");
                    try
                    {
                        inBox.Open(FolderAccess.ReadWrite);
                        List<MimeMessage> lstMsg = new List<MimeMessage>();
                        if (searchQuery != null)
                        {
                            var uids = inBox.Search(searchQuery);

                            foreach (var item in uids)
                            {
                                client.NoOp();
                                MimeMessage message = inBox.GetMessage(new UniqueId(item.Id));
                                inBox.SetFlags(item, MessageFlags.Seen, true);
                                lstMsg.Add(message);
                            }
                        }
                        return lstMsg;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        inBox.Close();
                        if (client.IsConnected)
                            client.Disconnect(true);
                    }
                }
            });
        }
    }
}
