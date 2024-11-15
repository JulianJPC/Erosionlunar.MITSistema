using Erosionlunar.MITSistema.Abstract;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Erosionlunar.MITSistema.ManipuladorControl
{
    public class Manipulador1_1 : ABSManipulador
    {
        public override DateTime getFecha(string dirA) 
        {
            List<string> lasLineas = getLineas(dirA, 2);
            Regex regex = new Regex(@"\d{2}/\d{4}");
            Match match = regex.Match(lasLineas[1]);
            string fechaRaw = match.ValueSpan.ToString();
            List<string> fechaEnPartes = fechaRaw.Split('/').ToList();
            var laFecha = DateTime.ParseExact(String.Join('/', "01", fechaEnPartes[0], fechaEnPartes[1]), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return laFecha;
        }
    }
}
