using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class MediosOpticosModel
    {
        [Key]
        public int? IdMedioOptico { get; set; }
        public int? IdParte { get; set; }
        public string? PeriodoMO { get; set; }
        public string? NombreMO { get; set; }
        public int? Ramificacion { get; set; }
        public int? EsRamaActiva { get; set; }
        public MediosOpticosModel() { }
    }
}
