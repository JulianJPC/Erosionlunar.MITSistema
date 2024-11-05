namespace Erosionlunar.MITSistema.Interface
{
    public interface IProcesadorArchivos
    {
        public void setInicial(string unNombreC, string? unaFecha, int unaParte, string? unaTerm, string? unPath, int unIdLibro, int unIdEmpresa);
        public DateTime getFecha();
        public void modificarArchivo();
        public void setInicioE(int elInicio);
        public bool necesitaFolios();
    }
}
