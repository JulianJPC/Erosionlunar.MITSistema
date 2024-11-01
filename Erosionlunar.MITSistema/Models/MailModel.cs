using MailKit;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace Erosionlunar.MITSistema.Models
{
    public class MailModel
    {
        private string id;
        private string idMailAnteriorCadena;
        private string tema;
        private string mensaje;
        private DateTimeOffset fecha;
        private List<string> emailsYNombresTO;
        private List<string> emailsYNombresFROM;
        private List<string> emailsYNombresCC;
        private bool esInbox;
        private bool esSent;
        private UniqueId UID;
        private List<string> gmailLabels;
        private int preParte;
        private string pathEnDisco;
        private string mensajeCorto;

        public string idV => id;
        
        public string pathEnDiscoV => pathEnDisco;
        [Display(Name = "Fecha Mail")]
        public DateTimeOffset fechaV => fecha;
        public string temaV => tema;
        public List<string> emailsYNombresTOV => emailsYNombresTO;
        public List<string> emailsYNombresFROMV => emailsYNombresFROM;
        public List<string> emailsYNombresCCV => emailsYNombresCC;
        public string mensajeV => mensaje;
        public int preParteV => preParte;
        public string mensajeCortoV => mensajeCorto;

        public MailModel() { }
        public MailModel(MimeMessage unMail, string path)
        {
            id = unMail.MessageId;
            idMailAnteriorCadena = unMail.InReplyTo;
            tema = unMail.Subject;
            mensaje = unMail.TextBody;
            fecha = unMail.Date;
            emailsYNombresTO = new List<string>();
            foreach (var email in unMail.To)
            {
                emailsYNombresTO.Add(email.ToString());
            }
            emailsYNombresFROM = new List<string>();
            foreach (var email in unMail.From)
            {
                emailsYNombresFROM.Add(email.ToString());
            }
            emailsYNombresCC = new List<string>();
            foreach (var email in unMail.Cc)
            {
                emailsYNombresCC.Add(email.ToString());
            }
            pathEnDisco = path;
            gmailLabels = new List<string>();
            mensajeCorto = obtainMensajeCorto(unMail);
        }
        public void setEsInbox(bool siONo)
        {
            esInbox = siONo;
        }
        public void setEsSent(bool siONo)
        {
            esSent = siONo;
        }
        public void setUID(UniqueId laUID)
        {
            UID = laUID;
        }
        public void setId(string laId)
        {
            id = laId;
        }
        public void setPathDisco(string elPath)
        {
            pathEnDisco = elPath;
        }
        public void setFecha(DateTimeOffset laFecha)
        {
            fecha = laFecha;
        }
        public void setGmailLabels(List<string> lasLabels)
        {
            gmailLabels = lasLabels;
        }
        private string obtainMensajeCorto(MimeMessage unMail)
        {
            var textPart = unMail.TextBody ?? unMail.HtmlBody;
            string originalContent = "";
            if (!string.IsNullOrEmpty(textPart))
            {
                // Split the email body into lines and filter out quoted sections
                var lines = textPart.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (var line in lines)
                {
                    // Skip lines that start with ">" or contain common reply patterns
                    if (!line.TrimStart().StartsWith(">") &&
                        !line.TrimStart().StartsWith("On ") &&
                        !line.Contains("wrote:"))
                    {
                        originalContent += line + Environment.NewLine;
                    }
                }
            }
            return originalContent;
        }
        public void getAtachments(string pathACopiar)
        {
            MimeMessage elMail = new MimeMessage();
            using (var stream = System.IO.File.OpenRead(pathEnDisco))
            {
                // Parse the .eml file and return the MimeMessage
                elMail = MimeMessage.Load(stream);
            }
            foreach(var unAttach in elMail.Attachments)
            {
                if (unAttach is MimePart mimePart)
                {
                    var fileName = mimePart.FileName;

                    // Set the output path for the attachment
                    var filePath = Path.Combine(pathACopiar, fileName);

                    // Save the attachment to disk
                    using (var fileStream = File.Create(filePath))
                    {
                        mimePart.Content.DecodeTo(fileStream);
                    }
                }
            }
        }
        public void getOnlyMails()
        {
            MimeMessage elMail = new MimeMessage();
            using (var stream = System.IO.File.OpenRead(pathEnDisco))
            {
                // Parse the .eml file and return the MimeMessage
                elMail = MimeMessage.Load(stream);
            }
            emailsYNombresTO = new List<string>();
            foreach (var email in elMail.To)
            {
                emailsYNombresTO.Add(email.ToString());
            }
            emailsYNombresFROM = new List<string>();
            foreach (var email in elMail.From)
            {
                emailsYNombresFROM.Add(email.ToString());
            }
            emailsYNombresCC = new List<string>();
            foreach (var email in elMail.Cc)
            {
                emailsYNombresCC.Add(email.ToString());
            }
        }
        public List<string> devolverMails()
        {
            List<string> listaEmails = new List<string>();
            foreach(string unEmail in emailsYNombresCC)
            {
                listaEmails.Add(unEmail);
            }
            foreach (string unEmail in emailsYNombresFROM)
            {
                listaEmails.Add(unEmail);
            }
            foreach (string unEmail in emailsYNombresTO)
            {
                listaEmails.Add(unEmail);
            }
            return listaEmails;
        }
        public void moverAPath(string nuevoPath)
        {
            int count = 1;
            string nombreMail = Path.GetFileName(pathEnDisco);

            while (File.Exists(Path.Combine(nuevoPath, nombreMail)))
            {
                nombreMail = $"mail({count++}).eml";
            }
            // Save the email as an .eml file
            string dirFinal = Path.Combine(nuevoPath, nombreMail);
            File.Move(pathEnDisco, dirFinal);
        }
        public void copiarDataMail(MimeMessage message)
        {
            id = message.MessageId;
            idMailAnteriorCadena = message.InReplyTo;
            sumarEmails(message.To, message.From, message.Cc);
            mensaje = message.TextBody;
            fecha = message.Date;
            tema = message.Subject;
        }
        private void sumarEmails(MimeKit.InternetAddressList losTO, MimeKit.InternetAddressList losFROM, MimeKit.InternetAddressList losCC)
        {
            emailsYNombresTO = new List<string>();
            emailsYNombresFROM = new List<string>();
            emailsYNombresCC = new List<string>();
            foreach (MimeKit.InternetAddress unaDire in losTO)
            {
                emailsYNombresTO.Add(unaDire.ToString().Replace("\"", "").Replace("\'", ""));
            }
            foreach (MimeKit.InternetAddress unaDire in losFROM)
            {
                emailsYNombresFROM.Add(unaDire.ToString().Replace("\"", "").Replace("\'", ""));
            }
            foreach (MimeKit.InternetAddress unaDire in losCC)
            {
                emailsYNombresCC.Add(unaDire.ToString().Replace("\"", "").Replace("\'", ""));
            }
        }
    }
}
