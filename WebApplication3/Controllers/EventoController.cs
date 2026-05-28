using EduDirectory3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduDirectory3.Controllers
{
    public class EventoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Evento
                .Include(e => e.Institucion)
                .Where(e => e.Fecha >= DateTime.Today)
                .OrderBy(e => e.Fecha)
                .ToListAsync();

            return View(eventos);
        }
    }
}
