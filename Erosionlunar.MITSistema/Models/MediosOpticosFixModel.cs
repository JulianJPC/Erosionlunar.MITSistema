using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography.Pkcs;
using Erosionlunar.MITSistema.Entities;
using Mysqlx.Cursor;

namespace Erosionlunar.MITSistema.Models
{
    public class MediosOpticosFixModel
    {
        private int IdMedioOptico;
        private int IdParte;
        private DateTime PeriodoMO;
        private string PeriodoMOStirng;
        private string NombreMO;
        private string FoliosMO;
        private string Volumen;
        private int Ramificacion;
        private int EsRamaActiva;
        private List<ArchivosFixModel> losArchivos;
        private int posicion;
        private int numeroPreParte;



        public int IdMedioOpticoV => IdMedioOptico;
        public int IdParteV => IdParte;
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PeriodoMOV => PeriodoMO;
        public string PeriodoMOStirngV => PeriodoMOStirng;
        public string NombreMOV => NombreMO;
        public string FoliosMOV => FoliosMO;
        public int RamificacionV => Ramificacion;
        public int EsRamaActivaV => EsRamaActiva;
        public int posicionV => posicion;
        public int numeroPreParteV => numeroPreParte;


        public MediosOpticosFixModel() { }
        public MediosOpticosFixModel(MediosOpticosModel unMO) 
        {
            IdMedioOptico = unMO.IdMedioOptico;
            IdParte = unMO.IdParte;
            setPeriodoMO(unMO.PeriodoMO);
            NombreMO = unMO.NombreMO;
            Ramificacion = unMO.Ramificacion;
            EsRamaActiva = unMO.EsRamaActiva;
            losArchivos = new List<ArchivosFixModel>();
        }
        public MediosOpticosFixModel(int numeroP)
        {
            numeroPreParte = numeroP;
            EsRamaActiva = 1;
            losArchivos = new List<ArchivosFixModel>();
        }
        public string makeNombreISO()
        {
            return NombreMO + getFechaParaBD() + ".iso";
        }
        public void makeFoliosMO()
        {
            string folioI = "";
            string folioF = "";
            string foliosFinal = "";
            int unIdLibro = getByIndiceIdLibro(0);
            foreach (ArchivosFixModel unA in getArchivos())
            {
                if (unA.FraccionV == 0 || unA.FraccionV == 1)
                {
                    folioI = unA.FolioIV.ToString();
                }
                folioF = unA.FolioFV.ToString();
                if (unIdLibro != unA.IdLibroV)
                {
                    foliosFinal = "Folios En Actas";
                }
            }
            if (foliosFinal != "") { foliosFinal = folioI + " A " + folioF; }
            setFoliosMO(foliosFinal);
        }

