using System.ComponentModel.DataAnnotations;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using System.Globalization;
using Erosionlunar.MITSistema.Models;

namespace Erosionlunar.MITSistema.Entities
{
    public class PreParteModel
    {
        [Key]
        public int? idPreParte { get; set; }
        public int? numeroP { get; set; }
        public string? fechaRecibido { get; set; }
        public int? idEmpresa { get; set; }
        public PreParteModel() { }
        public PreParteModel(PreParteFixModel model)
        {
            idPreParte = model.idPreParteV;
            numeroP = model.numeroPV;
            DateTime fechaRaw = model.fechaRecibidoV;
            fechaRecibido = fechaRaw.ToString("dd/MM/yyyy");
            idEmpresa = model.idEmpresaV;
        }
    }
}
