using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Erosionlunar.MITSistema.Entities;
using Erosionlunar.MITSistema.Models;
using Mysqlx.Prepare;
using MimeKit;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Erosionlunar.MITSistema.Controllers
{
    public class PartesController : Controller
    {
        private readonly MyAppContext _context;
        private string pathToPartes;
        public PartesController(MyAppContext context)
        {
            _context = context;
            var elPathRaw = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 1);
            pathToPartes = elPathRaw?.Valores ?? string.Empty;
        }

        // GET: Partes
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PartesList()
        {
            var Partes = _context.Partes.ToList();
            List<PartesFixModel> PartesArreglo = new List<PartesFixModel>();
            for (int i = Partes.Count - 1; i >= 0; i--)
            {
                var elParte = new PartesFixModel(Partes[i]);
                var Empresa = _context.Empresas.FirstOrDefault(c => c.IdEmpresa == elParte.idEmpresaV);
                if (Empresa != null)
                {
                    elParte.setNombresE(Empresa);
                }
                PartesArreglo.Add(elParte);
            }
            return View(PartesArreglo);
        }
        public ActionResult PartesDetail(int id)
        {
            var Parte = _context.Partes.FirstOrDefault(c => c.idParte == id);

            if (Parte != null)
            {
                //Trae Parte, losMO y Archivos
                PartesFixModel ParteArreglo = new PartesFixModel(Parte);
                var empresa = _context.Empresas.FirstOrDefault(c => c.IdEmpresa == ParteArreglo.idEmpresaV);
                if (empresa != null)
                {
                    ParteArreglo.setNombresE(empresa);
                }
                var losMOParte = _context.MediosOpticos.Where(c => c.IdParte == Parte.idParte).ToList();
                List<MediosOpticosFixModel> losMOFixParte = new List<MediosOpticosFixModel>();
                foreach (MediosOpticosModel unMO in losMOParte)
                {
                    losMOFixParte.Add(new MediosOpticosFixModel(unMO));
                }
                ParteArreglo.setMOs(losMOFixParte);
                for (int i = 0; i < ParteArreglo.getCantidadMOs(); i++)
                {
                    int elIdMO = ParteArreglo.getIdMO(i);
                    var losA = _context.Archivos.Where(c => c.IdMedioOptico == elIdMO).ToList();
                    var losAFix = new List<ArchivosFixModel>();
                    foreach (ArchivosModel unA in losA)
                    {
                        var unAFix = new ArchivosFixModel(unA);
                        losAFix.Add(unAFix);
                    }
                    ParteArreglo.setArchivosDeMO(losAFix, i);
                    for(int j = 0; j < ParteArreglo.getQAdeMO(i); j++)
                    {
                        var libro = _context.Libros.FirstOrDefault(c => c.IdLibro == ParteArreglo.getIDLAdeMO(i, j));
                        if (libro != null)
                        {
                            ParteArreglo.setByIndiceLibroA(i,j, libro);
                        }
                        
                    }
                    if (losA != null)
                    {
                        ParteArreglo.setArchivosDeMO(losAFix, i);
                    }
                }
                //Busca los Mails y pone en Parte
                string pathToEml = Path.Combine(pathToPartes, ParteArreglo.numeroPV.ToString());
                List<string> emlFiles = Directory.GetFiles(pathToEml, "*.eml").ToList();
                List<string> mensajesMail = new List<string>();
                List<MailModel> losMails = new List<MailModel>();
                foreach (string pathUnMail in emlFiles)
                {
                    using (var stream = System.IO.File.OpenRead(pathUnMail))
                    {
                        MailModel unMail = new MailModel(MimeMessage.Load(stream), pathUnMail);
                        losMails.Add(unMail);
                    }
                }
                ParteArreglo.setMails(losMails);
                return View(ParteArreglo);
            }
            else
            {
                return View("MyError", $"Parte No encontrado, la idParte: {id}");
            }

        }
        public ActionResult PartesEdit(int? id)
        {
            int idInt;

            if (id == null)
            {
                return NotFound();
            }
            else
            {
                idInt = id ?? 0;
            }
            var unParte = _context.Partes.FirstOrDefault(c => c.idParte == idInt);
            if (unParte == null)
            {
                return View("MyError", $"IdParte no encontrada: {idInt}");
            }
            else
            {
                PartesFixModel elParte = new PartesFixModel(unParte);
                var Empresas = _context.Empresas.FirstOrDefault(c => c.IdEmpresa == elParte.idEmpresaV);
                if (Empresas != null)
                {
                    elParte.setNombresE(Empresas);
                }
                return View(elParte);
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PartesEdit(int? id, string cosa)
        {
            PartesFixModel elParte = new PartesFixModel();
            elParte.setIdParte(stringToInt(Request.Form["idParteV"]));
            elParte.setIdEmpresa(stringToInt(Request.Form["idEmpresaV"]));
            elParte.setNumeroP(stringToInt(Request.Form["numeroPV"]));
            elParte.setFechaP(stringToDateTimeV(Request.Form["FechaPV"]));
            elParte.setComentario(Request.Form["ComentarioV"]);
            elParte.setDD(Request.Form["DestinatariosDireccionV"]);
            elParte.setDE(Request.Form["DestinatariosEmailV"]);
            elParte.setD(Request.Form["DestinatariosV"]);
            PartesModel unParte = new PartesModel(elParte);
            try
            {
                _context.Update(unParte);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return View("MyError", $"No existe el parte de ID: {unParte.idParte}\nError DB");    
            }
            return RedirectToAction("ParteDetail", new { id = elParte.idParteV });
        }

        private bool PartesModelExists(int? id)
        {
            return _context.Partes.Any(e => e.idParte == id);
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
            if (valor == null)
            {
                return DateTime.MinValue;
            }
            else
            {
                List<string> stringPartido = valor.Split('-').ToList();
                valorBien = String.Join('/', stringPartido[2], stringPartido[1], stringPartido[0]);
            }
            if (!Regex.IsMatch(valorBien, @"^[0-9]{2}/[0-9]{2}/[0-9]{4}$"))
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
