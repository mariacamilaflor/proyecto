namespace EduDirectory3.ViewModels
{
    public class InstitucionResultado
    {
        public int IdInstitucion { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Metodologia { get; set; } = "";
        public string Calendario { get; set; } = "";
        public decimal Costo { get; set; }  
        public string PaginaWeb { get; set; } = "";
        public string HorarioAtencion { get; set; } = "";
        public string NombreSede { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Barrio { get; set; } = "";
        public string Comuna { get; set; } = "";
        public string NumeroTelefono { get; set; } = "";
    }
}