using Microsoft.EntityFrameworkCore;


namespace Erosionlunar.MITSistema.Entities

{
    public class MyAppContext:DbContext
    {
        public MyAppContext(DbContextOptions<MyAppContext> options) : base(options)
        {
        }

        // Define DbSets for your tables
        public DbSet<PreParteModel> PreParte { get; set; }
        public DbSet<EmpresasModel> Empresas { get; set; }
        public DbSet<PartesModel> Partes { get; set; }
        public DbSet<MailsDeParteModel> MailsDeParte { get; set; }
        public DbSet<InformacionEmpresaModel> InformacionEmpresa { get; set; }
        public DbSet<TipoInformacionModel> TipoInformacion { get; set; }
        public DbSet<MediosOpticosModel> MediosOpticos { get; set; }
        public DbSet<ArchivosModel> Archivos { get; set; }
        public DbSet<InformacionLibroModel> InformacionLibro { get; set; }
        public DbSet<LibrosModel> Libros { get; set; }
        public DbSet<MainFrameModel> MainFrame { get; set; }
        public DbSet<ArchivosFechasModel> archivosFechas { get; set; }
        public DbSet<regexLibrosModel> regexLibros { get; set; }
        public DbSet<InformacionActaModel> informacionActa { get; set;}
    }
}
