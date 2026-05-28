using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduDirectory3.Controllers
{
    [Authorize]
    public class FormularioContactoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormularioContactoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================================
        // GET — Mostrar formulario de contacto
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Crear(int idInstitucion)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null)
                return Unauthorized();

            var institucion = await _context.Institucion
                .Include(i => i.Sede)
                .FirstOrDefaultAsync(i => i.IdInstitucion == idInstitucion);

            if (institucion == null)
                return NotFound();

            ViewBag.Usuario = usuario;
            ViewBag.Institucion = institucion;
            ViewBag.Sedes = institucion.Sede;

            return View();
        }

        // =============================================
        // POST — Enviar formulario de contacto
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int idSede, string mensaje)
        {
            // Obtener usuario logueado
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null)
            {
                TempData["MensajeLogin"] = "Para usar esta funcionalidad debes iniciar sesión.";
                return RedirectToAction("Login", "Account");
            }
            var sede = await _context.Sede
                .Include(s => s.IdInstitucionNavigation)
                .FirstOrDefaultAsync(s => s.IdSede == idSede);

            if (sede == null) return NotFound();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                ModelState.AddModelError("", "Nombre y mensaje son obligatorios.");

                ViewBag.Institucion = sede.IdInstitucionNavigation;
                ViewBag.Sedes = await _context.Sede
                    .Where(s => s.IdInstitucion == sede.IdInstitucion)
                    .ToListAsync();

                return View();
            }

            var formulario = new FormularioContacto
            {
                Nombre = usuario.Nombre,
                Mensaje = mensaje,
                IdUsuario = usuario.IdUsuario,
                IdSede = idSede
            };

            _context.FormularioContacto.Add(formulario);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Tu mensaje fue enviado correctamente. La institución se pondrá en contacto contigo.";
            return RedirectToAction("Crear", new { idInstitucion = sede.IdInstitucion });
        }
    }
}

