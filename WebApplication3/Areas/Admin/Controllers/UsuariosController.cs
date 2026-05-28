using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduDirectory3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuariosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =============================================
        // INDEX — Listar todos los usuarios
        // =============================================
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuario
                .OrderByDescending(u => u.IdUsuario)
                .ToListAsync();

            ViewData["Title"] = "Gestión de Usuarios";
            return View(usuarios);
        }

        // =============================================
        // DETAILS — Ver detalle de un usuario
        // =============================================
        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewData["Title"] = "Detalle de Usuario";
            return View(usuario);
        }

        // =============================================
        // EDIT GET — Formulario de edición
        // =============================================
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewData["Title"] = "Editar Usuario";
            return View(usuario);
        }

        // =============================================
        // EDIT POST — Guardar cambios
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string nombre, string apellido,
                                               string email, string? nuevaPassword, string rol)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null) return NotFound();

            var identityUser = await _userManager.FindByEmailAsync(usuario.Correo);
            if (identityUser == null) return NotFound();

            // Actualizar email
            if (email != usuario.Correo)
            {
                var token = await _userManager.GenerateChangeEmailTokenAsync(identityUser, email);
                var resultEmail = await _userManager.ChangeEmailAsync(identityUser, email, token);
                await _userManager.SetUserNameAsync(identityUser, email);

                if (!resultEmail.Succeeded)
                {
                    resultEmail.Errors.ToList().ForEach(e => ModelState.AddModelError("", e.Description));
                    return View(usuario);
                }
                usuario.Correo = email;
            }

            // Actualizar contraseña si se proporcionó
            if (!string.IsNullOrEmpty(nuevaPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                var resultPass = await _userManager.ResetPasswordAsync(identityUser, token, nuevaPassword);

                if (!resultPass.Succeeded)
                {
                    resultPass.Errors.ToList().ForEach(e => ModelState.AddModelError("", e.Description));
                    return View(usuario);
                }
                usuario.Contrasena = identityUser.PasswordHash ?? "";
            }

            // Actualizar rol
            if (rol != usuario.Rol)
            {
                var rolesActuales = await _userManager.GetRolesAsync(identityUser);
                await _userManager.RemoveFromRolesAsync(identityUser, rolesActuales);
                await _userManager.AddToRoleAsync(identityUser, rol);
                usuario.Rol = rol;
            }

            // Actualizar nombre y apellido
            usuario.Nombre = nombre;
            usuario.Apellido = apellido;

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // =============================================
        // DELETE POST — Eliminar usuario
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null) return NotFound();

            // Eliminar de AspNetUsers
            var identityUser = await _userManager.FindByEmailAsync(usuario.Correo);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);

            // Eliminar de tabla propia
            _context.Usuario.Remove(usuario);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}

