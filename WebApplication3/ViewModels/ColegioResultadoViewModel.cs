using EduDirectory3.Models;

namespace EduDirectory3.ViewModels
{
    public class ColegioResultadoViewModel
    {
        public int IdInstitucion { get; set; }
        public string Nombre { get; set; } = "";
        public string Imagen { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Metodologia { get; set; } = "";
        public decimal? Costo { get; set; }
        public string Descripcion { get; internal set; } = "";
        public string Calendario { get; internal set; } = "";
        public string PaginaWeb { get; internal set; } = "";
        public string HorarioAtencion { get; internal set; } = "";
        public AcademicoViewModel Academico { get; set; } = new();
        public SedesViewModel Sedes { get; internal set; } = new();
        public OpinionViewModel Opinion { get; internal set; } = new();
        public ContactoViewModel Contacto { get; internal set; } = new();
    }
}
