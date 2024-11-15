using Erosionlunar.MITSistema.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Erosionlunar.MITSistema.Abstract
{
    public abstract class ABSProcesador
    {
        protected int idArchivo;
        protected int idEmpresa;
        protected int idLibro;
        protected string nombreABien;
        protected string nombreEnDisco;
        protected int fraccion;
        protected DateTime fecha;
        protected int inicioE;
        protected int numeroInterno;
        protected int folioI;
        protected int folioF;
        protected int asientoI;
        protected int asientoF;
        protected Encoding encoding;
        protected List<Regex> losRegex;


        public int getIdArchivo()
        {
            return idArchivo;
        }
        public string getNombreEnDisco()
        {
            return nombreEnDisco;
        }
        public int getFolioI()
        {
            return folioI;
        }
        public int getFolioF()
        {
            return folioF;
        }
        public int getAsientoI()
        {
            return asientoI;
        }
        public int getAsientoF()
        {
            return asientoF;
        }
        private string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }
        public ArchivosModel produceAModel()
        {
            var elArchivo = new ArchivosModel();
            elArchivo.IdArchivo = idArchivo;
            elArchivo.IdMedioOptico = 0;
            elArchivo.IdLibro = idLibro;
            elArchivo.Fraccion = fraccion;
            elArchivo.FolioI = folioI;
            elArchivo.FolioF = folioF;
            elArchivo.AsientoI = asientoI;
            elArchivo.AsientoF = asientoF;
            elArchivo.HashA = CalculateMD5(nombreEnDisco);
            elArchivo.Ramificacion = -1;
            elArchivo.EsRamaActiva = -1;
            return elArchivo;
        }
        protected List<string> modificarLineaFolio(string lineaRaw, string folioCambiante, Regex unRegex, int cantidadEspacios, string nombreFolio)
        {
            List<string> lineaYFolio = new List<string>(2);
            string linea = lineaRaw.Replace("\u000C", "").TrimEnd();//\u000C ♀
            string folioNuevo = folioCambiante;
            if (unRegex.IsMatch(linea))
            {
                linea = sacarFinalYAgregarFolio(linea, folioNuevo, cantidadEspacios, nombreFolio);
                folioNuevo = (Int32.Parse(folioNuevo) + 1).ToString();
            }
            lineaYFolio.Add(linea);
            lineaYFolio.Add(folioNuevo);
            return lineaYFolio;
        }
        private string sacarFinalYAgregarFolio(string linea, string folio, int sacarDelFondo, string claseHoja)
        {
            StringBuilder nuevaLinea;
            if (linea.Length >= sacarDelFondo)
            {
                nuevaLinea = new StringBuilder(linea.Length + claseHoja.Length + folio.Length);
                nuevaLinea.Append(linea, 0, linea.Length - sacarDelFondo);
                nuevaLinea.Append(claseHoja);
                nuevaLinea.Append(folio);
            }
            else
            {
                nuevaLinea = new StringBuilder(claseHoja.Length + folio.Length);
                nuevaLinea.Append(claseHoja);
                nuevaLinea.Append(folio);
            }

            return nuevaLinea.ToString();
        }

        public void setRegex(List<regexLibrosModel> losFullRegex)
        {
            losRegex = new List<Regex>();
            foreach(regexLibrosModel unRegex in losFullRegex)
            {
                losRegex.Add(new Regex("^" + unRegex.elRegex + "$", RegexOptions.Compiled));
            }
        }
        public void setEncoding(int encod)
        {
            encoding = Encoding.GetEncoding(encod);
        }
        public virtual bool necesitaFolios()
        {
            bool necesita = false;
            if (numeroInterno == 1 & (fraccion == 0 | fraccion == 1))
            {
                folioI = 1;
                asientoI = 1;
            }
            else
            {
                necesita = true;
            }
            return necesita;
        }
        public void setIdArchivo(int idArch)
        {
            idArchivo = idArch;
        }
        public void setInicioE(int elInicio)
        {
            inicioE = elInicio;
            numeroInterno = getNumeroInterno();
        }
        private int getNumeroInterno()
        {
            int mes = fecha.Month;
            int nro = mes - inicioE + 1;
            if (nro <= 0)
            {
                nro = nro + 12;
            }
            return nro;
        }
        public virtual void setInicial(string unNombreC, string? unaFecha, int unaParte, string? unaTerm, string? unPath, int unIdLibro, int unIdEmpresa)
        {

        }
        public DateTime getFecha()
        {
            return fecha;
        }
        public virtual void modificarArchivo()
        {

        }
        public int getFraccion()
        {
            return fraccion;
        }

        public int getIdLibro()
        {
            return idLibro;
        }
        public bool setFoliosYAsiento(ArchivosModel elArchivo)
        {
            bool encontroLaInfo = false;

            if (elArchivo != null)
            {
                folioI = elArchivo.FolioF + 1 ?? 0;
                if (elArchivo.AsientoF > 20)
                {
                    asientoI = elArchivo.AsientoF + 1 ?? 0;
                }
                else
                {
                    asientoI = 0;
                }
                encontroLaInfo = true;
            }
            return encontroLaInfo;
        }
        protected void modificarRapido(int cantidadEspacios, string nombreLinea)
        {
            string folioCambiante = folioI.ToString();
            string asientoCambiante = asientoI.ToString();
            string pathNewFile = Path.Combine(Path.GetDirectoryName(nombreEnDisco), nombreABien);
            Encoding elEncodCrear = Encoding.GetEncoding(1252);

            using (StreamWriter writer = new StreamWriter(pathNewFile, true, elEncodCrear))
            {
                using (StreamReader reader = new StreamReader(nombreEnDisco, encoding))
                {
                    char[] buffer = new char[4096]; // Smaller buffer size for reduced memory usage
                    string remainder = "";

                    while (true)
                    {
                        int bytesRead = reader.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                            break;
                        string chunk = remainder + new string(buffer, 0, bytesRead);
                        string[] lines = chunk.Split('\n');

                        for (int indice = 0; indice < lines.Length - 1; indice++)
                        {
                            string linea = lines[indice];


                            List<string> lineaYFolio;

                            lineaYFolio = modificarLineaFolio(linea, folioCambiante, losRegex[0], cantidadEspacios, nombreLinea);
                            string nuevaLinea = lineaYFolio[0];
                            folioCambiante = lineaYFolio[1];
                            if (!string.IsNullOrEmpty(nuevaLinea))
                            {
                                writer.WriteLine(nuevaLinea);
                            }
                        }
                        remainder = lines[lines.Length - 1];
                    }
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        writer.WriteLine(remainder);
                    }
                }
            }
            folioF = (Int32.Parse(folioCambiante) - 1);
            asientoF = 0;

            File.Delete(nombreEnDisco);
            nombreEnDisco = pathNewFile;
        }
    }
}
