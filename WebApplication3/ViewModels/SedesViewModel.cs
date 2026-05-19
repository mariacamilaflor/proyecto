using EduDirectory3.Models;
using System.ComponentModel.DataAnnotations;

namespace EduDirectory3.ViewModels
{
    public class SedesViewModel
    {
        public List<string> NombreSede { get; set; } = new();
        public string? Comuna { get; set; }
        public string? Barrio { get; set; }
        public List<List<string>> Telefono { get; set; } = new();
        public List<string> Email { get; set; } = new();
        public List<string> PerteneceA { get; set; } = new();
        public string? Direccion { get; set; }
    }
}
