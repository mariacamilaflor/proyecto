using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduDirectory3.Controllers
{
    [Authorize]
    public class FavoritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET — Lista de favoritos del usuario
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null) return Unauthorized();

            var favoritos = await _context.Favorito
                .Include(f => f.Institucion)
                .Where(f => f.IdUsuario == usuario.IdUsuario)
                .OrderByDescending(f => f.FechaAgregado)
                .ToListAsync();

            return View(favoritos);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] int idInstitucion)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null) return Unauthorized();

            // Verificar que la institución existe
            var institucionExiste = await _context.Institucion.AnyAsync(i => i.IdInstitucion == idInstitucion);
            if (!institucionExiste) return BadRequest(new { mensaje = "Institución no encontrada" });

            var existe = await _context.Favorito.AnyAsync(f =>
                f.IdUsuario == usuario.IdUsuario &&
                f.IdInstitucion == idInstitucion);

            if (!existe)
            {
                _context.Favorito.Add(new Favorito
                {
                    IdUsuario = usuario.IdUsuario,
                    IdInstitucion = idInstitucion,
                    FechaAgregado = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Agregado a favoritos" });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar([FromBody] int idInstitucion)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null) return Unauthorized();

            var favorito = await _context.Favorito.FirstOrDefaultAsync(f =>
                f.IdUsuario == usuario.IdUsuario &&
                f.IdInstitucion == idInstitucion);

            if (favorito != null)
            {
                _context.Favorito.Remove(favorito);
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Eliminado de favoritos" });
        }

        [HttpGet]
        public async Task<IActionResult> EsFavorito(int idInstitucion)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null) return Ok(new { esFavorito = false });

            var existe = await _context.Favorito.AnyAsync(f =>
                f.IdUsuario == usuario.IdUsuario &&
                f.IdInstitucion == idInstitucion);

            return Ok(new { esFavorito = existe });
        }
        [HttpGet]
        public async Task<IActionResult> MisFavoritos()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);

            if (usuario == null) return Unauthorized();

            var ids = await _context.Favorito
                .Where(f => f.IdUsuario == usuario.IdUsuario)
                .Select(f => f.IdInstitucion)
                .ToListAsync();

            return Ok(ids);
        }
    }
}