        public List<string> getDireArchMDB()
        {
            List<string> listaArchivos = new List<string>();
            foreach(ArchivosFixModel unA in losArchivos)
            {
                string carpetaArchivo = Path.GetDirectoryName(unA.ubicacionInicialV);
                string nombreArchivo = Path.GetFileNameWithoutExtension(unA.ubicacionInicialV);
                string carpetaPrincipal = Path.Combine(carpetaArchivo, nombreArchivo);
                List<string> archivosSecundarios = Directory.GetFiles(carpetaPrincipal).ToList();
                listaArchivos.Add(unA.ubicacionInicialV);
                listaArchivos.AddRange(archivosSecundarios);
            }
            return listaArchivos;
        }
        public List<string> getDireArchISO()
        {
            List<string> listaArchivos = new List<string>();
            foreach (ArchivosFixModel unA in losArchivos)
            {
                string carpetaArchivo = Path.GetDirectoryName(unA.ubicacionInicialV);
                string nombreArchivo = Path.GetFileNameWithoutExtension(unA.ubicacionInicialV);
                string carpetaPrincipal = Path.Combine(carpetaArchivo, nombreArchivo);
                List<string> archivosSecundarios = Directory.GetFiles(carpetaPrincipal).ToList();
                var archivosSecundariosISO = new List<string>();
                foreach(string unPathA in archivosSecundarios)
                {
                    archivosSecundariosISO.Add(@"Libros\" + nombreArchivo + @"\" + Path.GetFileName(unPathA));
                }
                listaArchivos.Add(@"Libros\" + Path.GetFileName(unA.ubicacionInicialV));
                listaArchivos.AddRange(archivosSecundariosISO);
            }
            return listaArchivos;
        }
        public List<string> getDireFolderParaISO()
        {
            List<string> listaCarpetas = new List<string>();
            foreach (ArchivosFixModel unA in losArchivos)
            {
                string carpetaArchivo = Path.GetDirectoryName(unA.ubicacionInicialV);
                string nombreArchivo = Path.GetFileNameWithoutExtension(unA.ubicacionInicialV);
                listaCarpetas.Add(@"Libros\" + nombreArchivo);
            }
            return listaCarpetas;
        }
        //      ---- SETs AND GETs ----
        //SET
        private void setPeriodoMO(string elPeriodo)
        {
            if(elPeriodo != null)
            {
                string mes = elPeriodo.Substring(0, 2);
                string year = elPeriodo.Substring(2, 2);
                PeriodoMO = DateTime.ParseExact("01"+mes+"20"+year, "ddMMyyyy", CultureInfo.InvariantCulture);
                PeriodoMOStirng = mes + "/" + "20" + year;
            }
            else
            {
                PeriodoMO = DateTime.ParseExact("01/01/0001", "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            
        }
        public void setPeriodoMO(DateTime laFecha)
        {
            PeriodoMO = laFecha;
            string mes = PeriodoMO.Month.ToString();
            if(mes.Length == 1)
            {
                mes = "0" + mes;
            }
            PeriodoMOStirng = mes + "/" + PeriodoMO.Year.ToString();
        }
        public void setArchivos(List<ArchivosFixModel> losA)
        {
            losArchivos = losA;
        }
        public void initArchivos()
        {
            losArchivos = new List<ArchivosFixModel>();
        }
        //SET Archivos
        public void setLibroA(int indiceA, LibrosModel elLibroNuevo)
        {
            losArchivos[indiceA].setLibro(elLibroNuevo);
        }

        //GET
        public void addArchivo(ArchivosFixModel unA)
        {
            losArchivos.Add(unA);
        }
        public string getFechaParaBD()
        {
            string mes = PeriodoMO.Month.ToString();
            if (mes.Length == 1)
            {
                mes = "0" + mes;
            }
            string year = PeriodoMO.Year.ToString();
            string lastTwoDigits = year.Substring(year.Length - 2);
            return mes + lastTwoDigits;
        }
        public int getCantidadArchivos()
        {
            return losArchivos.Count();
        }
        //GET Archivos
        public string getByIndiceUbicacionI(int indice)
        {
            return losArchivos[indice].ubicacionInicialV;
        }
        public string getByIndiceNombreEnISO(int indice)
        {
            return losArchivos[indice].getPathEnISO();
        }
        public string getByIndiceTerminacionInicial(int indice)
        {
            return losArchivos[indice].terminacionInicialV;
        }
        public int getByIndicePosicion(int indice)
        {
            return losArchivos[indice].posicionV;
        }
        public List<string> getByIndiceNombresCPosibles(int indice)
        {
            return losArchivos[indice].nombresCortosPosiblesV;
        }
        public int getByIndiceQNombresCPosibles(int indice)
        {
            return losArchivos[indice].nombresCortosPosiblesV.Count();
        }
        public string getByIndiceNombre(int indice)
        {
            return losArchivos[indice].nombreLibroV;
        }
        public int getByIndiceFraccion(int indice)
        {
            return losArchivos[indice].FraccionV;
        }
        public int getByIndiceFolioI(int indice)
        {
            return losArchivos[indice].FolioIV;
        }
        public int getByIndiceFolioF(int indice)
        {
            return losArchivos[indice].FolioFV;
        }
        public string getByIndiceHashA(int indice)
        {
            return losArchivos[indice].HashAV;
        }
        public int getByIndiceAsientoI(int indice)
        {
            return losArchivos[indice].AsientoIV;
        }
        public int getByIndiceAsientoF(int indice)
        {
            return losArchivos[indice].AsientoFV;
        }
        public int getByIndiceIdLibro(int indice)
        {
            return losArchivos[indice].IdLibroV;
        }

        public List<ArchivosFixModel> getArchivos()
        {
            return losArchivos;
        }

        public void setIdMO(int elIdMO)
        {
            IdMedioOptico = elIdMO;
        }

        public void setIdParte(int elIdParte)
        {
            IdParte = elIdParte;
        }

        public void setRamificacion(int laRami)
        {
            Ramificacion = laRami;
        }

        internal void setEsRamaActiva(int esActiva)
        {
            EsRamaActiva = esActiva;
        }

        public void setNombreMO(string elNombreMO)
        {
            NombreMO = elNombreMO;
        }
        public int getByIndiceId(int indiceA)
        {
            return losArchivos[indiceA].IdArchivoV;
        }

        public void setFoliosMO(string foliosFinal)
        {
            FoliosMO = foliosFinal;
        }
    }
}
