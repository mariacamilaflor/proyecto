namespace EduDirectory3.ViewModels
{
    public class FormularioPreferenciaViewModel
    {
        // Datos básicos
        public decimal Presupuesto { get; set; }
        public int EdadEstudiante { get; set; }
        public string TipoPreferencia { get; set; } = "";
        public string MetodologiaPreferencia { get; set; } = "";

        // Selecciones múltiples
        public List<string> ServiciosPreferencia { get; set; } = new();
        public List<string> JornadasPreferencia { get; set; } = new();
        public List<string> NivelesPreferencia { get; set; } = new();
        public List<string> ActividadesPreferencia { get; set; } = new();

    }
}
