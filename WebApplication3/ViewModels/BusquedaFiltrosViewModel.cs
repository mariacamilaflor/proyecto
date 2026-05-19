using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduDirectory3.ViewModels
{
    public class BusquedaFiltrosViewModel
    {
        // Texto libre
        public string? Nombre { get; set; }

        // Filtros de Nivel y Jornada (IDs)
        public List<int> NivelesSeleccionados { get; set; } = new();
        public List<int> JornadasSeleccionadas { get; set; } = new();
        public List<int> ServiciosSeleccionados { get; set; } = new();

        // Filtros de Institucion
        public string? Tipo { get; set; }
        public string? Metodologia { get; set; }
        public string? Calendario { get; set; }
        public decimal? CostoMax { get; set; }

        // Filtro de Ubicacion
        public string? Comuna { get; set; }

        // Ordenamiento y paginación
        public string OrdenarPor { get; set; } = "valoracion";
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
        public int TotalItems { get; set; }

        // Listas para los dropdowns del formulario
        public List<SelectListItem> Niveles { get; set; } = new();
        public List<SelectListItem> Jornadas { get; set; } = new();
        public List<SelectListItem> Servicios { get; set; } = new();
        public List<string> Comunas { get; set; } = new();
     
        // Resultados
        public List<ColegioResultadoViewModel> Resultados { get; set; } = new();

        // Helper para paginación
        public int TotalPaginas =>
            (int)Math.Ceiling((double)TotalItems / TamañoPagina);
    }
}
