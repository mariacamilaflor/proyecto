namespace EduDirectory3.ViewModels
{
    public class ContactoViewModel
    {
        public List<string> RectorNombre { get; internal set; } = new();
        public List<string> RectorApellido { get; internal set; } = new();
        public List<string> RectorTelefono { get; internal set; } = new();
        public List<string> RectorEmail { get; internal set; } = new();
    }
}
