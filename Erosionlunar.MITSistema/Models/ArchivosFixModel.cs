using Erosionlunar.MITSistema.Entities;
using System.Text.RegularExpressions;

namespace Erosionlunar.MITSistema.Models
{
    public class ArchivosFixModel
    {

        private int IdArchivo;
        private int IdMedioOptico;
        private int IdLibro;
        private int Fraccion;
        private int FolioI;
        private int FolioF;
        private int AsientoI;
        private int AsientoF;
        private string HashA;
        private int Ramificacion;
        private int EsRamaActiva;
        private string nombreLibro;
        private string nombreCorto;
        private int posicion;
        private List<string> nombresCortosPosibles;
        private string terminacionInicial;
        private string ubicacionInicial;
        private LibrosModel elLibro;
        private double pesoEnDisco;

        public int IdArchivoV => IdArchivo;
        public int IdMedioOpticoV => IdMedioOptico;
        public int IdLibroV => IdLibro;
        public int FraccionV => Fraccion;
        public int FolioIV => FolioI;
        public int FolioFV => FolioF;
        public int AsientoIV => AsientoI;
        public int AsientoFV => AsientoF;
        public string HashAV => HashA;
        public int RamificacionV => Ramificacion;
        public int EsRamaActivaV => EsRamaActiva;
        public string nombreLibroV => nombreLibro;
        public string nombreCortoV => nombreCorto;
        public int posicionV => posicion;
        public List<string> nombresCortosPosiblesV => nombresCortosPosibles;
        public string terminacionInicialV => terminacionInicial;
        public string ubicacionInicialV => ubicacionInicial;
        public LibrosModel elLibroV => elLibro;
        public double pesoV => pesoEnDisco;

        public ArchivosFixModel() { }
        public ArchivosFixModel(string pathAlArchivo) 
        {
            nombresCortosPosibles = new List<string>();
            terminacionInicial = Path.GetExtension(pathAlArchivo);
            ubicacionInicial = pathAlArchivo;
            EsRamaActiva = 1;
        }
        public ArchivosFixModel(ArchivosModel elModel) 
        {
            IdArchivo = elModel.IdArchivo;
            IdMedioOptico = elModel.IdMedioOptico;
            IdLibro = elModel.IdLibro;
            Fraccion = elModel.Fraccion ?? 0;
            FolioF = elModel.FolioF ?? 0;
            FolioI = elModel.FolioI ?? 0;
            AsientoI = elModel.AsientoI ?? 0;
            AsientoF = elModel.AsientoF ?? 0;
            HashA = elModel.HashA;
            Ramificacion = elModel.Ramificacion;
            EsRamaActiva = elModel.EsRamaActiva;
        }
        public void setLibro(LibrosModel elLibroNuevo)
        {
            elLibro = elLibroNuevo;
            nombreLibro = elLibro.NombreL;
            nombreCorto = elLibro.NombreArchivoL;
        }
        public void setUbicacion(string ubi)
        {
            ubicacionInicial = ubi;
        }
        private List<string> getPathVisspool()
        {
            var dirCarpeta = Path.GetDirectoryName(ubicacionInicial);
            var getNameFile = Path.GetFileNameWithoutExtension(ubicacionInicial);
            var dirCosas = Path.Combine(dirCarpeta, getNameFile);
            var archivosMdb = Directory.GetFiles(dirCosas).ToList();
            return archivosMdb;
        }
        public void calcularPeso()
        {
            FileInfo fileInfo = new FileInfo(ubicacionInicial);
            long fileSizeInBytes = fileInfo.Length;
            if (Regex.IsMatch(ubicacionInicial, @"^.*visspool.*$"))
            {
                var archivosMdb = getPathVisspool();
                foreach(string unArch in archivosMdb)
                {
                    FileInfo unFileInfo = new FileInfo(unArch);
                    fileSizeInBytes += unFileInfo.Length;
                }
            }
            double fileSizeInMB = fileSizeInBytes / (1024.0 * 1024.0);
            pesoEnDisco = fileSizeInMB;
        }

        public void setIdMO(int elIdMO)
        {
            IdMedioOptico = elIdMO;
        }

        public string getPathEnISO()
        {
            string laUbicacion = Path.GetFileName(ubicacionInicial);
            if(Path.GetExtension(ubicacionInicial).ToLower() == ".mdb")
            {
                laUbicacion = @"Libros\" + laUbicacion;
            }
            return laUbicacion;
        }
    }
}
