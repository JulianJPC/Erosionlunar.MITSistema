using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class MainFrameModel
    {
        [Key]
        public int? IdMainFrame { get; set; }
        public string? Valores { get; set; }
        public string? Descripcion { get; set; }
    }
}
