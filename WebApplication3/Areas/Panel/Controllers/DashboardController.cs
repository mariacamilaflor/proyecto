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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper: obtiene la institucion del usuario logueado
        private async Task<Institucion?> GetInstitucionActualAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);
            if (usuario == null) return null;
            return await _context.Institucion
                .Include(i => i.Sede)
                .Include(i => i.Rector)
                .FirstOrDefaultAsync(i => i.IdUsuario == usuario.IdUsuario);
        }

        // ─────────────────────────────────────────────
        // INDEX - Dashboard principal
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null)
                return RedirectToAction("Crear");

            var sedeIds = institucion.Sede.Select(s => s.IdSede).ToList();

            var mensajesSinLeer = await _context.FormularioContacto
                .Where(f => sedeIds.Contains(f.IdSede))
                .CountAsync();

            var evaluaciones = await _context.Evaluacion
                .Include(e => e.Calificacion)
                .Include(e => e.IdUsuarioNavigation)
                .Where(e => sedeIds.Contains(e.IdSede))
                .ToListAsync();

            var promedioGeneral = evaluaciones.Any()
                ? evaluaciones.SelectMany(e => e.Calificacion).Select(c =>
                    (c.CalificacionAmbienteEscolar + c.CalificacionMetodologia + c.CalificacionSeguridad +
                     c.CalificacionProfesores + c.CalificacionActividadesExtracurriculares + c.CalificacionInfraestructura) / 6)
                    .DefaultIfEmpty(0).Average()
                : 0;

            ViewBag.Institucion = institucion;
            ViewBag.TotalMensajes = mensajesSinLeer;
            ViewBag.TotalEvaluaciones = evaluaciones.Count;
            ViewBag.PromedioGeneral = Math.Round(promedioGeneral, 1);
            ViewBag.TotalSedes = institucion.Sede.Count;

            return View();
        }

        // ─────────────────────────────────────────────
        // CRUD INSTITUCIÓN
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Perfil()
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null)
                return RedirectToAction("Crear");
            return View(institucion);
        }

        public async Task<IActionResult> Crear()
        {
            // Solo si aún no tiene institución
            var institucion = await GetInstitucionActualAsync();
            if (institucion != null)
                return RedirectToAction("Perfil", "Dashboard", new { area = "Panel" });
            return View(new Institucion());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Institucion model, IFormFile? imagenFile)
        {
            // Quitar campos que no vienen del form
            ModelState.Remove("Imagen");
            ModelState.Remove("IdUsuarioNavigation");
            ModelState.Remove("Email");
            ModelState.Remove("Rector");
            ModelState.Remove("Sede");
            ModelState.Remove("ResultadosIcfes");

            // Buscar usuario por Name (más confiable que ClaimTypes.Email)
            var email = User.Identity?.Name;
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null)
            {
                ModelState.AddModelError("", $"No se encontró el usuario con correo: {email}");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value!.Errors.Any())
                    .Select(x => $"{x.Key}: {x.Value!.Errors.First().ErrorMessage}");
                ModelState.AddModelError("", "Errores: " + string.Join(" | ", errores));
                return View(model);
            }

            if (imagenFile != null && imagenFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imagenFile.FileName);
                var path = Path.Combine("wwwroot/Imagen/", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using var stream = new FileStream(path, FileMode.Create);
                await imagenFile.CopyToAsync(stream);
                model.Imagen = "/Imagen/" + fileName;
            }
            else
            {
                model.Imagen = "/Imagen/school.png"; // imagen por defecto
            }

            model.IdUsuario = usuario.IdUsuario;
            _context.Institucion.Add(model);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Institución creada correctamente.";
            return RedirectToAction("Index", "Dashboard", new { area = "Panel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Institucion model, IFormFile? imagenFile)
        {
            var institucion = await _context.Institucion.FindAsync(model.IdInstitucion);
            if (institucion == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imagenFile.FileName);
                    var path = Path.Combine("wwwroot/Imagen/", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    using var stream = new FileStream(path, FileMode.Create);
                    await imagenFile.CopyToAsync(stream);
                    institucion.Imagen = "/Imagen/" + fileName;
                }

                institucion.Nombre = model.Nombre;
                institucion.PaginaWeb = model.PaginaWeb;
                institucion.Metodologia = model.Metodologia;
                institucion.Calendario = model.Calendario;
                institucion.Costo = model.Costo;
                institucion.Tipo = model.Tipo;
                institucion.Descripcion = model.Descripcion;
                institucion.HorarioAtencion = model.HorarioAtencion;

                await _context.SaveChangesAsync();
                TempData["Exito"] = "Perfil actualizado correctamente.";
                return RedirectToAction("Perfil");
            }
            return View("Perfil", model);
        }

        // ─────────────────────────────────────────────
        // MENSAJES (FormularioContacto)
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Mensajes()
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null) return RedirectToAction("Crear");

            var sedeIds = institucion.Sede.Select(s => s.IdSede).ToList();

            var mensajes = await _context.FormularioContacto
                .Include(f => f.IdUsuarioNavigation)
                .Include(f => f.IdSedeNavigation)
                .Where(f => sedeIds.Contains(f.IdSede))
                .OrderByDescending(f => f.IdFormularioContacto)
                .ToListAsync();

            ViewBag.Institucion = institucion;
            return View(mensajes);
        }

        // ─────────────────────────────────────────────
        // EVALUACIONES / VALORACIONES
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Evaluaciones()
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null) return RedirectToAction("Crear");

            var sedeIds = institucion.Sede.Select(s => s.IdSede).ToList();

            var evaluaciones = await _context.Evaluacion
                .Include(e => e.Calificacion)
                .Include(e => e.IdUsuarioNavigation)
                .Include(e => e.IdSedeNavigation)
                .Where(e => sedeIds.Contains(e.IdSede))
                .OrderByDescending(e => e.IdEvaluacion)
                .ToListAsync();

            ViewBag.Institucion = institucion;
            return View(evaluaciones);
        }
    }
}
