using EduDirectory3.Models;

namespace EduDirectory3.ViewModels
{
    public class AcademicoViewModel
    {
        public List<List<string>> Niveles { get;  set; } = new();
        public List<List<string>> Jornadas { get;  set; } = new();
        public List<List<string>> Servicios { get;  set; } = new();
        public List<decimal> Puntajes { get;  set; } = new();
        public List<int> Anios { get;  set; } = new();
        public List<List<string>> ActividadesExtracurriculares { get; set; } = new();
    }
}
