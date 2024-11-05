using System.ComponentModel.DataAnnotations;

namespace Erosionlunar.MITSistema.Entities
{
    public class regexLibrosModel
    {

        [Key]
        public int idRegexLibros { get; set; }
        public string elRegex { get; set; }
        public int idTipoRegex { get; set; }
        public int idLibro { get; set; }
    }
}