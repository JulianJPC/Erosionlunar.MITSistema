using static System.Environment;
using System.Net.Mail;
using System.Xml;
using MailKit.Net.Imap;
using MailKit;
using MimeKit;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;

namespace Erosionlunar.MITSistema.MailControl
{
    public class EmailSimple
    {
        private string tokenConection { get; set; }
        private string emailABuscar { get; set; }
        private string folderAllEmails { get; set; }

        public EmailSimple(string token, string email, string folderMails)
        {
            tokenConection = token;
            emailABuscar = email;
            folderAllEmails = folderMails;
        }
        public List<Models.MailModel> verMails()
        {
            List<Models.MailModel> listaAllEmails = new List<Models.MailModel>();
            using (var client = new ImapClient())
            {
                List<MailKit.UniqueId> UIDs = new List<MailKit.UniqueId>();
                List<List<string>> etiquetasGmail = new List<List<string>>();
                using (var cancel = new CancellationTokenSource())
                {
                    client.Connect("imap.gmail.com", 993, true, cancel.Token);
                    // If you want to disable an authentication mechanism,
                    // you can do so by removing the mechanism like this:
                    client.AuthenticationMechanisms.Remove("XOAUTH");
                    client.Authenticate(emailABuscar, tokenConection, cancel.Token);
                    // The Inbox folder is always available...
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly, cancel.Token);
                    int numberOfEmailsToFetch = 1;
                    // download each message based on the message index
                    IList<IMessageSummary> mensajes = inbox.Fetch(Math.Max(0, inbox.Count - numberOfEmailsToFetch), -1, MessageSummaryItems.UniqueId | MessageSummaryItems.GMailLabels);
                    foreach (var mensaje in mensajes)
                    {
                        MimeMessage message = inbox.GetMessage(mensaje.UniqueId, cancel.Token);
                        Models.MailModel unNuevoMail = new Models.MailModel();
                        unNuevoMail.copiarDataMail(message);
                        unNuevoMail.setEsInbox(true);
                        unNuevoMail.setEsSent(false);

                        // Add UID and labels
                        unNuevoMail.setUID(mensaje.UniqueId);
                        unNuevoMail.setGmailLabels(new List<string>(mensaje.GMailLabels));

                        listaAllEmails.Add(unNuevoMail);
                    }
                    var sentInbox = client.GetFolder(MailKit.SpecialFolder.Sent);
                    sentInbox.Open(FolderAccess.ReadOnly);
                    // Fetch all sent messages with UniqueId and GMailLabels
                    IList<IMessageSummary> sentMensajes = sentInbox.Fetch(Math.Max(0, inbox.Count - numberOfEmailsToFetch), -1, MessageSummaryItems.UniqueId | MessageSummaryItems.GMailLabels);
                    foreach (var sentMensaje in sentMensajes)
                    {
                        MimeMessage message = sentInbox.GetMessage(sentMensaje.UniqueId, cancel.Token);
                        Models.MailModel unNuevoMail = new Models.MailModel();
                        unNuevoMail.copiarDataMail(message);
                        unNuevoMail.setEsInbox(false);
                        unNuevoMail.setEsSent(true);

                        // Add UID and labels
                        unNuevoMail.setUID(sentMensaje.UniqueId);
                        unNuevoMail.setGmailLabels(new List<string>(sentMensaje.GMailLabels));

                        listaAllEmails.Add(unNuevoMail);
                    }

                    client.Disconnect(true, cancel.Token);
                }
            }
            return listaAllEmails;
        }
        //public Models.MailModel GetSingleMailByIndex(int emailIndex)
        //{
        //    Models.MailModel mail = null;
        //    using (var client = new ImapClient())
        //    {
        //        using (var cancel = new CancellationTokenSource())
        //        {
        //            client.Connect("imap.gmail.com", 993, true, cancel.Token);
        //            client.AuthenticationMechanisms.Remove("XOAUTH");
        //            client.Authenticate(emailABuscar, tokenConection, cancel.Token);

        //            var inbox = client.Inbox;
        //            inbox.Open(FolderAccess.ReadOnly, cancel.Token);

