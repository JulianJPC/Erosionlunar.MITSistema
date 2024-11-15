using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class InformacionEmpresaModel
    {
        [Key]
        public int IdInformacionEmpresa { get; set; }
        public int IdEmpresa { get; set; }
        public int IdTipoInformacion { get; set; }
        public string? Informacion { get; set; }
        public int activo { get; set; }
    }
}