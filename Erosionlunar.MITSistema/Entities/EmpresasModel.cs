using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities

{
    public class EmpresasModel
    {
        [Key]
        public int? IdEmpresa { get; set; }
        public int? IdGrupoEmpresa { get; set; }
        public string? NombreE { get; set; }
        public string? NombreCortoE { get; set; }
        public int? IEjercicioE { get; set; }
    }
}
