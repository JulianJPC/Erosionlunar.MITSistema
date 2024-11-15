using DiscUtils.Iso9660;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Erosionlunar.MITSistema.ISOControl
{
    public class ISOControl
    {
        public void crearIso(List<string> CarpetasASumar, List<string> ArchivosASumar, List<string> ArchivosASumarPathIso, string direccionIso, string nombreIso, DateTime fechaDeArchivos)
        {
            fechaDeArchivos = devolverFechaDateTime(fechaDeArchivos);
            CDBuilder iso = new CDBuilder { UseJoliet = true, VolumeIdentifier = nombreIso };
            foreach (string carpeta in CarpetasASumar)
            {
                iso.AddDirectory(carpeta).CreationTime = fechaDeArchivos;
            }
            for (int indice = 0; indice < ArchivosASumar.Count; indice++)
            {
                FileInfo elArchivo = new FileInfo(ArchivosASumar[indice]);
                iso.AddFile(ArchivosASumarPathIso[indice], elArchivo.FullName).CreationTime = fechaDeArchivos;
            }
            iso.Build(direccionIso);
        }
        private DateTime devolverFechaDateTime(DateTime fecha)
        {
            Random rnd = new Random();
            DateTime mesSiguiente = fecha.AddMonths(1);
            string mes = mesSiguiente.Month.ToString();
            if(mes.Length == 1) { mes = "0" +  mes; }
            string year = fecha.Year.ToString().Substring(2, 2);
            string diaRandom = rnd.Next(1, 15).ToString();
            if (diaRandom.Length == 1) { diaRandom = "0" + diaRandom; }
            string horaRandom = rnd.Next(9, 18).ToString();
            if (horaRandom.Length == 1) { horaRandom = "0" + horaRandom; }
            string minutoRandom = rnd.Next(0, 59).ToString();
            if (minutoRandom.Length == 1) { minutoRandom = "0" + minutoRandom; }
            string segundoRandom = rnd.Next(0, 59).ToString();
            if (segundoRandom.Length == 1) { segundoRandom = "0" + segundoRandom; }
            string text = "20" + year + "-" + mes + "-" + diaRandom + " " + horaRandom + ":" + minutoRandom + ":" + segundoRandom;
            string format = "yyyy-MM-dd HH:mm:ss";
            DateTime fechaRandom = DateTime.ParseExact(
                text,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            return fechaRandom;
        }
        
    }
}
