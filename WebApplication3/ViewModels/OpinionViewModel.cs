namespace EduDirectory3.ViewModels
{
    public class OpinionViewModel
    {
        public List<string> Comentarios { get; set; } = new();      
        public List<string> Nombres { get; set; } = new();          
        public List<string> Apellidos { get; set; } = new();        
        public List<decimal> Ambiente { get; set; } = new();
        public List<decimal> Seguridad { get; set; } = new();
        public List<decimal> Metodologia { get; set; } = new();
        public List<decimal> Profesores { get; set; } = new();
        public List<decimal> ActividadesEx { get; set; } = new();
        public List<decimal> Infraestructura { get; set; } = new();
        public double PromedioRating { get; set; }
        public int TotalReseñas { get; set; }
        public List<int> IdEvaluacion { get; internal set; }
    }
}