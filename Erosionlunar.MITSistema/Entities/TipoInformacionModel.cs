using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class TipoInformacionModel
    {
        [Key]
        public int? IdTipoInformacion { get; set; }
        public string? TipoInfromacion { get; set; }
    }
}