        //            if (emailIndex < inbox.Count)
        //            {
        //                // Fetch the message at the specified index
        //                MimeMessage message = inbox.GetMessage(emailIndex, cancel.Token);

        //                // Map the message to your MailModel
        //                mail = new Models.MailModel();
        //                mail.copiarDataMail(message);
        //                mail.esInbox = true;
        //                mail.esSent = false;

        //                // Optionally, fetch Gmail labels or other metadata
        //                var messageSummary = inbox.Fetch(new List<int> { emailIndex }, MessageSummaryItems.UniqueId | MessageSummaryItems.GMailLabels);
        //                if (messageSummary.Any())
        //                {
        //                    mail.UID = messageSummary[0].UniqueId;
        //                    mail.gmailLabels = new List<string>(messageSummary[0].GMailLabels);
        //                }
        //            }

        //            client.Disconnect(true, cancel.Token);
        //        }
        //    }
        //    return mail;
        //}
        public string getFolderAll()
        {
            return folderAllEmails;
        }
        public void GetAllMail()
        {
            using (var client = new ImapClient())
            {
                using (var cancel = new CancellationTokenSource())
                {
                    client.Connect("imap.gmail.com", 993, true, cancel.Token);
                    client.AuthenticationMechanisms.Remove("XOAUTH");
                    client.Authenticate(emailABuscar, tokenConection, cancel.Token);

                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly, cancel.Token);

                    var messages = inbox.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope);
                    int count = 1;
                    foreach (var messageSummary in messages)
                    {
                        MimeMessage message = inbox.GetMessage(messageSummary.UniqueId);
                        SaveEmailToDisk(message, count++);
                    }
                    var sentInbox = client.GetFolder(MailKit.SpecialFolder.Sent);
                    sentInbox.Open(FolderAccess.ReadOnly);

                    var messagesSent = sentInbox.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope);
                    foreach (var messageSummary in messagesSent)
                    {
                        MimeMessage message = sentInbox.GetMessage(messageSummary.UniqueId);
                        SaveEmailToDisk(message, count++);
                    }
                    client.Disconnect(true, cancel.Token);
                }
            }
        }
        private void SaveEmailToDisk(MimeMessage message, int index)
        {
            try
            {
                // Generate a unique file name based on the email subject or index
                string fileName = "mail.eml";
                string filePath = Path.Combine(folderAllEmails, fileName);

                // Ensure unique file names if there are duplicates
                int count = 1;
                while (System.IO.File.Exists(filePath))
                {
                    fileName = $"mail({count++}).eml";
                    filePath = Path.Combine(folderAllEmails, fileName);
                }
                // Save the email as an .eml file
                using (var stream = System.IO.File.Create(filePath))
                {
                    message.WriteTo(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save email '{message.Subject}': {ex.Message}");
            }
        }
        public MemoryStream SaveEmail(string emailId)
        {
            MemoryStream stream = new MemoryStream();
            using (var client = new ImapClient())
            {
                using (var cancel = new CancellationTokenSource())
                {
                    client.Connect("imap.gmail.com", 993, true, cancel.Token);
                    client.AuthenticationMechanisms.Remove("XOAUTH");
                    client.Authenticate(emailABuscar, tokenConection, cancel.Token);

                    // Select the Inbox folder
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite, cancel.Token);

                    // Find the email by UniqueId or emailId
                    var uids = inbox.Search(SearchQuery.HeaderContains("Message-ID", emailId));

                    if (uids.Count == 1)
                    {
                        var message = inbox.GetMessage(uids[0], cancel.Token);

                        // Save the email content to a memory stream
                        
                        message.WriteTo(stream);
                        stream.Position = 0; // Reset the stream position to the beginning

                        // Delete the email from the inbox
                        inbox.AddFlags(uids[0], MessageFlags.Deleted, true, cancel.Token);
                        inbox.Expunge(cancel.Token); // Ensure it's permanently deleted

                        client.Disconnect(true, cancel.Token);
                        // Return the email as a downloadable file
                        //return File(stream, "message/rfc822", $"{emailId}.eml");
                    }
                }
            }
            return stream;
        }
    }
}
