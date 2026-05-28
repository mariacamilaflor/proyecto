using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduDirectory3.Models
{
    public class Favorito
    {
        [Key]
        public int IdFavorito { get; set; }
        public int IdUsuario { get; set; }
        public int IdInstitucion { get; set; }
        public DateTime FechaAgregado { get; set; } = DateTime.Now;
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }
        [ForeignKey("IdInstitucion")]
        public virtual Institucion Institucion { get; set; }
    }
}
