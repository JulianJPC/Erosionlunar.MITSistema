using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography.Pkcs;
using Erosionlunar.MITSistema.Entities;

namespace Erosionlunar.MITSistema.Models
{
    public class MediosOpticosFixModel
    {
        private int IdMedioOptico;
        private int IdParte;
        private DateTime PeriodoMO;
        private string PeriodoMOStirng;
        private string NombreMO;
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
        public int RamificacionV => Ramificacion;
        public int EsRamaActivaV => EsRamaActiva;
        public int posicionV => posicion;
        public int numeroPreParteV => numeroPreParte;


        public MediosOpticosFixModel() { }
        public MediosOpticosFixModel(MediosOpticosModel unMO) 
        {
            IdMedioOptico = unMO.IdMedioOptico ?? 0;
            IdParte = unMO.IdParte ?? 0;
            setPeriodoMO(unMO.PeriodoMO);
            NombreMO = unMO.NombreMO;
            Ramificacion = unMO.Ramificacion ?? 0;
            EsRamaActiva = unMO.EsRamaActiva ?? 0;
            losArchivos = new List<ArchivosFixModel>();
        }
        public MediosOpticosFixModel(int numeroP)
        {
            numeroPreParte = numeroP;
            EsRamaActiva = 1;
            losArchivos = new List<ArchivosFixModel>();
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
        public void setArchivos(List<ArchivosFixModel> losA)
        {
            losArchivos = losA;
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
        public int getCantidadArchivos()
        {
            return losArchivos.Count();
        }
        //GET Archivos
        public string getByIndiceUbicacionI(int indice)
        {
            return losArchivos[indice].ubicacionInicialV;
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
    }
}
