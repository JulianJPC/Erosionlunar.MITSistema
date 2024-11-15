using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class InformacionActaModel
    {
        [Key]
        public int IdInformacionActa {  get; set; }
        public int IdArchivo { get; set; }
        public string? pathArchivo { get; set; }
        public string? volumen {  get; set; }
        public string? foliosMO {  get; set; }
    }
}
