using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edu.Areas.Panel.Controllers
{
    [Area("Panel")]
    [Authorize(Roles = "Institucion")]
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PerfilController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =============================================
        // HELPER — Obtener Usuario propio por correo
        // =============================================
        private async Task<Usuario?> GetUsuarioActualAsync()
        {
            var correo = User.Identity?.Name;
            if (string.IsNullOrEmpty(correo)) return null;
            return await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);
        }

        // =============================================
        // INDEX — Ver información de la institución
        // =============================================
        public async Task<IActionResult> Index()
        {
            var usuario = await GetUsuarioActualAsync();
            if (usuario == null) return Unauthorized();

            var inst = await _context.Institucion
                .Include(i => i.Email)
                .Include(i => i.Rector)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Ubicacion)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Telefono)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Nivel)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Jornada)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Servicio)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.ActividadesExtracurriculares)
                .FirstOrDefaultAsync(i => i.IdUsuario == usuario.IdUsuario);

            if (inst == null)
                return RedirectToAction("Edit");

            ViewData["Title"] = "Mi Institución";
            return View(inst);
        }

        // =============================================
        // EDIT GET — Formulario de edición
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var usuario = await GetUsuarioActualAsync();
            if (usuario == null) return Unauthorized();

            var inst = await _context.Institucion
                .Include(i => i.Email)
                .Include(i => i.Rector)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Ubicacion)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Telefono)
                .FirstOrDefaultAsync(i => i.IdUsuario == usuario.IdUsuario);

            // Si no existe aún, se crea un objeto vacío para el formulario de registro
            if (inst == null)
            {
                inst = new Institucion { IdUsuario = usuario.IdUsuario };
            }

            ViewData["Title"] = inst.IdInstitucion == 0 ? "Registrar Institución" : "Editar Institución";
            return View(inst);
        }

        // =============================================
        // EDIT POST — Guardar cambios
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Institucion model)
        {
            var usuario = await GetUsuarioActualAsync();
            if (usuario == null) return Unauthorized();

            model.IdUsuario = usuario.IdUsuario;

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = model.IdInstitucion == 0 ? "Registrar Institución" : "Editar Institución";
                return View(model);
            }

            var existe = await _context.Institucion
                .FirstOrDefaultAsync(i => i.IdInstitucion == model.IdInstitucion && i.IdUsuario == usuario.IdUsuario);

            if (existe == null)
            {
                // CREAR
                _context.Institucion.Add(model);
                TempData["Exito"] = "Institución registrada exitosamente.";
            }
            else
            {
                // ACTUALIZAR
                existe.Nombre = model.Nombre;
                existe.PaginaWeb = model.PaginaWeb;
                existe.Imagen = model.Imagen;
                existe.Metodologia = model.Metodologia;
                existe.Calendario = model.Calendario;
                existe.Tipo = model.Tipo;
                existe.Costo = model.Costo;
                existe.HorarioAtencion = model.HorarioAtencion;
                existe.Descripcion = model.Descripcion;

                _context.Institucion.Update(existe);
                TempData["Exito"] = "Institución actualizada exitosamente.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // =============================================
        // DELETE GET — Confirmación de eliminación
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var usuario = await GetUsuarioActualAsync();
            if (usuario == null) return Unauthorized();

            var inst = await _context.Institucion
                .Include(i => i.Sede)
                .FirstOrDefaultAsync(i => i.IdUsuario == usuario.IdUsuario);

            if (inst == null)
                return RedirectToAction("Index");

            ViewData["Title"] = "Eliminar Institución";
            return View(inst);
        }

        // =============================================
        // DELETE POST — Ejecutar eliminación
        // =============================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var usuario = await GetUsuarioActualAsync();
            if (usuario == null) return Unauthorized();

            var inst = await _context.Institucion
                .Include(i => i.Email)
                .Include(i => i.Rector)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Ubicacion)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Telefono)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Nivel)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Jornada)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Servicio)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.ActividadesExtracurriculares)
                .FirstOrDefaultAsync(i => i.IdUsuario == usuario.IdUsuario);

            if (inst == null)
                return RedirectToAction("Index");

            _context.Institucion.Remove(inst);
            await _context.SaveChangesAsync();

            // También eliminar la cuenta Identity si se desea
            var identityUser = await _userManager.FindByNameAsync(usuario.Correo);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
