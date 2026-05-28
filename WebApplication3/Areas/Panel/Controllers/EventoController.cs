using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduDirectory3.Areas.Panel.Controllers
{
    [Area("Panel")]
    [Authorize(Roles = "Institucion")]
    public class EventoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Institucion?> GetInstitucionActualAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);
            if (usuario == null) return null;
            return await _context.Institucion
                .FirstOrDefaultAsync(i => i.IdUsuario == usuario.IdUsuario);
        }

        // GET — Lista de eventos de la institución
        public async Task<IActionResult> Index()
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null) return Unauthorized();

            var eventos = await _context.Evento
                .Where(e => e.IdInstitucion == institucion.IdInstitucion)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();

            return View(eventos);
        }

        // GET — Formulario crear evento
        public IActionResult Crear() => View();

        // POST — Guardar evento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Evento evento)
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null) return Unauthorized();

            evento.IdInstitucion = institucion.IdInstitucion;
            _context.Evento.Add(evento);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Evento creado correctamente.";
            return RedirectToAction("Index");
        }

        // POST — Eliminar evento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idEvento)
        {
            var evento = await _context.Evento.FindAsync(idEvento);
            if (evento != null)
            {
                _context.Evento.Remove(evento);
                await _context.SaveChangesAsync();
            }

            TempData["Exito"] = "Evento eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}