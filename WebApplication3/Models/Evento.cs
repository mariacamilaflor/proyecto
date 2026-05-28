using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduDirectory3.Models
{
    public class Evento
    {
        [Key]
        public int IdEvento { get; set; }
        public int IdInstitucion { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string Lugar { get; set; }
        [ForeignKey("IdInstitucion")]
        public virtual Institucion Institucion { get; set; }
    }
}
