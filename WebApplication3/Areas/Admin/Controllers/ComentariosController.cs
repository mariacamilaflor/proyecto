using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduDirectory3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ComentariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComentariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================================
        // INDEX — Listar comentarios reportados
        // =============================================
        public async Task<IActionResult> Index()
        {
            var reportados = await _context.Evaluacion
                .Include(e => e.IdUsuarioNavigation)
                .Include(e => e.IdSedeNavigation)
                .Where(e => e.Reportado == true)
                .OrderByDescending(e => e.IdEvaluacion)
                .ToListAsync();

            ViewData["Title"] = "Comentarios Reportados";
            return View(reportados);
        }

        // =============================================
        // DELETE — Eliminar comentario reportado
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var evaluacion = await _context.Evaluacion
                .Include(e => e.Calificacion)  // ← carga las calificaciones hijas
                .Include(e => e.IdUsuarioNavigation)
                .FirstOrDefaultAsync(e => e.IdEvaluacion == id);

            if (evaluacion == null) return NotFound();

            // 1. Primero elimina las calificaciones hijas
            _context.Calificacion.RemoveRange(evaluacion.Calificacion);

            // 2. Luego elimina la evaluación
            _context.Evaluacion.Remove(evaluacion);

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Comentario eliminado correctamente.";
            return RedirectToAction("Index");
        }

        // =============================================
        // DESMARCAR REPORTE — Marcar como revisado
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desmarcar(int id)
        {
            var evaluacion = await _context.Evaluacion.FindAsync(id);
            if (evaluacion == null) return NotFound();

            evaluacion.Reportado = false;
            evaluacion.MotivoReporte = null;
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Reporte descartado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
