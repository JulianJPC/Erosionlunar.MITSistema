using Erosionlunar.MITSistema.Abstract;
using Erosionlunar.MITSistema.Entities;
using Erosionlunar.MITSistema.ManipuladorControl;
using Erosionlunar.MITSistema.Models;
using Erosionlunar.MITSistema.ProcesadorControl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Erosionlunar.MITSistema.Controllers
{
    public class ProcesosController : Controller
    {
        private readonly MyAppContext _context;
        private string pathToPartes;
        public ProcesosController(MyAppContext context)
        {
            _context = context;
            var elPathRaw = _context.MainFrame.FirstOrDefault(c => c.IdMainFrame == 1);
            pathToPartes = elPathRaw?.Valores ?? string.Empty;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcesarCarpeta(int cosa)
        {
            string direccion = Request.Form["dire"];
            string nombreEC = Request.Form["NombreE"];
            //fecha, nombre Corto, terminacion
            if(direccion != "" && direccion != null)
            {
                var listaIdLibros = new List<int>();
                var listaDeManipuladores = new List<ABSManipulador>();
                var numeroParte = stringToInt(Path.GetFileName(direccion));
                var elParte = _context.Partes.FirstOrDefault(c => c.numeroP == numeroParte);
                
                //Listas Que se mandan
                var listaDeInfoArchivos = new List<List<string>>();
                var posiblesNombresLibros = new List<string>();
                var listaNombreA = new List<string>();
                var listaDeLasFechas = new List<string>();
                var listaDeLasTerminaciones = new List<string>();
                int idEmpresa;
                var archivosEnCarpetaRaw = Directory.GetFiles(Path.Combine(direccion, "txt")).ToList();
                
                if (elParte == null) 
                {
                    var elPreParte = _context.PreParte.FirstOrDefault(c => c.numeroP == numeroParte);
                    if(elPreParte == null) { return View("MyError", $"No se encuentra parte asociado al numero: {numeroParte}"); }
                    idEmpresa = elPreParte.idEmpresa ?? 0;
                }
                else { idEmpresa = elParte.idEmpresa; }
                

                //los Posibles Nombres Cortos libros
                var losLibros = _context.Libros.Where(c => c.IdEmpresa == idEmpresa).ToList();
                foreach(LibrosModel unLibro in losLibros)
                {
                    if(unLibro.NombreArchivoL != null) { posiblesNombresLibros.Add(unLibro.NombreArchivoL); }
                    
                }
                //Rellenar Variables con valores Default
                for (int i = 0; i  < archivosEnCarpetaRaw.Count; i++)
                {
                    listaIdLibros.Add(0);
                    listaNombreA.Add(posiblesNombresLibros[0]);
                    listaDeLasFechas.Add("");
                    string terminacion = Path.GetExtension(archivosEnCarpetaRaw[i]).ToLower();
                    listaDeLasTerminaciones.Add(terminacion);
                    listaDeManipuladores.Add(null);
                }

                //Obtengo ID libros del nombre del archivo
                for (int i = 0; i < archivosEnCarpetaRaw.Count; i++)
                {
                    var losInfoLibrosRegex = getInfoLibros(idEmpresa, 15);
                    foreach (InformacionLibroModel unaInfo in losInfoLibrosRegex)
                    {
                        var primerasDiezLineas = getLineas(archivosEnCarpetaRaw[i], 10);
                        if (unaInfo.Informacion == null) { unaInfo.Informacion = ""; }
                        Regex elRegex = makeRegex(unaInfo.Informacion);
                        foreach (string unaLinea in primerasDiezLineas)
                        {
                            if (elRegex.IsMatch(unaLinea) && unaInfo.IdLibro != null) { listaIdLibros[i] = unaInfo.IdLibro; }
                        }
                    }
                }
                //Obtengo nombre de los Archivos y Manipuladores
                for (int i = 0; i < archivosEnCarpetaRaw.Count; i++)
                {
                    if (listaIdLibros[i] != 0)
                    {
                        var libro = _context.Libros.FirstOrDefault(c => c.IdLibro == listaIdLibros[i]);
                        if (libro == null) { return View("MyError", $"No se encuentra libro asociado al numero: {listaIdLibros[i]}"); }
                        listaNombreA[i] = libro.NombreArchivoL;
                        var elManipulador = getManipulador(listaIdLibros[i]);
                        listaDeManipuladores[i] = elManipulador;
                    }
                }
                //Obtengo Fechas Archivos
                for (int i = 0; i < archivosEnCarpetaRaw.Count; i++)
                {
                    if (listaDeManipuladores[i] != null)
                    {
                        DateTime laFecha = listaDeManipuladores[i].getFecha(archivosEnCarpetaRaw[i]);
                        listaDeLasFechas[i] = laFecha.Month.ToString() + laFecha.Year.ToString().Substring(2, 2);
                        if (listaDeLasFechas[i].Length == 3) { listaDeLasFechas[i] = "0" + listaDeLasFechas[i]; }
                    }
                }
                ViewBag.IdEmpresa = idEmpresa;
                listaDeInfoArchivos.Add(posiblesNombresLibros);
                listaDeInfoArchivos.Add(listaNombreA);
                listaDeInfoArchivos.Add(listaDeLasFechas);
                listaDeInfoArchivos.Add(listaDeLasTerminaciones);
                listaDeInfoArchivos.Add(archivosEnCarpetaRaw);
                return View("ProcesarCarpeta2", listaDeInfoArchivos);
            }
            return View("MyError", "La Direccion a buscar los archivos no existe.");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcesarCarpeta2()
        {
            string? idEmpresa = Request.Form[$"idEmpresa"];
            if(idEmpresa == null) { return View("MyError", $"Id Empresa no valido."); }
            var listaProcesadores = new List<ABSProcesador>();
            int cantidadArchivos = -1;
            int cantidadCorrecta = 0;
            bool esNull = false;

            
            for (int i = 0; i < 1000 && esNull == false; i++)
            {
                string? unNombreC = Request.Form[$"nombreC[{i}]"];
                string? unaFecha = Request.Form[$"fecha[{i}]"];
                string? unaParte = Request.Form[$"parte[{i}]"];
                string? unaTerm = Request.Form[$"term[{i}]"];
                string? unPath = Request.Form[$"unPath[{i}]"];

                if (unNombreC != null & unaFecha != null & unaParte != null & unaTerm != null & unPath != null)
                {
                    var idLibro = getIdLibroFromNombreC(unNombreC, stringToInt(idEmpresa));
                    if(idLibro == 0) { return View("MyError", $"Id Libro no valido de nombre corto  '{unNombreC}' y ID Empresa '{idEmpresa}'."); }
                    var unProcesador = getProcesador(idLibro);
                    var elLibro = _context.Libros.FirstOrDefault(c => c.IdLibro == idLibro);
                    unProcesador.setInicial(unNombreC, unaFecha, stringToInt(unaParte), unaTerm, unPath, idLibro, stringToInt(idEmpresa));
                    unProcesador.setInicioE(getInicioE(stringToInt(idEmpresa)));
                    unProcesador.setEncoding(stringToInt(elLibro.Codificacion));
                    listaProcesadores.Add(unProcesador);
                    cantidadCorrecta += 1;
                }
                else if (unNombreC == null & unaFecha == null & unaParte == null & unaTerm == null & unPath == null)
                {
                    esNull = true;
                }
                cantidadArchivos++;
            }
            //Error Input
            if (listaProcesadores.Count == 0 || cantidadCorrecta != cantidadArchivos)
            {
                return View("MyError", $"Error. Cantidad de Archivos: {listaProcesadores.Count}\nCantidad Correcta: {cantidadCorrecta}");
            }
            //Ordenar Lista
            listaProcesadores = listaProcesadores.OrderBy(obj => obj.getFecha()).ToList();
            //Modificar
            var archivoDB = new List<ArchivosModel>();
            var archivosFechasDB = new List<ArchivosFechasModel>();
            int newIdA = getLastIdArchivo()+1;
            int newIdF = _context.archivosFechas.Max(c => c.idArchivo) + 1;
            foreach (ABSProcesador unProce in listaProcesadores)
            {
                //IdArchivo
                unProce.setIdArchivo(newIdA);
                newIdA++;
                //Folios y asientos y Regex
                bool necesitaTraer = unProce.necesitaFolios();
                if (necesitaTraer)
                {
                    var archivoAnterior = getArchivoBefore(unProce.getFecha(), unProce.getFraccion(), unProce.getIdLibro());
                    bool encontroEnBD = unProce.setFoliosYAsiento(archivoAnterior);
                    if (!encontroEnBD) { return View("MyError", $"No encontro los folios y asientos anteriores del libro({unProce.getIdLibro()})"); }
                }
                unProce.setRegex(getLosRegex(unProce.getIdLibro() ));
                //Modifica
                unProce.modificarArchivo();
                var elArchivoModel = unProce.produceAModel();
                var laFechaArchivo = new ArchivosFechasModel();
                laFechaArchivo.IdArchivosFechas = newIdF;
                newIdF++;
                laFechaArchivo.fecha = unProce.getFecha();
                laFechaArchivo.idLibro = unProce.getIdLibro();
                laFechaArchivo.idArchivo = unProce.getIdArchivo();
                archivoDB.Add( elArchivoModel );
                archivosFechasDB.Add(laFechaArchivo);
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.AddRange(archivoDB);
                    _context.AddRange(archivosFechasDB);
                    _context.SaveChanges();
                    transaction.Commit();
                    
                }
                catch (Exception ex)
                {
                    // Roll back the transaction if there's an error
                    transaction.Rollback();
                    return View("MyError", ex.Message);
                }
            }
            return View("ProcesarCarpeta3", listaProcesadores);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult convertirVisspool(int cosa)
        {
            string direccion = Request.Form["dire"];
            string nombreEC = Request.Form["NombreE"];
            //fecha, nombre Corto, terminacion
            if (direccion != "" && direccion != null)
            {
                var listaIdLibros = new List<int>();
                var numeroParte = stringToInt(Path.GetFileName(direccion));
                var elParte = _context.Partes.FirstOrDefault(c => c.numeroP == numeroParte);
                var lasDir = getDirsVisspool();
                

                var visspoolizador = new VisspoolControl.VisspoolControl(lasDir);
                //Listas Que se mandan
                var listaDeInfoArchivos = new List<List<string>>();
                var listaHashesAnteriores = new List<string>();
                var listaRegexLineas = new List<Regex>();
                var listaNombres = new List<string>();
                var listaArchivos = new List<ArchivosModel>();
                var archivosEnCarpetaRaw = Directory.GetFiles(Path.Combine(direccion, "txt")).ToList();


                for (int i = 0; i < archivosEnCarpetaRaw.Count; i++)
                {
                    string elHash = CalculateMD5(archivosEnCarpetaRaw[i]);
                    listaHashesAnteriores.Add(elHash);
                    var elArchivo = _context.Archivos.FirstOrDefault(c => c.HashA == elHash);
                    listaArchivos.Add(elArchivo);
                    var elNombreLibro = _context.Libros.AsNoTracking().FirstOrDefault(c => c.IdLibro == elArchivo.IdLibro).NombreL;
                    listaNombres.Add(elNombreLibro);
                }
                for (int i = 0; i < listaArchivos.Count; i++)
                {
                    var laInfo = _context.InformacionLibro.AsNoTracking().FirstOrDefault(c => c.IdLibro == listaArchivos[i].IdLibro & c.IdTipoInformacion == 3);
                    listaRegexLineas.Add(new Regex(@"^.*" + laInfo.Informacion + @".*$", RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                for(int i = 0; i<listaArchivos.Count; i++)
                {
                    string nuevoHash = visspoolizador.Visspool(archivosEnCarpetaRaw[i], listaArchivos[i], listaRegexLineas[i], listaNombres[i]);
                    listaArchivos[i].HashA = nuevoHash;
                }
                _context.SaveChanges();
                return View("Index");
            }
            return View("MyError", "La Direccion a buscar los archivos no existe.");
        }
        public ActionResult convertirVisspool()
        {
            var carpetasConInfornacion = new List<List<string>>();
            var carpetasDePartes = Directory.GetDirectories(pathToPartes).ToList();
            var carpetasConTxt = new List<string>();
            //Obtengo carpetas que tengan la carpeta txt
            foreach (string unaCarpeta in carpetasDePartes)
            {
                var carpetasEnCarpeta = Directory.GetDirectories(unaCarpeta);
                if (carpetasEnCarpeta.Contains(Path.Combine(unaCarpeta, "txt")))
                {
                    carpetasConTxt.Add(unaCarpeta);
                }
            }
            var carpetasProcesadas = new List<string>();
            foreach (string unaCarpeta in carpetasConTxt) 
            {
                var archivosEnCarpetaTxt = Directory.GetFiles(Path.Combine(unaCarpeta, "txt")).ToList();
                bool estanEnBD = true;
                
                var losHashes = new List<string>();
                foreach(string unArch in archivosEnCarpetaTxt)
                {
                    losHashes.Add(CalculateMD5(unArch));
                }
                foreach(string unHash in losHashes)
                {
                    var elArch = _context.Archivos.AsNoTracking().FirstOrDefault(c => c.HashA == unHash && c.IdMedioOptico == 0);
                    if (elArch == null) { estanEnBD = false; }
                }
                if (archivosEnCarpetaTxt.Count < 1)
                {
                    estanEnBD = false;
                }
                if (estanEnBD)
                {
                    carpetasProcesadas.Add(unaCarpeta);
                }
            }
            foreach (string unaCarpeta in carpetasProcesadas)
            {
                var infoCarpeta = new List<string>();
                var archivosEnCarpetaRaw = Directory.GetFiles(Path.Combine(unaCarpeta, "txt")).ToList();
                List<string> archivosEnCarpeta = new List<string>();
                foreach (string unA in archivosEnCarpetaRaw)
                {
                    archivosEnCarpeta.Add(Path.GetFileName(unA));
                }

                string? elNumeroPString = Path.GetFileName(unaCarpeta);
                int elNumeroP;
                string nombreCorto = "";
                if (elNumeroPString != null)
                {
                    bool esUnNumero = Int32.TryParse(elNumeroPString, out elNumeroP);
                    if (esUnNumero)
                    {
                        nombreCorto = getNombreCortoEFromNumeroP(elNumeroP);
                    }
                }
                if (nombreCorto != "")
                {
                    infoCarpeta.Add(unaCarpeta);
                    infoCarpeta.Add(nombreCorto);
                    infoCarpeta.AddRange(archivosEnCarpeta);
                    carpetasConInfornacion.Add(infoCarpeta);
                }

            }
            return View(carpetasConInfornacion);
        }
        private List<regexLibrosModel> getLosRegex(int idLibro)
        {
            var losRegex = _context.regexLibros.Where(c => c.idLibro == idLibro).ToList();
            return losRegex;
        }
        private ArchivosModel getArchivoBefore(DateTime laFecha, int fraccion, int idLibro)
        {
            ArchivosModel elArchivo = null;
            var mesABuscar = laFecha;
            int fraccionMaxAnterior = 0;
            if (fraccion == 0 | fraccion == 1)
            {
                mesABuscar = laFecha.AddMonths(-1);
                var archivosDelMesAnterior = _context.archivosFechas.AsNoTracking().Where(c => c.idLibro == idLibro & c.fecha == laFecha).ToList();
                List<ArchivosModel> losArchivos = new List<ArchivosModel>();
                foreach(ArchivosFechasModel unArch in archivosDelMesAnterior)
                {
                    var unArchivoAnterior = _context.Archivos.AsNoTracking().FirstOrDefault(c => c.IdArchivo == unArch.idArchivo);
                    losArchivos.Add(unArchivoAnterior);
                }
                foreach(ArchivosModel unArch in losArchivos)
                {
                    if(unArch.Fraccion > fraccionMaxAnterior)
                    {
                        fraccionMaxAnterior = unArch.Fraccion ?? 0;
                    }
                }
            }
            else
            {
                fraccionMaxAnterior = fraccion - 1;
            }

            var archivosFechas = _context.archivosFechas.AsNoTracking().Where(c => c.idLibro == idLibro & c.fecha == mesABuscar).ToList();
            foreach(ArchivosFechasModel unArch in archivosFechas)
            {
                var unArchDeArch = _context.Archivos.AsNoTracking().FirstOrDefault(c => c.IdArchivo == unArch.idArchivo & fraccion == fraccionMaxAnterior);
                if(unArchDeArch != null)
                {
                    elArchivo = unArchDeArch; break;
                } 
            }
            return elArchivo;
        }
        private int getLastIdArchivo()
        {
            return  _context.Archivos.Max(c => c.IdArchivo);
        }

        private int getInicioE(int idEmpresa)
        {
            int inicioE = 0;
            var laEmpresa = _context.Empresas.FirstOrDefault(c => c.IdEmpresa == idEmpresa);
            if(laEmpresa != null) { inicioE = laEmpresa.IEjercicioE; }
            return inicioE;
        }

        public ActionResult ProcesarCarpeta()
        {
            var carpetasConInfornacion = new List<List<string>>();
            var carpetasDePartes = Directory.GetDirectories(pathToPartes).ToList();
            var carpetasConTxt = new List<string>();
            //Obtengo carpetas que tengan la carpeta txt
            foreach(string unaCarpeta in carpetasDePartes)
            {
                var carpetasEnCarpeta = Directory.GetDirectories(unaCarpeta);
                if (carpetasEnCarpeta.Contains(Path.Combine(unaCarpeta, "txt")))
                {
                    carpetasConTxt.Add(unaCarpeta);
                }
            }
           
            foreach(string unaCarpeta in carpetasConTxt)
            {
                var infoCarpeta = new List<string>();
                var archivosEnCarpetaRaw = Directory.GetFiles(Path.Combine(unaCarpeta, "txt")).ToList();
                List<string> archivosEnCarpeta = new List<string>();
                foreach (string unA in archivosEnCarpetaRaw)
                {
                    archivosEnCarpeta.Add(Path.GetFileName(unA));
                }
                
                string? elNumeroPString = Path.GetFileName(unaCarpeta);
                int elNumeroP;
                string nombreCorto = "";
                if (elNumeroPString != null)
                {
                    bool esUnNumero = Int32.TryParse(elNumeroPString, out elNumeroP);
                    if (esUnNumero)
                    {
                        nombreCorto = getNombreCortoEFromNumeroP(elNumeroP);
                    }
                }
                if(nombreCorto !=  "")
                {
                    infoCarpeta.Add(unaCarpeta);
                    infoCarpeta.Add(nombreCorto);
                    infoCarpeta.AddRange(archivosEnCarpeta);
                    carpetasConInfornacion.Add(infoCarpeta);
                }

            }
            return View(carpetasConInfornacion);
        }

        private string getNombreCortoEFromNumeroP(int numeroP)
        {
            int idEmpresa = 0;
            string nombreCorto = "";
            var parteRaw = _context.Partes.FirstOrDefault(c => c.numeroP == numeroP);
            if (parteRaw == null)
            {
                var preParteRaw = _context.PreParte.FirstOrDefault(c => c.numeroP == numeroP);
                if (preParteRaw != null) { idEmpresa = preParteRaw.idEmpresa ?? 0; }

            }
            else
            {
                idEmpresa = parteRaw.idEmpresa;
            }
            if(idEmpresa != 0)
            {
                var laEmpresa = _context.Empresas.FirstOrDefault(c => c.IdEmpresa == idEmpresa);
                if (laEmpresa != null) 
                {
                    if (laEmpresa.NombreCortoE != null) { nombreCorto = laEmpresa.NombreCortoE; }
                }
            }
            return nombreCorto;
        }
        private string getIdEmpresaFromNumeroP(int numeroP)
        {
            int idEmpresa = 0;
            string nombreCorto = "";
            var parteRaw = _context.Partes.FirstOrDefault(c => c.numeroP == numeroP);
            if (parteRaw != null)
            {
                idEmpresa = parteRaw.idEmpresa;
            }
            if (idEmpresa != 0)
            {
                var laEmpresa = _context.Empresas.FirstOrDefault(c => c.IdEmpresa == idEmpresa);
                if (laEmpresa != null)
                {
                    if (laEmpresa.NombreCortoE != null) { nombreCorto = laEmpresa.NombreCortoE; }
                }
            }
            return nombreCorto;
        }
        private List<InformacionLibroModel> getInfoLibros(int idEmpresa, int idTipo)
        {
            List<InformacionLibroModel> listaInfoLibros = new List<InformacionLibroModel>();
            var losLibros = _context.Libros.Where(c => c.IdEmpresa == idEmpresa & c.activo == 1).ToList();
            foreach(LibrosModel unLibro in losLibros)
            {
                var laInfo = _context.InformacionLibro.FirstOrDefault(c => c.IdLibro == unLibro.IdLibro && c.IdTipoInformacion == idTipo);
                if (laInfo != null) { listaInfoLibros.Add(laInfo); }
            }
            return listaInfoLibros;
        }
        private Regex makeRegex(string pattern)
        {
            var elRegex = new Regex(@"^.*" + pattern + @".*$", RegexOptions.Compiled);
            return elRegex;
        }
        private List<string> getLineas(string direA, int cantidad)
        {
            var lasLineas = new List<string>();
            using (StreamReader reader = new StreamReader(direA))
            {
                for (int i = 0; i < cantidad; i++)
                {
                    // Read each line and add it to the list
                    string line = reader.ReadLine();

                    // Break if we reach the end of the file
                    if (line == null)
                        break;

                    lasLineas.Add(line);
                }
            }
            return lasLineas;
        }
        private int stringToInt(string? valor)
        {
            int elNumero = 0;
            bool esNumero = Int32.TryParse(valor, out elNumero);
            return elNumero;
        }
        private ABSManipulador getManipulador(int idLibro)
        {
            if(idLibro == 1)
            {
                return new Manipulador1_1();
            }
            else if (idLibro == 2)
            {
                return new Manipulador1_2();
            }
            else
            {
                return null;
            }
        }
        private ABSProcesador getProcesador(int idLibro)
        {
            if (idLibro == 1)
            {
                return new Procesador1_1();
            }
            else if (idLibro == 2)
            {
                return new Procesador1_2();
            }
            else
            {
                return null;
            }
        }
        private List<string> getDirsVisspool()
        {
            var lasDir = new List<string>();
            for(int i= 5; i < 13; i++)
            {
                lasDir.Add(_context.MainFrame.AsNoTracking().FirstOrDefault(c => c.IdMainFrame == i).Valores.ToString());
            }
            return lasDir;
        }
        private int getIdLibroFromNombreC(string nombreC, int idEmpresa)
        {
            int idLibro = 0;
            var libro = _context.Libros.FirstOrDefault(c=> c.NombreArchivoL == nombreC & c.IdEmpresa == idEmpresa);
            if(libro != null) { idLibro = libro.IdLibro; }
            return idLibro;
        }
        

    }
}
