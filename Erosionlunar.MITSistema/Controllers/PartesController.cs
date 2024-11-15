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
using System.Security.Cryptography;
using System.Security.Policy;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Humanizer;
using System.Text;
using System.IO.Compression;
using NuGet.Packaging;
using Org.BouncyCastle.Asn1;

namespace Erosionlunar.MITSistema.Controllers
{
    public class PartesController : Controller
    {
        private readonly MyAppContext _context;
        private string pathToPartes;
        private string pathToBaseMDB;
        public PartesController(MyAppContext context)
        {
            _context = context;
            var elPathRaw = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 1);
            var BaseMDB = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 13);
            pathToPartes = elPathRaw?.Valores ?? string.Empty;
            pathToBaseMDB = BaseMDB?.Valores ?? string.Empty;
        }

        // GET: Partes
        public ActionResult Index()
        {
            return View();
        }

        //PartesMO2 POST
        private PreParteFixModel getPreParteFromId(string? idPreParte)
        {
            int elIdPreParte = -1;
            if(idPreParte != null)
            {
                elIdPreParte = stringToInt(idPreParte);
            }
            var elPreParteRaw = _context.PreParte.AsNoTracking().FirstOrDefault(c => c.idPreParte == stringToInt(idPreParte));
            PreParteFixModel elPreParte = null;
            if(elPreParteRaw != null)
            {
                elPreParte = new PreParteFixModel(elPreParteRaw);
            }
            return elPreParte;
        }
        private void prepararParte(PartesFixModel elParte, int numeroP, int idEmpresa)
        {
            elParte.setIdParte(_context.Partes.Max(c => c.idParte + 1));
            elParte.setNumeroP(numeroP);
            elParte.setIdEmpresa(idEmpresa);
            elParte.setIdTipoParte(2);
            elParte.setComentario("");
            elParte.setFechaP(DateTime.Now);
            var laEmpresa = _context.Empresas.AsNoTracking().FirstOrDefault(c => c.IdEmpresa == elParte.idEmpresaV);
            elParte.setNombresE(laEmpresa);
            var destDire = _context.InformacionEmpresa.AsNoTracking().FirstOrDefault(c => c.IdTipoInformacion == 2 && c.IdEmpresa == elParte.idEmpresaV);
            elParte.setDD(destDire);
            var losMails = _context.MailsDeParte.AsNoTracking().Where(c => c.numeroP == numeroP.ToString()).ToList();
            foreach (MailsDeParteModel unMailDeParte in losMails)
            {
                var laInfoMail = _context.InformacionEmpresa.AsNoTracking().FirstOrDefault(c => c.IdInformacionEmpresa == unMailDeParte.idNombreYMail);
                elParte.addMailInfoDirs(laInfoMail);
            }
        }
        private List<string> getStringFromRequest(HttpRequest elRequest, string nombreInput)
        {
            bool esNull = false;
            var laLista = new List<string>();

            for (int i = 0; i < 1000 & esNull == false; i++)
            {
                string? elInput = elRequest.Form[nombreInput + $"[{i}]"];
                if (elInput != null)
                {
                    laLista.Add(elInput);
                }
            }
            return laLista;
        }
        private List<int> getIntFromRequest(HttpRequest elRequest, string nombreInput)
        {
            bool esNull = false;
            var laLista = new List<int>();

            for (int i = 0; i < 1000 & esNull == false; i++)
            {
                string? elInput = elRequest.Form[nombreInput + $"[{i}]"];
                if (elInput != null)
                {
                    laLista.Add(stringToInt(elInput));
                }
            }
            return laLista;
        }
        private List<MediosOpticosFixModel> crearMOs(List<int> losMOIdArch, List<int> losMOGrupo, List<string> losMONombre, List<string> losPath, List<int> losIdArch, int idParte)
        {
            var losMO = new List<MediosOpticosFixModel>();
            var idMaxIdMO = _context.MediosOpticos.Max(c => c.IdMedioOptico);
            idMaxIdMO++;
            for (int i = 0; i < losMOIdArch.Count; i++)
            {
                int elIdSeleccionado = losMOIdArch[i];
                if (losMOGrupo[i] == 0) // Si el grupo es 0 crea un nuevoMO
                {
                    var nuevoMO = new MediosOpticosFixModel();
                    nuevoMO.setIdMO(idMaxIdMO);
                    idMaxIdMO++;
                    nuevoMO.setIdParte(idParte);
                    nuevoMO.setNombreMO(losMONombre[i]);
                    nuevoMO.setRamificacion(0);
                    nuevoMO.setEsRamaActiva(1);
                    nuevoMO.initArchivos();
                    //Trae el Archivo de BD
                    var elArchivo = _context.Archivos.AsNoTracking().FirstOrDefault(c => c.IdArchivo == losIdArch[i]);
                    var elArchivoFix = new ArchivosFixModel(elArchivo);
                    elArchivoFix.setUbicacion(losPath[i]);
                    nuevoMO.addArchivo(elArchivoFix);
                    losMO.Add(nuevoMO);
                }
                if (losMOGrupo[i] != 0)
                {
                    int indiceDeMO = 0;
                    for (int j = 0; j < losIdArch.Count; j++)
                    {
                        if (losMOGrupo[j] == 0) { indiceDeMO++; }
                        if (losIdArch[j] == losMOGrupo[i])
                        {
                            //Trae el Archivo de BD
                            var elArchivo = _context.Archivos.AsNoTracking().FirstOrDefault(c => c.IdArchivo == losIdArch[i]);
                            var elArchivoFix = new ArchivosFixModel(elArchivo);
                            elArchivoFix.setUbicacion(losPath[i]);
                            losMO[indiceDeMO].addArchivo(elArchivoFix);
                        }
                    }
                }
            }
            return losMO;
        }
        private bool verificarFechasMO(List<MediosOpticosFixModel> losMO)
        {
            bool estanBien = true;
            foreach (MediosOpticosFixModel unMO in losMO)
            {
                var unaFecha = _context.archivosFechas.AsNoTracking().FirstOrDefault(c => c.idArchivo == unMO.getByIndiceId(0));
                var fechaDate = unaFecha.fecha;
                for (int i = 1; i < unMO.getCantidadArchivos(); i++)
                {
                    var otraFecha = _context.archivosFechas.AsNoTracking().FirstOrDefault(c => c.idArchivo == unMO.getByIndiceId(0));
                    var fechaDateOtra = otraFecha.fecha;
                    if (fechaDate != fechaDateOtra)
                    {
                        estanBien = false;
                        break;
                    }
                }
                unMO.setPeriodoMO(fechaDate);
            }
            return estanBien;
        }
        private void crearCheckSum(string checkSumPath, List<ArchivosFixModel> losArchivos)
        {
            System.IO.File.WriteAllText(checkSumPath, "");
            foreach (ArchivosFixModel unArchivo in losArchivos)
            {
                System.IO.File.AppendAllText(checkSumPath, Path.GetFileName(unArchivo.ubicacionInicialV) + "||" + unArchivo.HashAV + "\n");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PartesMO2()
        {
            //Preparando PreParte
            var elPreParte = getPreParteFromId(Request.Form["elIdPreParte"]);
            if (elPreParte == null) { return View("MyError", $"No se encuentra el PreParte de ID: {Request.Form["elIdPreParte"]}."); }
            //Variables
            
            //Preparando Parte
            var elParte = new PartesFixModel();
            try
            {
                prepararParte(elParte, elPreParte.numeroPV, elPreParte.idEmpresaV);
            }
            catch(Exception ex)
            {
                return View("MyError", "Problema en la preparación del Parte." + ex.InnerException);
            }
            //Preparando Input
            var losIdArch = getIntFromRequest(Request, "idArchivo");
            var losPath = getStringFromRequest(Request, "ubicacion");
            var losMOIdArch = getIntFromRequest(Request, "MOID"); ;
            var losMOGrupo = getIntFromRequest(Request, "MOGrupo"); ;
            var losMONombre = getStringFromRequest(Request, "MONombre");
            bool sonDelMismoTamaño = losIdArch.Count == losPath.Count && losPath.Count == losMOIdArch.Count && losMOIdArch.Count == losMOGrupo.Count && losMOGrupo.Count == losMONombre.Count;
            if (!sonDelMismoTamaño) { return View("MyError", $"Error en tamaño variables input."); }
            //Crear MOs
            var losMO = new List<MediosOpticosFixModel>();
            try
            {
                losMO = crearMOs(losMOIdArch, losMOGrupo, losMONombre, losPath, losIdArch, elParte.idParteV);
            }
            catch(Exception ex)
            {
                return View("MyError", "Problema en la Creacion Medios Opticos." + ex.InnerException);
            }

            //Chequear Fecha MO
            bool tienenFechasBienMOs = true;
            try
            {
                tienenFechasBienMOs = verificarFechasMO(losMO);
            }
            catch(Exception ex)
            {
                return View("MyError", "Problema en la verificacion Fechas Medios Opticos." + ex.InnerException);
            }
            if(!tienenFechasBienMOs)
            {
                return View("MyError", $"Las Fechas de los Archivos del MO no coinciden.");
            }
            //Obtener Folios MO
            foreach(MediosOpticosFixModel unMO in losMO)
            {
                unMO.makeFoliosMO();
            }

            //Escribir Checksum
            string pathFolder = Path.Join(pathToPartes, elParte.numeroPV.ToString());
            var checkSumPath = Path.Combine(pathFolder, "checksum.txt");
            var isoControl = new ISOControl.ISOControl();
            var dirsIsos = new List<string>();

            foreach (MediosOpticosFixModel unMO in losMO)
            {
                bool esVisspool = false;
                if (Path.GetExtension(unMO.getByIndiceUbicacionI(0)).ToLower() == ".mdb") { esVisspool = true; }
                crearCheckSum(checkSumPath, unMO.getArchivos());
                //Crear ISO
                string nombreISO = unMO.makeNombreISO();
                string pathISO = Path.Combine(pathToPartes, nombreISO);
                dirsIsos.Add(pathISO);
                var direArchivos = new List<string>();
                var direArchivosISO = new List<string>();
                var direFoldersISO = new List<string>();

                direArchivos.Add(checkSumPath);
                direArchivosISO.Add(Path.GetFileName(checkSumPath));

                if (esVisspool)
                {
                    direArchivos.AddRange(unMO.getDireArchMDB());
                    direArchivos.AddRange(getBaseArch());
                    direArchivosISO.AddRange(unMO.getDireArchISO());
                    direArchivosISO.AddRange(getBaseArchISO());
                    direFoldersISO.AddRange(unMO.getDireFolderParaISO());
                    direFoldersISO.AddRange(getFolderISO());
                }

                foreach (ArchivosFixModel unArchivo in unMO.getArchivos())
                {
                    isoControl.crearIso(direFoldersISO, direArchivos, direArchivosISO, pathISO, nombreISO, unMO.PeriodoMOV);
                }
            }
            string pathAlTxtAccess = Path.Combine(pathFolder, elParte.numeroPV + ".txt");
            hacerTxtActasNuevoAuto(elParte, losMO, pathAlTxtAccess);

            string pathZip = Path.Combine(pathFolder, elParte.numeroPV.ToString() + elParte.nombreCortoEV);
            ZipFile.CreateFromDirectory(dirsIsos[0], pathZip);
            using (var archive = ZipFile.Open(pathZip, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(pathAlTxtAccess, Path.GetFileName(pathAlTxtAccess));
            }
            for (int i = 1; i < dirsIsos.Count; i++)
            {
                using (var archive = ZipFile.Open(pathZip, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(dirsIsos[i], Path.GetFileName(dirsIsos[i]));
                }
            }
            System.IO.File.Delete(checkSumPath);
            System.IO.File.Delete(pathAlTxtAccess);
            foreach(string unaDir in dirsIsos)
            {
                System.IO.File.Delete(unaDir);
            }

            //Insert y Updates a la BD
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var elParteRaw = new PartesModel(elParte);
                    _context.Partes.Add(elParteRaw);
                    _context.SaveChanges();
                    
                    var losMOParaBD = new List<MediosOpticosModel>();
                    foreach (MediosOpticosFixModel unMOFix in losMO)
                    {
                        losMOParaBD.Add(new MediosOpticosModel(unMOFix));
                        foreach(ArchivosFixModel unA in unMOFix.getArchivos())
                        {
                            var laInfoActa = new InformacionActaModel();
                            laInfoActa.foliosMO = unMOFix.FoliosMOV;
                            laInfoActa.IdInformacionActa = (_context.informacionActa.Max(c => c.IdInformacionActa)) + 1;
                            laInfoActa.pathArchivo = unA.getPathEnISO();
                            laInfoActa.IdArchivo = unA.IdArchivoV;
                            _context.informacionActa.Add(laInfoActa);
                        }


                    }
                    _context.MediosOpticos.AddRange(losMOParaBD);
                    _context.SaveChanges();
                    foreach(MediosOpticosFixModel unMOFix in losMO)
                    {
                        var losAFix = unMOFix.getArchivos();
                        foreach (ArchivosFixModel unAFix in losAFix)
                        {
                            int rama = 0;
                            var result = (from archivo in _context.Archivos
                                               join medio in _context.MediosOpticos
                                               on archivo.IdMedioOptico equals medio.IdMedioOptico
                                               where archivo.IdLibro == unAFix.IdLibroV
                                                     && archivo.Fraccion == unAFix.FraccionV
                                                     && medio.PeriodoMO == unMOFix.getFechaParaBD()
                                                     && archivo.EsRamaActiva == 1
                                               select new
                                               {
                                                   archivo.Ramificacion,
                                                   archivo.IdArchivo
                                               }).ToList();
                            if (result.Count > 0)
                            {
                                rama = result[0].Ramificacion + 1;
                                var archivoADesactivar = _context.Archivos.FirstOrDefault(c => c.IdArchivo == result[0].IdArchivo);
                                archivoADesactivar.EsRamaActiva = 0;
                                _context.SaveChanges();
                            }
                            var archivoToUpdate = _context.Archivos.FirstOrDefault(a => a.IdArchivo == unAFix.IdArchivoV);
                            archivoToUpdate.IdMedioOptico = unMOFix.IdMedioOpticoV;
                            archivoToUpdate.Ramificacion = 0;
                            archivoToUpdate.EsRamaActiva = 1;
                            _context.Entry(archivoToUpdate).State = EntityState.Modified;
                        }
                    }
                    _context.SaveChanges(); 
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Roll back the transaction if there's an error
                    transaction.Rollback();
                    return View("MyError", ex.Message + ex.InnerException);
                }
            }
            
            return View("PartesList");

        }
        private void hacerTxtActasNuevoAuto(PartesFixModel elParte, List<MediosOpticosFixModel> losMO, string pathArchivo)
        {
            var elPreParte = _context.PreParte.AsNoTracking().FirstOrDefault(c => c.numeroP == elParte.numeroPV);
            string fechaRecibido = "";
            if(elPreParte != null) { fechaRecibido = elPreParte.fechaRecibido; }
            string texto = "INSERT INTO Partes(IdParte, IdTipoParte, idEmpresa, fechaP, comentario, destinatarios, destEmail, destDireccion, destCC, destEmailCC, fechaRecibido) VALUES( {0}, {1}, {2},'{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}' );\r\n";
            texto = String.Format(texto, elParte.numeroPV, elParte.idTipoParteV, elParte.idEmpresaV, elParte.FechaPV.ToString("dd/MM/yyyy"), elParte.ComentarioV, elParte.DestinatariosV, elParte.DestinatariosEmailV, elParte.DestinatariosDireccionV, "", "", fechaRecibido);
            for (int indice = 0; indice < losMO.Count; indice++)
            {
                string textoMO = "INSERT INTO MediosOpticos(IdMedioOptico, IdParte, periodoMO, nombreMO, foliosMO, vecesImprimido) VALUES({0}, {1},'{2}', '{3}', '{4}', 0);\r\n";
                texto += String.Format(textoMO, losMO[indice].IdMedioOpticoV, elParte.numeroPV, losMO[indice].getFechaParaBD, losMO[indice].NombreMOV, losMO[indice].FoliosMOV);
                for (int iArch = 0; iArch < losMO[indice].getCantidadArchivos(); iArch++)
                {
                    string textoArch = "INSERT INTO Archivos(IdArchivo, idMedioOptico, idLibro, fraccion, folioI, folioF, HashA, NombreA)";
                    textoArch += "VALUES( {0}, {1}, {2}, {3}, {4}, {5}, '{6}', '{7}');\r\n";
                    texto += String.Format(textoArch, losMO[indice].getByIndiceId(iArch), losMO[indice].IdMedioOpticoV, losMO[indice].getByIndiceIdLibro(iArch), losMO[indice].getByIndiceFraccion(iArch), losMO[indice].getByIndiceFolioI(iArch), losMO[indice].getByIndiceFolioF(iArch), losMO[indice].getByIndiceHashA(iArch), losMO[indice].getByIndiceNombreEnISO(iArch));
                }
            }
            System.IO.File.WriteAllText(pathArchivo, texto, Encoding.GetEncoding(1252));
        }
        private List<string> getBaseArch()
        {
            var losArchivos = new List<string>();
            string[] archivosEnBaseMDB = Directory.GetFiles(pathToBaseMDB);
            string[] carpetasEnBaseMDB = Directory.GetDirectories(pathToBaseMDB);
            string[] archivosEnInstalador = { };
            string[] archivosEnSoporte = { };
            foreach (string carpeta in carpetasEnBaseMDB)
            {
                if (Regex.IsMatch(carpeta, @"^.*Instalador$"))
                {
                    archivosEnInstalador = Directory.GetFiles(carpeta);
                }
                else if (Regex.IsMatch(carpeta, @"^.*Soporte$"))
                {
                    archivosEnSoporte = Directory.GetFiles(carpeta);
                }
            }
            losArchivos.AddRange(archivosEnInstalador);
            losArchivos.AddRange(archivosEnBaseMDB);
            losArchivos.AddRange(archivosEnSoporte);
            return losArchivos;
        }
        private List<string> getBaseArchISO()
        {
            var losArchivos = new List<string>();
            string[] archivosEnBaseMDB = Directory.GetFiles(pathToBaseMDB);
            string[] carpetasEnBaseMDB = Directory.GetDirectories(pathToBaseMDB);
            string[] archivosEnInstalador = { };
            string[] archivosEnSoporte = { };
            foreach (string carpeta in carpetasEnBaseMDB)
            {
                if (Regex.IsMatch(carpeta, @"^.*Instalador$"))
                {
                    archivosEnInstalador = Directory.GetFiles(carpeta);
                }
                else if (Regex.IsMatch(carpeta, @"^.*Soporte$"))
                {
                    archivosEnSoporte = Directory.GetFiles(carpeta);
                }
            }
            foreach(string archivo in archivosEnInstalador)
            {
                losArchivos.Add(@"Instalador\" + Path.GetFileName(archivo));
            }
            foreach (string archivo in archivosEnBaseMDB)
            {
                losArchivos.Add(Path.GetFileName(archivo));
            }
            foreach (string archivo in archivosEnSoporte)
            {
                losArchivos.Add(@"Soporte\" + Path.GetFileName(archivo));
            }
            return losArchivos;
        }
        private List<string> getFolderISO()
        {
            var lasFolder = new List<string>();
            string[] archivosEnBaseMDB = Directory.GetFiles(pathToBaseMDB);
            string[] carpetasEnBaseMDB = Directory.GetDirectories(pathToBaseMDB);
            string[] archivosEnInstalador = { };
            string[] archivosEnSoporte = { };
            foreach (string carpeta in carpetasEnBaseMDB)
            {
                if (Regex.IsMatch(carpeta, @"^.*Instalador$"))
                {
                    archivosEnInstalador = Directory.GetFiles(carpeta);
                }
                else if (Regex.IsMatch(carpeta, @"^.*Soporte$"))
                {
                    archivosEnSoporte = Directory.GetFiles(carpeta);
                }
            }
            lasFolder = carpetasEnBaseMDB.ToList();
            return lasFolder;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PartesMO(int cosa)
        {
            //Preparando Input
            PreParteFixModel elPreParteNew = new PreParteFixModel();
            elPreParteNew.setIdPreParte(stringToInt(Request.Form["idPreParteV"]));
            elPreParteNew.setNumeroDeParte(stringToInt(Request.Form["numeroPV"]));
            DateTime laFecha = stringToDateTimeV(Request.Form["fechaRecibidoV"]);
            elPreParteNew.setFechaRecibido(laFecha);
            elPreParteNew.setIdEmpresa(stringToInt(Request.Form["idEmpresaV"]));
            elPreParteNew.setNombreEmpresa(Request.Form["nCortoEV"]);

            var losArchivos = _context.Archivos.Where(c => c.IdMedioOptico == 0 && c.HashA != "0").ToList();
            var losArchivosFix = new List<ArchivosFixModel>();
            foreach(ArchivosModel unArch in losArchivos)
            {
                var unArchFix = new ArchivosFixModel(unArch);
                var elLibro = _context.Libros.FirstOrDefault(c => c.IdLibro == unArchFix.IdLibroV);
                if (elLibro.IdEmpresa == elPreParteNew.idEmpresaV)
                {
                    unArchFix.setLibro(elLibro);
                    var ubicacionPreParte = Path.Combine(pathToPartes, elPreParteNew.numeroPV.ToString());
                    var losPathArchivos = new List<string>();
                    var ubicacionTxt = Path.Combine(ubicacionPreParte, "txt");
                    var ubicacionVisspool = Path.Combine(ubicacionPreParte, "visspool");
                    var ubicacionPdf = Path.Combine(ubicacionPreParte, "pdf");
                    if (Directory.Exists(ubicacionTxt))
                    {
                        losPathArchivos.AddRange(Directory.GetFiles(ubicacionTxt).ToList());
                    }
                    if (Directory.Exists(ubicacionPdf))
                    {
                        losPathArchivos.AddRange(Directory.GetFiles(ubicacionPdf).ToList());
                    }
                    if (Directory.Exists(ubicacionVisspool))
                    {
                        var lasCarpetasEnV = Directory.GetDirectories(ubicacionVisspool);
                        foreach (string unaCarpeta in lasCarpetasEnV)
                        {
                            losPathArchivos.AddRange(Directory.GetFiles(unaCarpeta));
                        }
                    }
                    foreach(string unPathArchivo in losPathArchivos)
                    {
                        if(CalculateMD5(unPathArchivo) == unArch.HashA)
                        {
                            unArchFix.setUbicacion(unPathArchivo);
                            unArchFix.calcularPeso();
                            losArchivosFix.Add(unArchFix);
                        }
                    }
                }
            }

            ViewBag.elPreParte = elPreParteNew;
            return View("PartesMO2", losArchivosFix);
        }
        private string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }
        public ActionResult PartesMO()
        {
            //Obtengo prePartes
            var losPrePartesSinParte = _context.PreParte
                                   .Where(pre => !_context.Partes
                                                          .Any(parte => parte.numeroP == pre.numeroP))
                                   .ToList();
            var losPreParteFix = new List<PreParteFixModel>();
            foreach(PreParteModel unPreParte in losPrePartesSinParte)
            {
                var preParteFix = new PreParteFixModel(unPreParte);
                preParteFix.setNombreEmpresa(getNombreCEmpresa(preParteFix.idEmpresaV));
                losPreParteFix.Add(preParteFix);
            }
            return View(losPreParteFix);
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
        private string getNombreCEmpresa(int idE)
        {
            var empresa = _context.Empresas
                                  .Where(c => c.IdEmpresa == idE)
                                  .Select(c => c.NombreCortoE)
                                  .FirstOrDefault();
            string nCortoE = empresa ?? "NULL";
            return nCortoE;
        }
    }
}
