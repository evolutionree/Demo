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
using System.IO;

namespace UBeat.Crm.MailService
{
    public class EMail
    {
        string[] GetUndeliverableAddresses(MimeMessage message)
        {
            var report = message.Body as MultipartReport;

            if (report == null)
                throw new ArgumentException("This is not a multipart/report!");

            MessageDeliveryStatus status = null;

            for (int i = 0; i < report.Count; i++)
            {
                status = report[i] as MessageDeliveryStatus;
                if (status != null)
                    break;
            }

            if (status == null)
                throw new ArgumentException("Did not contain a message/delivery-status!");

            var undeliverables = new List<string>();
            for (int i = 0; i < status.StatusGroups.Count; i++)
            {
                var recipient = status.StatusGroups[i]["Final-Recipient"];
                int semicolon = recipient.IndexOf(';');

                var undeliverable = recipient.Substring(semicolon + 1).Trim();
                undeliverables.Add(undeliverable);
            }

            return undeliverables.ToArray();
        }

        public async Task<MimeMessage> SendMessageAsync(string host, int port, string userAccount, string userPassword, MimeMessage message, bool enableSsl = true, bool isPostFile = false)
        {
            using (SmtpClient client = new SmtpHelper(host, port, enableSsl, userAccount, userPassword).SmtpClient)
            {
                return await Task.Run(() =>
                 {
                     try
                     {
                         client.Send(message);
                         //var addressLst = GetUndeliverableAddresses(message);
                         return message;
                     }
                     catch (ArgumentNullException ex)
                     {
                         throw ex;
                     }
                     catch (ObjectDisposedException ex)
                     {
                         throw ex;
                     }
                     catch (InvalidOperationException ex)
                     {
                         throw ex;
                     }
                     catch (OperationCanceledException ex)
                     {
                         throw ex;
                     }
                     catch (IOException ex)
                     {
                         throw ex;
                     }
                     catch (CommandException ex)
                     {
                         throw ex;
                     }
                     catch (ProtocolException ex)
                     {
                         throw ex;
                     }
                     finally
                     {
                         if (client.IsConnected)
                             client.Disconnect(true);
                     }
                 });
            }
        }

        public async Task<List<MimeMessage>> ImapRecMessageAsync(string host, int port, string userAccount, string userPassword, SearchQuery searchQuery, bool enableSsl)
        {
            using (ImapClient client = new ImapHelper(host, port, enableSsl, userAccount, userPassword).ImapClient)
            {
                return await Task.Run(() =>
                {
                    var inBox = client.GetFolder("INBOX");
                    try
                    {
                        inBox.Open(FolderAccess.ReadOnly);
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
                });
            }
        }
        public List<MimeMessage> ImapRecMessage(string host, int port, string userAccount, string userPassword, SearchQuery searchQuery, bool enableSsl)
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
        }

        public MimeMessage SendMessage(string host, int port, string userAccount, string userPassword, MimeMessage message, bool enableSsl = true, bool isPostFile = false)
        {
            using (SmtpClient client = new SmtpHelper(host, port, enableSsl, userAccount, userPassword).SmtpClient)
            {
                try
                {
                    client.Send(message);
                    return message;
                }
                catch (ArgumentNullException ex)
                {
                    throw ex;
                }
                catch (ObjectDisposedException ex)
                {
                    throw ex;
                }
                catch (InvalidOperationException ex)
                {
                    throw ex;
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (IOException ex)
                {
                    throw ex;
                }
                catch (CommandException ex)
                {
                    throw ex;
                }
                catch (ProtocolException ex)
                {
                    throw ex;
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect(true);
                }
            }
        }
    }
}
