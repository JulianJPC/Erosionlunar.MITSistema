using System.ComponentModel.DataAnnotations;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using System.Globalization;
using Erosionlunar.MITSistema.Models;

namespace Erosionlunar.MITSistema.Entities
{
    public class PartesModel
    {
        [Key]
        public int? idParte { get; set; }
        public int? idTipoParte { get; set; }
        public int? idEmpresa { get; set; }
        public int? numeroP { get; set; }
        public string? FechaP { get; set; }
        public string? Comentario { get; set; }
        public string? DestinatariosDireccion { get; set; }
        public string? DestinatariosEmail { get; set; }
        public string? Destinatarios { get; set; }

        public PartesModel() { }
        public PartesModel(PartesFixModel elParte)
        {
            idParte = elParte.idParteV;
            idTipoParte = elParte.idTipoParteV;
            idEmpresa = elParte.idEmpresaV;
            numeroP = elParte.numeroPV;
            FechaP = elParte.FechaPV.ToString("dd/MM/YYYY");
            Comentario = elParte.ComentarioV;
            DestinatariosDireccion = elParte.DestinatariosDireccionV;
            DestinatariosEmail = elParte.DestinatariosEmailV;
            Destinatarios = elParte.DestinatariosV;
        }
    }
}
