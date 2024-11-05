using Erosionlunar.MITSistema.Abstract;
using Erosionlunar.MITSistema.Entities;
using System.Globalization;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Erosionlunar.MITSistema.ProcesadorControl
{
    public class Procesador1_2 : ABSProcesador
    {
        public override void setInicial(string unNombreC, string? unaFecha, int unaParte, string? unaTerm, string? unPath, int unIdLibro, int unIdEmpresa)
        {
            idEmpresa = unIdEmpresa;
            idLibro = unIdLibro;
            nombreABien = unNombreC + unaFecha;
            if(unaParte != 0)
            {
                nombreABien += "P" + unaParte + unaTerm;
            }
            else
            {
                nombreABien += unaTerm;
            }
            string mes = unaFecha.Substring(0, 2);
            string year = 20 + unaFecha.Substring(2, 2);
            fecha = DateTime.ParseExact(String.Join('/', "01", mes, year), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            nombreEnDisco = unPath;
            fraccion = unaParte;
        }
        public override void modificarArchivo()
        {
            modificarRapido(20, "Página: ");
        }
    }
}
