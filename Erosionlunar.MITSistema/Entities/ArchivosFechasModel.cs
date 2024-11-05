using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class ArchivosFechasModel
    {
        [Key]
        public int IdArchivosFechas { get; set; }
        public DateTime fecha { get; set; }
        public int idArchivo { get; set; }
        public int idLibro { get; set; }
    }
}