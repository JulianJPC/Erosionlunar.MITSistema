using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class LibrosModel
    {
        [Key]
        public int IdLibro { get; set; }
        public int IdEmpresa { get; set; }
        public string NombreL { get; set; }
        public string NombreArchivoL { get; set; }
        public string? Codificacion { get; set; }
        public int activo { get; set; }
        public LibrosModel() { }
    }
}
