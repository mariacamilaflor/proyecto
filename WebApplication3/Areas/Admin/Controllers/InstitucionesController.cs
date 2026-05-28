using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduDirectory3.ViewModels;
namespace EduDirectory3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InstitucionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstitucionesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =============================================
        // INDEX — Listar todas las instituciones
        // =============================================
        public async Task<IActionResult> Index()
        {
            var instituciones = await _context.Institucion
                .Include(i => i.Sede)
                .Include(i => i.IdUsuarioNavigation)
                .ToListAsync();

            ViewData["Title"] = "Gestión de Instituciones";
            return View(instituciones);
        }

        // =============================================
        // DETAILS — Ver detalle de una institución
        // =============================================
        public async Task<IActionResult> Details(int id)
        {
            var inst = await _context.Institucion
                .Include(i => i.Rector)
                .Include(i => i.Email)
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
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(i => i.IdInstitucion == id);

            if (inst == null) return NotFound();

            ViewData["Title"] = inst.Nombre;
            return View("Edit", inst);
        }

        // =============================================
        // CREATE GET
        // =============================================
        [HttpGet]
        public IActionResult Create()
        {
            TempData["RolRegistro"] = "Institucion";
            return Redirect("/Identity/Account/Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearUsuarioAdminViewModel model)
        {
            model.Rol = "Institucion"; // forzar rol siempre
            model.Apellido = model.Apellido ?? "-";

            if (!ModelState.IsValid)
                return View(model);

            // 1. Verificar que el correo no exista
            var existe = await _userManager.FindByEmailAsync(model.Correo);
            if (existe != null)
            {
                ModelState.AddModelError("Correo", "Ya existe una cuenta con ese correo.");
                return View(model);
            }

            // 2. Crear cuenta en Identity
            var identityUser = new ApplicationUser
            {
                UserName = model.Correo,
                Email = model.Correo,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(identityUser, model.Contrasena);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            // 3. Asignar rol Institucion
            await _userManager.AddToRoleAsync(identityUser, "Institucion");

            // 4. Crear en tabla Usuario propia
            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Apellido = "-",
                Correo = model.Correo,
                Contrasena = identityUser.PasswordHash ?? "",
                Rol = "Institucion"
            };
            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Cuenta de institución creada para {model.Correo}.";
            return RedirectToAction("Index");
        }

        // =============================================
        // EDIT GET
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var inst = await _context.Institucion
                .Include(i => i.Rector)
                .FirstOrDefaultAsync(i => i.IdInstitucion == id);

            if (inst == null) return NotFound();

            var usuarios = _context.Usuario.ToList(); // o el método que uses para traerlos
            ViewBag.Usuarios = usuarios;
            return View(inst);
        }

        // =============================================
        // EDIT POST
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Institucion model)
        {
            if (id != model.IdInstitucion) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = await _context.Usuario
                    .Where(u => u.Rol == "Institucion").ToListAsync();
                return View(model);
            }

            var existe = await _context.Institucion.FindAsync(id);
            if (existe == null) return NotFound();

            existe.Nombre = model.Nombre;
            existe.PaginaWeb = model.PaginaWeb;
            existe.Imagen = model.Imagen;
            existe.Metodologia = model.Metodologia;
            existe.Calendario = model.Calendario;
            existe.Tipo = model.Tipo;
            existe.Costo = model.Costo;
            existe.HorarioAtencion = model.HorarioAtencion;
            existe.Descripcion = model.Descripcion;
            existe.IdUsuario = model.IdUsuario;

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Institución actualizada.";
            return RedirectToAction("Index");
        }

        // =============================================
        // DELETE POST
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inst = await _context.Institucion
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Evaluacion)
                .Include(i => i.Email)
                .Include(i => i.Rector)
                .FirstOrDefaultAsync(i => i.IdInstitucion == id);

            if (inst == null) return NotFound();

            _context.Institucion.Remove(inst);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Institución eliminada.";
            return RedirectToAction("Index");
        }
    }
}