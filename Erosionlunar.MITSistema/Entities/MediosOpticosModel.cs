using Erosionlunar.MITSistema.Models;
using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class MediosOpticosModel
    {
        [Key]
        public int IdMedioOptico { get; set; }
        public int IdParte { get; set; }
        public string? PeriodoMO { get; set; }
        public string NombreMO { get; set; }
        public int Ramificacion { get; set; }
        public int EsRamaActiva { get; set; }
        public MediosOpticosModel() { }
        public MediosOpticosModel(MediosOpticosFixModel unModel)
        {
            IdMedioOptico = unModel.IdMedioOpticoV;
            IdParte = unModel.IdParteV;
            PeriodoMO = unModel.getFechaParaBD();
            NombreMO = unModel.NombreMOV;
            Ramificacion = unModel.RamificacionV;
            EsRamaActiva = unModel.EsRamaActivaV;
        }
    }
}
