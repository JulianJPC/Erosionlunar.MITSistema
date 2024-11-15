using Erosionlunar.MITSistema.Entities;
using Microsoft.EntityFrameworkCore;

namespace Erosionlunar.MITSistema.Models
{
    public class MailsDeParteFixModel
    {
        private int idMailsDeParte;
        private string numeroP;
        private string tipo;
        private int idNombreYMail;
        private string Nombre;
        private string Email;

        public int idMailsDeParteV => idMailsDeParte;
        public string numeroPV => numeroP;
        public string tipoV => tipo;
        public int idNombreYMailV => idNombreYMail;
        public string NombreV => Nombre;
        public string EmailV => Email;

        public MailsDeParteFixModel() { }
        public MailsDeParteFixModel(MailsDeParteModel elMail)
        {
            idMailsDeParte = elMail.idMailsDeParte;
            numeroP = elMail.numeroP ?? "0";
            tipo = elMail.tipo ?? "0";
            idNombreYMail = elMail.idNombreYMail;
            Nombre = "";
            Email = "";
        }
        public void setNombreYMail(InformacionEmpresaModel laInfo)
        {
            string nombreYEmail = laInfo.Informacion ?? "-";
            Nombre = nombreYEmail.Substring(0, nombreYEmail.IndexOf('<')).Trim();
            Email = nombreYEmail.Substring(nombreYEmail.IndexOf('<') + 1, nombreYEmail.IndexOf('>') - nombreYEmail.IndexOf('<') - 1).Trim();
        }
        public void setNumeroP(string numeroNuevoP)
        {
            numeroP = numeroNuevoP;
        }
        public void setIdNombreYMail(int laId)
        {
            idNombreYMail = laId;
        }
        public void setIdMailDeParte(int laId)
        {
            idMailsDeParte = laId;
        }
    }
}
