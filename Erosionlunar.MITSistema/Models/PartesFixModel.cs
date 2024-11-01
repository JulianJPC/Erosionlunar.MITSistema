using Erosionlunar.MITSistema.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Erosionlunar.MITSistema.Models
{
    public class PartesFixModel
    {
        private int idParte;
        private int idTipoParte;
        private int idEmpresa;
        private int numeroP;
        private DateTime FechaP;
        private string nombreCortoE;
        private string nombreE;
        private string Comentario;
        private string DestinatariosDireccion;
        private string DestinatariosEmail;
        private string Destinatarios;
        private List<MediosOpticosFixModel> losMO;
        private List<MailModel> losMails;

        [Display(Name = "ID Parte")]
        public int idParteV => idParte;
        [Display(Name = "ID Tipo Parte")]
        public int idTipoParteV => idTipoParte;
        [Display(Name = "ID Empresa")]
        public int idEmpresaV => idEmpresa;
        [Display(Name = "Número Parte")]
        public int numeroPV => numeroP;
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha Parte")]
        public DateTime FechaPV => FechaP;
        [Display(Name = "Empresa")]
        public string nombreCortoEV => nombreCortoE;
        [Display(Name = "Nombre Empresa")]
        public string nombreEV => nombreE;
        [Display(Name = "Comentario")]
        public string ComentarioV => Comentario;
        [Display(Name = "Destinatarios Dirección")]
        public string DestinatariosDireccionV => DestinatariosDireccion;
        [Display(Name = "Destinatarios Email")]
        public string DestinatariosEmailV => DestinatariosEmail;
        [Display(Name = "Destinatarios")]
        public string DestinatariosV => Destinatarios;


        public PartesFixModel() {  }
        public PartesFixModel(PartesModel unParte) 
        {
            idParte = unParte.idParte ?? 0;
            idTipoParte = unParte.idTipoParte ?? 0;
            idEmpresa = unParte.idEmpresa ?? 0;
            numeroP = unParte.numeroP ?? 0;
            FechaP = arregloFecha(unParte.FechaP);
            Comentario = unParte.Comentario;
            DestinatariosDireccion = unParte.DestinatariosDireccion;
            DestinatariosEmail = unParte.DestinatariosEmail;
            unParte.Destinatarios = unParte.Destinatarios;
        }
        private DateTime arregloFecha(string laFechaRaw)
        {
            string stringEntero;
            if(laFechaRaw == null | laFechaRaw == "0" | laFechaRaw == "")
            {
                stringEntero = "01/01/0001";
            }
            else
            {
                List<string> pedazos = laFechaRaw.Split("/").ToList();
                if (pedazos[0].Length == 1)
                {
                    pedazos[0] = "0" + pedazos[0];
                }
                if (pedazos[1].Length == 1)
                {
                    pedazos[1] = "0" + pedazos[1];
                }
                stringEntero = pedazos[0] + "/" + pedazos[1] + "/" + pedazos[2];
            }
            DateTime laFecha = DateTime.ParseExact(stringEntero, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return laFecha;
        }

        public List<string> darMensajesMails()
        {
            var mensajes = new List<string>();
            foreach(var unMail in losMails)
            {
                mensajes.Add(unMail.mensajeV);
            }
            return mensajes;
        }
        //      ---- SETs AND GETs ----
        //SET
        public void setNombresE(EmpresasModel unaEmpresa)
        {
            nombreCortoE = unaEmpresa.NombreCortoE ?? "";
            nombreE = unaEmpresa.NombreE ?? "";
        }
        public void setMOs(List<MediosOpticosFixModel> losNuevosMO)
        {
            losMO = losNuevosMO;
        }
        public void setMails(List<MailModel> losMailsNuevos)
        {
            losMails = losMailsNuevos;
        }
        public void setIdParte(int elId)
        {
            idParte = elId;
        }
        public void setIdEmpresa(int elId)
        {
            idEmpresa = elId;
        }
        public void setNumeroP(int elNumeroP)
        {
            numeroP = elNumeroP;
        }
        public void setFechaP(DateTime laFecha)
        {
            FechaP = laFecha;
        }
        public void setComentario(string elComentario)
        {
            Comentario = elComentario;
        }
        public void setDD(string laDD)
        {
            DestinatariosDireccion = laDD;
        }
        public void setDE(string elDE)
        {
            DestinatariosEmail = elDE;
        }
        public void setD(string laD)
        {
            Destinatarios = laD;
        }
        //SET MO
        public void setArchivosDeMO(List<ArchivosFixModel> losA, int indice)
        {
            losMO[indice].setArchivos(losA);
        }
        //SET Archivos
        public void setByIndiceLibroA(int indice, int indiceA, LibrosModel elLibro)
        {
            losMO[indice].setLibroA(indiceA, elLibro);
        }

        //GET

        //GET MOs
        public int getCantidadMOs()
        {
            return losMO.Count();
        }
        public string getByIndiceNombreMO(int indice)
        {
            return losMO[indice].NombreMOV;
        }
        public string getByIndicePreiodoMO(int indice)
        {
            return losMO[indice].PeriodoMOStirngV;
        }
        public int getIdMO(int indice)
        {
            return losMO[indice].IdMedioOpticoV;
        }
        public int getQAdeMO(int indice)
        {
            return losMO[indice].getCantidadArchivos();
        }

        //GET Archivos
        public string getNLAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceNombre(indiceArchivo);
        }
        public int getFAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceFraccion(indiceArchivo);
        }
        public int getFIAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceFolioI(indiceArchivo);
        }
        public int getFFAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceFolioF(indiceArchivo);
        }
        public string getHAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceHashA(indiceArchivo);
        }
        public int getAIAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceAsientoI(indiceArchivo);
        }
        public int getAFAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceAsientoF(indiceArchivo);
        }
        public int getIDLAdeMO(int indice, int indiceArchivo)
        {
            return losMO[indice].getByIndiceIdLibro(indiceArchivo);
        }
    }
}
