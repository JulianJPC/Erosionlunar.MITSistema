using Erosionlunar.MITSistema.Entities;
using System.ComponentModel.DataAnnotations;
using System.Globalization;


namespace Erosionlunar.MITSistema.Models
{
    public class PreParteFixModel
    {
        private int idPreParte;
        private int numeroP;
        private DateTime fechaRecibido;
        private int idEmpresa;
        private string nCortoE; 
        private List<MailsDeParteFixModel> losMails;

        [Display(Name = "PreParte")]
        public int idPreParteV => idPreParte;
        [Display(Name = "Número Parte")]
        public int numeroPV => numeroP;
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha Recibido")]
        public DateTime fechaRecibidoV => fechaRecibido;
        [Display(Name = "ID Empresa")]
        public int idEmpresaV => idEmpresa;
        [Display(Name = "Empresa")]
        public string nCortoEV => nCortoE;

        public PreParteFixModel(PreParteModel unPreParte)
        {
                idPreParte = unPreParte.idPreParte;
                numeroP = unPreParte.numeroP;
                string arregloF = unPreParte.fechaRecibido ?? "01/01/0001";
                fechaRecibido = DateTime.ParseExact(arregloF, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                idEmpresa = unPreParte.idEmpresa ?? 0;
                losMails = new List<MailsDeParteFixModel>();
                nCortoE = "";
        }
        public PreParteFixModel() { }
        public void setNombreEmpresa(string nombreCortoE)
        {
            nCortoE = nombreCortoE;
        }
        public void setMails(List<MailsDeParteFixModel> nuevosMails)
        {
            losMails = nuevosMails;
        }
        public void setFechaRecibido(DateTime laFecha)
        {
            fechaRecibido = laFecha;
        }
        public void setNumeroDeParte(int elNumeroP)
        {
            numeroP = elNumeroP;
        }
        public void setIdEmpresa(int elIdEmpresa)
        {
            idEmpresa = elIdEmpresa;
        }
        public void setIdPreParte(int elIdPreParte)
        {
            idPreParte = elIdPreParte;
        }
        public bool tieneMails()
        {
            return losMails?.Count > 0;
        }
        public bool tieneElMail(MailsDeParteModel unMail)
        {
            int elIdABuscar = unMail.idMailsDeParte;
            bool estaEnMails = false;
            foreach(MailsDeParteFixModel unMailMio in losMails)
            {
                if(elIdABuscar == unMailMio.idMailsDeParteV)
                {
                    estaEnMails = true;
                    break;
                }
            }
            return estaEnMails;
        }
        public bool tieneElMail(MailsDeParteFixModel unMail)
        {
            int elIdABuscar = unMail.idMailsDeParteV;
            bool estaEnMails = false;
            foreach (MailsDeParteFixModel unMailMio in losMails)
            {
                if (elIdABuscar == unMailMio.idMailsDeParteV)
                {
                    estaEnMails = true;
                    break;
                }
            }
            return estaEnMails;
        }
        public void setNumeroDeParteMails(int numeroPNuevo)
        {
            foreach(MailsDeParteFixModel unMail in losMails)
            {
                unMail.setNumeroP(numeroPNuevo.ToString());
            }
        }
        public List<MailsDeParteModel> devolverMailsDiferentes(PreParteFixModel otroPreParte)
        {
            var laLista = new List<MailsDeParteModel>();
            foreach(MailsDeParteFixModel unMail in losMails)
            {
                if (!otroPreParte.tieneElMail(unMail))
                {
                    laLista.Add(new MailsDeParteModel(unMail));
                }
            }
            return laLista;
        }
        public List<List<string>> nombresYMailsParaV()
        {
            List<List<string>> laLista = new List<List<string>>();
            foreach(MailsDeParteFixModel unMail in losMails)
            {
                var miniLista = new List<string>();
                miniLista.Add(unMail.NombreV);
                miniLista.Add(unMail.EmailV);
                laLista.Add(miniLista);
            }
            return laLista;
        }
        public List<int> idsMailsParaV()
        {
            List<int> laLista = new List<int>();
            foreach (MailsDeParteFixModel unMail in losMails)
            {
                laLista.Add(unMail.idMailsDeParteV);
            }
            return laLista;
        }
        public List<int> idsNyMMailsParaV()
        {
            List<int> laLista = new List<int>();
            foreach (MailsDeParteFixModel unMail in losMails)
            {
                laLista.Add(unMail.idNombreYMailV);
            }
            return laLista;
        }
        public void setMailsDeView(List<string> losIds, List<string> losIdsNyE)
        {
            losMails = new List<MailsDeParteFixModel>();
            if(losIds.Count == losIdsNyE.Count)
            {
                for (int i = 0; i < losIds.Count(); i++)
                {
                    var unMail = new MailsDeParteFixModel();
                    unMail.setNumeroP(numeroPV.ToString());
                    int elNumeroId;
                    int elNumeroIdNyE;
                    bool esNumeroId = Int32.TryParse(losIds[i], out elNumeroId);
                    bool esNumeroIdNyE = Int32.TryParse(losIdsNyE[i], out elNumeroIdNyE);
                    if (esNumeroId)
                    {
                        unMail.setIdMailDeParte(elNumeroId);
                    }
                    if (esNumeroIdNyE)
                    {
                        unMail.setIdNombreYMail(elNumeroIdNyE);
                    }
                    if (esNumeroIdNyE & esNumeroId)
                    {
                        losMails.Add(unMail);
                    }

                }
            }
        }
    }
}
