using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Erosionlunar.MITSistema.Models;

namespace Erosionlunar.MITSistema.Entities
{
    public class MailsDeParteModel
    {
        [Key]
        public int idMailsDeParte { get; set; }
        public string? numeroP { get; set; }
        public string? tipo { get; set; }
        public int idNombreYMail { get; set; }
        public MailsDeParteModel() { }
        public MailsDeParteModel(MailsDeParteFixModel elModel)
        {
            idMailsDeParte = elModel.idMailsDeParteV;
            numeroP = elModel.numeroPV;
            tipo = elModel.tipoV;
            idNombreYMail = elModel.idNombreYMailV;
        }
    }
}

