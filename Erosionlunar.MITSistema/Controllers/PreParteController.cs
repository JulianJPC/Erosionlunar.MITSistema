using Erosionlunar.MITSistema.Entities;
using Erosionlunar.MITSistema.MailControl;
using Erosionlunar.MITSistema.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using Mysqlx.Prepare;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Erosionlunar.MITSistema.Controllers
{
    public class PreParteController : Controller
    {
        private readonly MyAppContext _context;
        private string pathToPartes;
        private string elToken;
        private string elEmailDeBusqueda;
        private string pathToFolderEmails;
        public PreParteController(MyAppContext context)
        {
            _context = context;
            var elPathRaw = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 1);
            var tokenMail = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 2);
            var emailAUsar = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 3);
            var folderDeLosMails = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 4);
            pathToPartes = elPathRaw?.Valores ?? string.Empty;
            elToken = tokenMail?.Valores ?? string.Empty;
            elEmailDeBusqueda = emailAUsar?.Valores ?? string.Empty;
            pathToFolderEmails = folderDeLosMails?.Valores ?? string.Empty;
        }
        // GET: PreParteController
        public ActionResult Index()
        {
            return View();
        }
        
        // GET: PreParteController
        public ActionResult PreParteDinamico()
        {
            EmailSimple controladorMails = new EmailSimple(elToken, elEmailDeBusqueda, pathToFolderEmails);
            //controladorMails.GetAllMail();
            List<Models.MailModel> losMails = new List<Models.MailModel>();
            string folderMails = controladorMails.getFolderAll();
            List<string> emlFiles = Directory.GetFiles(folderMails, "*.eml").ToList();
            foreach(string pathUnMail in emlFiles)
            {
                using (var stream = System.IO.File.OpenRead(pathUnMail))
                {
                    // Parse the .eml file and return the MimeMessage
                    losMails.Add(new MailModel(MimeMessage.Load(stream), pathUnMail));
                }
            }           
            List<MailModel> sortedEmails = losMails.OrderBy(email => email.fechaV).ToList();
            return View(sortedEmails);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PreParteDinamico(int accion, string pathInput, string numberInput)
        {
            //Preparando Input
            string parte = numberInput;
            string pathFolder = Path.Combine(pathToPartes, parte);
            List<MailModel> losMails = new List<MailModel>();
            int cantidadMails = -1;
            int cantidadCorrecta = 0;
            bool esNull = false;
            for (int i=0; i < 1000 & esNull == false;i++)
            {
                string? unId = Request.Form[$"losMails[{i}]Id"];
                string? unPath = Request.Form[$"losMails[{i}]pathEnDisco"];
                string? unaFecha = Request.Form[$"losMails[{i}]fecha"];
                if(unId != null & unPath != null & unaFecha != null)
                {
                    var unMail = new MailModel();
                    unMail.setId(unId);
                    unMail.setPathDisco(unPath);
                    DateTimeOffset laF = DateTimeOffset.Parse(unaFecha);
                    unMail.setFecha(laF);
                    losMails.Add(unMail);
                    cantidadCorrecta+= 1;
                }
                else if(unId == null & unPath == null & unaFecha == null)
                {
                    esNull = true;
                }
                cantidadMails++;
            }
            //Error Input
            if(losMails.Count == 0 || cantidadCorrecta != cantidadMails)
            {
                return View("MyError", $"Error. Cantidad de Mails: {losMails.Count}\nCantidad Correcta: {cantidadCorrecta}");
            }
            foreach(MailModel unMail in losMails)
            {
                unMail.getOnlyMails();
            }
            
            //Acciones
            if (accion == 0)//Guardar en Disco
            {
                foreach (MailModel unMail in losMails)
                {
                     unMail.moverAPath(pathInput);
                }
            }
            else if (accion == 1)//Mover a parte
            {   
                foreach (MailModel unMail in losMails)
                {
                    moverEmailAParte(pathFolder, parte, unMail);
                }
            }
            else if(accion == 2)//Crear PreParte y mover
            {
                //Preparo PreParteModel
                PreParteModel elPreParte = new PreParteModel();
                int numeroPreP = _context.PreParte.Max(p => p.idPreParte) + 1 ?? 0;
                int numeroP = _context.Partes.Max(p => p.numeroP) + 1 ?? 0;
                DateTimeOffset laFechaPrincipio = DateTime.Now;
                foreach (MailModel unMail in losMails)
                {
                    if (unMail.fechaV < laFechaPrincipio)
                    {
                        laFechaPrincipio = unMail.fechaV;
                    }
                }
                elPreParte.idPreParte = numeroPreP;
                elPreParte.numeroP = numeroP;
                elPreParte.fechaRecibido =laFechaPrincipio.ToString("dd/MM/yyyy");

                //Da un aproximado de IdEmpresa dado que empresa se repite más
                List<int> numeroDeVeces = new List<int>();
                List<int> idEmpresas = new List<int>();
                List<string> allEmailsDirs = new List<string>();
                foreach(MailModel unMail in losMails)
                {
                    allEmailsDirs.AddRange(unMail.devolverMails());//obtengo los string de emails
                }
                foreach(string unaDir in allEmailsDirs)
                {
                    //Veo los idEmpresas de los mails y cuento cuantas veces se repiten
                    var laInfo = _context.InformacionEmpresa.FirstOrDefault(c => c.Informacion == unaDir.Replace("\"", ""));
                    if(laInfo != null)
                    {
                        int elIdDeLaEmpresas = laInfo.IdEmpresa ?? 0;
                        if (idEmpresas.Contains(elIdDeLaEmpresas))
                        {
                            int indiceEmp = idEmpresas.IndexOf(elIdDeLaEmpresas);
                            numeroDeVeces[indiceEmp]++;
                        }
                        else
                        {
                            idEmpresas.Add(elIdDeLaEmpresas);
                            numeroDeVeces.Add(1);
                        }
                    }
                }
                int indiceBigger = numeroDeVeces.IndexOf(numeroDeVeces.Max());//obtengo el indice de idEmpresa que se repite mas
                elPreParte.idEmpresa = idEmpresas[indiceBigger];
                //Crea el Parte y mueve los mails
                try
                {
                    _context.PreParte.Add(elPreParte);
                    _context.SaveChanges();
                    Directory.CreateDirectory(pathFolder);
                    Directory.CreateDirectory(Path.Combine(pathFolder, "txt"));
                    foreach (MailModel unMail in losMails)
                    {
                        unMail.getAtachments(Path.Combine(pathFolder, "txt"));
                    }
                    foreach (MailModel unMail in losMails)
                    {
                        moverEmailAParte(pathFolder, parte, unMail);
                    }
                    return RedirectToAction("PreParteEdit", new { id = elPreParte.idPreParte });
                }
                catch(Exception ex)
                {
                    return View("MyError", ex.Message);
                }
            }
            else
            {
                return View("MyError", $"Accion invalida: {accion}");
            }

            return RedirectToAction("PreParteDinamico");
        }
        private void moverEmailAParte(string pathFolder, string parte, MailModel unMail)
        {
            int number = 0;
            Regex elRegex = new Regex(@"^.*[0-9]{5,6}_[0-9]{1,2}\.eml$", RegexOptions.Compiled);
            List<string> emlFiles = Directory.GetFiles(pathFolder, "*.eml").ToList();
            foreach (string emlFile in emlFiles)
            {
                if (elRegex.IsMatch(emlFile))
                {
                    string fifthFromEnd = emlFile[emlFile.Length - 5].ToString();
                    string sixthFromEnd = emlFile[emlFile.Length - 6].ToString();
                    int numberEml;
                    if (sixthFromEnd != "_")
                    {
                        numberEml = Int32.Parse(fifthFromEnd + sixthFromEnd);
                    }
                    else
                    {
                        numberEml = Int32.Parse(fifthFromEnd);
                    }
                    if (numberEml >= number)
                    {
                        number = numberEml + 1;
                    }
                }
            }
            string pathToFile = Path.Combine(pathFolder, $"{parte}_{number}.eml");
            System.IO.File.Move(unMail.pathEnDiscoV, pathToFile);
        }
        // POST: PreParteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DescargarMailNormal(MailModel elModel)
        {
            EmailSimple controladorEmail = new EmailSimple(elToken, elEmailDeBusqueda, pathToFolderEmails);
            MemoryStream theStream = controladorEmail.SaveEmail(elModel.idV.ToString());
            if (theStream != null)
            {
                return File(theStream, "message/rfc822", $"{elModel.temaV}.eml");
            }
            else
            {
                return NotFound(); // Return 404 if the record is not found
            }
        }
        // POST: PreParteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DescargarMailEnParte(MailModel elModel)
        {
            EmailSimple controladorEmail = new EmailSimple(elToken, elEmailDeBusqueda, pathToFolderEmails);
            MemoryStream theStream = controladorEmail.SaveEmail(elModel.idV.ToString());
            string pathFolder = Path.Combine(pathToPartes, elModel.preParteV.ToString());
            string pathFileTemp = Path.Combine(pathToPartes, elModel.preParteV.ToString(), "emailDescargado.eml");
            int count = 1;
            while (System.IO.File.Exists(pathFileTemp))
            {
                string newFileName = $"emailDescargado({count}).eml";
                pathFileTemp = Path.Combine(pathToPartes, elModel.preParteV.ToString(), newFileName);
                count++;
            }

            if (theStream != null && Directory.Exists(pathFolder))
            {
                using (var fileStream = new FileStream(pathFileTemp, FileMode.Create, FileAccess.Write))
                {
                    theStream.Position = 0; // Reset the stream position to the beginning
                    theStream.CopyTo(fileStream); // Copy the content of MemoryStream to the FileStream
                }
                List<string> emlFiles = Directory.GetFiles(pathFolder, "*.eml").ToList();
                int number = 0;
                Regex elRegex = new Regex(@"^.*[0-9]{5,6}_[0-9]{1,2}\.eml$", RegexOptions.Compiled);
                foreach (string emlFile in emlFiles)
                {
                    if (elRegex.IsMatch(emlFile))
                    {
                        string fifthFromEnd = emlFile[emlFile.Length - 5].ToString();
                        string sixthFromEnd = emlFile[emlFile.Length - 6].ToString();
                        int numberEml;
                        if (sixthFromEnd != "_")
                        {
                            numberEml = Int32.Parse(fifthFromEnd + sixthFromEnd);
                        }
                        else
                        {
                            numberEml = Int32.Parse(fifthFromEnd);
                        }
                        if (numberEml >= number)
                        {
                            number = numberEml + 1;
                        }
                    }
                }
                string pathToFile = Path.Combine(pathFolder, $"{elModel.preParteV}_{number}.eml");
                System.IO.File.Move(pathFileTemp, pathToFile);


                return RedirectToAction("PreParteDinamico");
            }
            else
            {
                return NotFound(); // Return 404 if the record is not found
            }
        }
        public ActionResult PreParteList()
        {
            var PrePartes = _context.PreParte.ToList();
            List<PreParteFixModel> PrePartesFix = new List<PreParteFixModel>();
            for (int i = PrePartes.Count - 1; i >= 0; i--)
            {
                var nuevoPreParte = new PreParteFixModel(PrePartes[i]);
                int idEmp = nuevoPreParte.idEmpresaV;
                nuevoPreParte.setNombreEmpresa(getNombreCEmpresa(idEmp));
                PrePartesFix.Add(nuevoPreParte);  
            }

            return View(PrePartesFix);
        }

        public ActionResult PreParteDetail(int id)
        {
            var preParte = _context.PreParte.FirstOrDefault(p => p.idPreParte == id);
            if (preParte == null || id == 0)
            {
                return View("MyError", $"No existe PreParte con la ID: {id}");
            }
            var elPreParte = new PreParteFixModel(preParte);
            elPreParte.setNombreEmpresa(getNombreCEmpresa(elPreParte.idEmpresaV));
            elPreParte.setMails(getDirsEmail(elPreParte.numeroPV));
            return View(elPreParte);
        }

        // GET: PreParteController/Create
        public ActionResult PreParteNew()
        {
            var empresas = _context.Empresas.Select(c => new
            {
                c.IdEmpresa, // Assuming there's an Id property
                c.NombreCortoE // Assuming there's a Name property
            }).ToList();

            // Create a SelectList and pass it to the view using ViewBag
            ViewBag.empresas = new SelectList(empresas, "IdEmpresa", "NombreCortoE");
            var maxNParte = _context.Partes.Max(p => p.numeroP);
            var preParteModel = new Models.PreParteFixModel();
            preParteModel.setFechaRecibido(DateTime.Now);
            preParteModel.setNumeroDeParte(maxNParte + 1 ?? 0);
            preParteModel.setIdEmpresa(-1);
            return View(preParteModel);
        }

        // POST: PreParteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PreParteNew(int id)
        {
            var elNuevoPreParte = new PreParteModel();
            elNuevoPreParte.idEmpresa = stringToInt(Request.Form["idEmpresaV"]);
            elNuevoPreParte.fechaRecibido = stringToDateTimeV(Request.Form["fechaRecibidoV"]).ToString("dd/MM/yyyy");
            elNuevoPreParte.numeroP = stringToInt(Request.Form["numeroPV"]);
            int numeroIDNuevo = _context.PreParte.Max(p => p.idPreParte) + 1 ?? 0;
            elNuevoPreParte.idPreParte = numeroIDNuevo;
            try
            {
                _context.PreParte.Add(elNuevoPreParte);
                _context.SaveChanges();
                return RedirectToAction("PreParteEdit", new { id = elNuevoPreParte.idPreParte });
            }
            catch (Exception ex)
            {
                return View("MyError", ex.Message);
            }
        }

        // GET: PreParteController/Edit/5
        public ActionResult PreParteEdit(int id)
        {
            var preParte = _context.PreParte.FirstOrDefault(p => p.idPreParte == id);
            PreParteFixModel elPreParte;
            // Check if PreParte is found
            if (preParte == null)
            {
                return NotFound(); // Return 404 if the record is not found
            }
            else
            {
                elPreParte = new PreParteFixModel(preParte);
                elPreParte.setMails(getDirsEmail(elPreParte.numeroPV));
                var Mails = _context.InformacionEmpresa
                .Where(m => m.IdEmpresa == elPreParte.idEmpresaV && m.IdTipoInformacion == 1)
                        .ToList();
                Mails.Reverse();
                ViewBag.contactosMail = Mails;
            }
            return View(elPreParte);
        }

        // POST: PreParteController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PreParteEdit(int id, List<string> losIdNyE, List<string> losIdMail)
        {
            //Preparando Input
            PreParteFixModel elPreParteNew = new PreParteFixModel();
            elPreParteNew.setIdPreParte(stringToInt(Request.Form["idPreParteV"]));
            elPreParteNew.setNumeroDeParte(stringToInt(Request.Form["numeroPV"]));
            DateTime laFecha = stringToDateTimeV(Request.Form["fechaRecibidoV"]);
            elPreParteNew.setFechaRecibido(laFecha);
            elPreParteNew.setIdEmpresa(stringToInt(Request.Form["idEmpresaV"]));
            elPreParteNew.setMailsDeView(losIdMail, losIdNyE);
            //Errores Input
            var elPreParteOldRaw = _context.PreParte.FirstOrDefault(p => p.idPreParte == id);
            if (elPreParteOldRaw == null | elPreParteNew.idPreParteV == 0)
            {
                string errorMessage = $"El PreParte de ID: {id}, no existe";
                if(elPreParteNew.idPreParteV == 0)
                {
                    errorMessage = "La información que llega al controler es Nula.";
                }
                return View("MyError", errorMessage);
            }
            
            //Creando variables fix
            var elPreParteOld = new PreParteFixModel(elPreParteOldRaw);
            var mailsDeParteOldRaw = _context.MailsDeParte.AsNoTracking().Where(m => m.numeroP == elPreParteOld.numeroPV.ToString()).ToList();
            var mailsDeParteOld = new List<MailsDeParteFixModel>();
            foreach(MailsDeParteModel unMail in mailsDeParteOldRaw)
            {
                var unMailFix = new MailsDeParteFixModel(unMail);
                var laInfoEmpresa = getInfoEmpresa(unMailFix.idNombreYMailV);
                unMailFix.setNombreYMail(laInfoEmpresa);
                mailsDeParteOld.Add(unMailFix);
            }
            elPreParteOld.setIdEmpresa(elPreParteNew.idEmpresaV);
            elPreParteOld.setFechaRecibido(elPreParteNew.fechaRecibidoV);
            elPreParteOld.setMails(mailsDeParteOld);

            //Updateando cambio numeroP
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (elPreParteOld.numeroPV != elPreParteNew.numeroPV) // si el numero de Parte Cambio
                    {
                        _context.Database.ExecuteSqlRaw(
                            "UPDATE PreParte SET numeroP = {0} WHERE idPreParte = {1}",
                            elPreParteNew.numeroPV, elPreParteOld.idPreParteV
                        );

                        _context.Database.ExecuteSqlRaw(
                            "UPDATE MailsDeParte SET numeroP = {0} WHERE numeroP = {1}",
                            elPreParteNew.numeroPV.ToString(), elPreParteOld.numeroPV.ToString()
                        );
                        elPreParteOld.setNumeroDeParteMails(elPreParteNew.numeroPV);
                        elPreParteOld.setNumeroDeParte(elPreParteNew.numeroPV);
                        _context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    // Roll back the transaction if there's an error
                    transaction.Rollback();
                    return View("MyError", ex.Message);
                }
            }
            //Eliminacion y Creacion MailsDePartes
            var losMailsAEliminar = elPreParteOld.devolverMailsDiferentes(elPreParteNew);  //Si no esta en elPreParteNew
            var losMailsACrear = elPreParteNew.devolverMailsDiferentes(elPreParteOld);     //Si no esta en elPreParteOld
            
            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach(var mail in losMailsAEliminar)
                {
                    _context.MailsDeParte.Remove(mail);
                }
                int ultimoId = _context.MailsDeParte.Max(p => p.idMailsDeParte) + 1 ?? 1;
                foreach (var mail in losMailsACrear)
                {
                    if (mail.numeroP != null)
                    {
                        var exist = _context.MailsDeParte.Any(m => m.idNombreYMail == mail.idNombreYMail && m.numeroP == mail.numeroP.ToString());
                        if (!exist)
                        {
                            // If it's a new mail, you can add it to the database
                            _context.MailsDeParte.Add(new MailsDeParteModel
                            {
                                idNombreYMail = mail.idNombreYMail,
                                numeroP = mail.numeroP.ToString(),
                                tipo = "TO",
                                idMailsDeParte = ultimoId
                            });
                            ultimoId++;
                        }
                    }

                }

            }
            _context.SaveChanges();

            return RedirectToAction("PreParteList");
        }


        private string getNombreCEmpresa(int idE)
        {
            var empresa = _context.Empresas
                                  .Where(c => c.IdEmpresa == idE)
                                  .Select(c => c.NombreCortoE)
                                  .FirstOrDefault();
            string nCortoE = empresa ?? "NULL";
            return nCortoE;
        }
        private InformacionEmpresaModel getInfoEmpresa(int idInfo)
        {
            var InfoEmpresa = new InformacionEmpresaModel();
            var laInfo = _context.InformacionEmpresa.FirstOrDefault(p => p.IdInformacionEmpresa == idInfo);
            if(laInfo != null)
            {
                InfoEmpresa = laInfo;
            }
            return InfoEmpresa;
        }
        private List<MailsDeParteFixModel> getDirsEmail(int numeroP)
        {
            var tempDirsMails = _context.MailsDeParte.AsNoTracking().Where(m => m.numeroP == numeroP.ToString());

            var losMails = tempDirsMails.Select(mail => new MailsDeParteFixModel(mail)).ToList();
            foreach(MailsDeParteFixModel unMail in losMails)
            {
                unMail.setNombreYMail(getInfoEmpresa(unMail.idNombreYMailV));
            }
            return losMails;
        }
        private int stringToInt(string? valor)
        {
            int elNumero = 0;
            bool esNumero = Int32.TryParse(valor, out elNumero);
            return elNumero;
        }
        private DateTime stringToDateTimeV(string? valor)
        {
            DateTime laFecha;
            string valorBien;
            if(valor == null)
            {
                return DateTime.MinValue;
            }
            else
            {
                List<string> stringPartido = valor.Split('-').ToList();
                valorBien = String.Join('/', stringPartido[2], stringPartido[1], stringPartido[0]);
            }
            if(!Regex.IsMatch(valorBien, @"^[0-9]{2}/[0-9]{2}/[0-9]{4}$"))
            {
                return DateTime.MinValue;
            }
            else
            {
                laFecha = DateTime.ParseExact(valorBien, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                return laFecha;
            }
        }
    }
}
