using EduDirectory3.Models;

namespace EduDirectory3.Areas.Panel.ViewModels
{
    public class SedeViewModel
    {
        public Sede Sede { get; set; } = new();
        public List<Ubicacion> Ubicaciones { get; set; } = new();
        public List<Coordenada> Coordenadas { get; set; } = new();
        public List<Telefono> Telefonos { get; set; } = new();
        public List<Servicio> Servicios { get; set; } = new();
        public List<Nivel> Niveles { get; set; } = new();
        public List<Jornada> Jornadas { get; set; } = new();
        public List<ActividadesExtracurriculares> Actividades { get; set; } = new();
    }

    public class InstitucionDetalleViewModel
    {
        public Institucion Institucion { get; set; } = new();
        public List<Sede> Sedes { get; set; } = new();
        public List<Rector> Rectores { get; set; } = new();
        public List<ResultadosIcfes> ResultadosIcfes { get; set; } = new();
        public List<Email> Emails { get; set; } = new();
    }
}
