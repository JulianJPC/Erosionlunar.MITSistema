using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class InformacionLibroModel
    {
        [Key]
        public int IdInformacionLibro { get; set; }
        public int IdLibro { get; set; }
        public int IdTipoInformacion { get; set; }
        public string? Informacion { get; set; }
        public int Activo { get; set; }
        public InformacionLibroModel() { }
    }
}
