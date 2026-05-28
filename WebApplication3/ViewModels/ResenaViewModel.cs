using System.ComponentModel.DataAnnotations;
namespace EduDirectory3.ViewModels
{
    public class ResenaViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una sede")]
        public int IdSede { get; set; }
        [Required(ErrorMessage = "El comentario es obligatorio")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 500 caracteres")]
        public string Comentario { get; set; } = "";
        public decimal CalificacionInfraestructura { get; set; }
        public decimal CalificacionProfesores { get; set; }
        public decimal CalificacionMetodologia { get; set; }
        public decimal CalificacionAmbienteEscolar { get; set; }
        public decimal CalificacionSeguridad { get; set; }
        public decimal CalificacionActividadesExtracurriculares { get; set; }
        public int IdInstitucion { get; set; }
        public string NombreInstitucion { get; set; } ="";
        public List<SedeSelectItem> Sedes { get; set; } = new();
    }
    public class SedeSelectItem
    {
        public int IdSede { get; set; }
        public string Nombre { get; internal set; }
    }
}
