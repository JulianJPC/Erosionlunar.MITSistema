using Erosionlunar.MITSistema.Entities;

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
            IdArchivo = elModel.IdArchivo ?? 0;
            IdMedioOptico = elModel.IdMedioOptico ?? 0;
            IdLibro = elModel.IdLibro ?? 0;
            Fraccion = elModel.Fraccion ?? 0;
            FolioF = elModel.FolioF ?? 0;
            FolioI = elModel.FolioI ?? 0;
            AsientoI = elModel.AsientoI ?? 0;
            AsientoF = elModel.AsientoF ?? 0;
            HashA = elModel.HashA;
            Ramificacion = elModel.Ramificacion ?? 0;
            EsRamaActiva = elModel.EsRamaActiva ?? 0;
        }
        public void setLibro(LibrosModel elLibroNuevo)
        {
            elLibro = elLibroNuevo;
            nombreLibro = elLibro.NombreL;
            nombreCorto = elLibro.NombreArchivoL;
        }
    }
}
