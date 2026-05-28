using EduDirectory3.Models;
using System.ComponentModel.DataAnnotations;

namespace EduDirectory3.ViewModels
{
    public class SedesViewModel
    {
        public List<string> NombreSede { get; set; } = new();
        public List<string> Comuna { get; set; } = new();
        public List<string> Barrio { get; internal set; } = new();
        public List<List<string>> Telefono { get; set; } = new();
        public List<string> Email { get; set; } = new();
        public List<string> PerteneceA { get; set; } = new();
        public List<string> Direccion { get; set; } = new();
        public List<int> IdSedes { get; internal set; } = new();
    }
}
