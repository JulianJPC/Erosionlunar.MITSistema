using Erosionlunar.MITSistema.Models;
using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class ArchivosModel
    {
        [Key]
        public int IdArchivo { get; set; }
        public int IdMedioOptico { get; set; }
        public int IdLibro { get; set; }
        public int? Fraccion { get; set; }
        public int? FolioI { get; set; }
        public int? FolioF { get; set; }
        public int? AsientoI { get; set; }
        public int? AsientoF { get; set; }
        public string? HashA { get; set; }
        public int Ramificacion { get; set; }
        public int EsRamaActiva { get; set; }
        public ArchivosModel() { }
        public ArchivosModel(ArchivosFixModel unAFix)
        {
            IdArchivo = unAFix.IdArchivoV;
            IdMedioOptico = unAFix.IdMedioOpticoV;
            IdLibro = unAFix.IdLibroV;
            Fraccion = unAFix.FraccionV;
            FolioI = unAFix.FolioIV;
            FolioF = unAFix.FolioFV;
            AsientoI = unAFix.AsientoIV;
            AsientoF = unAFix.AsientoFV;
            HashA = unAFix.HashAV;
            Ramificacion = unAFix.RamificacionV;
            EsRamaActiva = unAFix.EsRamaActivaV;
        }
    }
}
