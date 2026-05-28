using EduDirectory3.Areas.Panel.ViewModels;
using EduDirectory3.Data;
using EduDirectory3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace EduDirectory3.Areas.Panel.Controllers
{
    [Area("Panel")]
    [Authorize(Roles = "Institucion")]
    public class SedesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SedesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: obtiene la institución del usuario logueado
        private async Task<Institucion?> GetInstitucionActualAsync()
        {
            var email = User.Identity?.Name;
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);
            if (usuario == null) return null;
            return await _context.Institucion
                .FirstOrDefaultAsync(i => i.IdUsuario == usuario.IdUsuario);
        }

        // ─────────────────────────────────────────────
        // LISTA DE SEDES
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null)
                return RedirectToAction("Crear", "Dashboard", new { area = "Panel" });

            var sedes = await _context.Sede
                .Include(s => s.Ubicacion)
                .Include(s => s.Telefono)
                .Include(s => s.Nivel)
                .Include(s => s.Jornada)
                .Include(s => s.Servicio)
                .Include(s => s.ActividadesExtracurriculares)
                .Where(s => s.IdInstitucion == institucion.IdInstitucion)
                .ToListAsync();

            ViewBag.Institucion = institucion;
            return View(sedes);
        }

        // ─────────────────────────────────────────────
        // CREAR SEDE
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Crear()
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null)
                return RedirectToAction("Crear", "Dashboard", new { area = "Panel" });

            ViewBag.Institucion = institucion;
            return View(new SedeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            string NombreSede,
            // Ubicación
            string Direccion, string Barrio, string Comuna,
            // Coordenada
            string? CoordenadaX, string? CoordenadaY,
            // Listas
            List<string>? Telefonos,
            List<string>? Servicios,
            List<string>? Niveles,
            List<string>? Jornadas,
            List<string>? Actividades)
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(NombreSede))
            {
                ModelState.AddModelError("", "El nombre de la sede es obligatorio.");
                ViewBag.Institucion = institucion;
                return View(new SedeViewModel());
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Crear sede
                var sede = new Sede
                {
                    NombreSede = NombreSede,
                    IdInstitucion = institucion.IdInstitucion
                };
                _context.Sede.Add(sede);
                await _context.SaveChangesAsync();

                // 2. Ubicación + Coordenada
                if (!string.IsNullOrWhiteSpace(Direccion))
                {
                    var ubicacion = new Ubicacion
                    {
                        Direccion = Direccion,
                        Barrio = Barrio ?? "",
                        Comuna = Comuna ?? "",
                        IdSede = sede.IdSede
                    };
                    _context.Ubicacion.Add(ubicacion);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(CoordenadaX) && !string.IsNullOrWhiteSpace(CoordenadaY))
                    {
                        if (decimal.TryParse(CoordenadaX, System.Globalization.NumberStyles.Any,
                         System.Globalization.CultureInfo.InvariantCulture, out decimal x) &&
                        decimal.TryParse(CoordenadaY, System.Globalization.NumberStyles.Any,
                         System.Globalization.CultureInfo.InvariantCulture, out decimal y))
                        {
                            var coord = new Coordenada
                            {
                                CoordenadaX = x,
                                CoordenadaY = y,
                                IdUbicacion = ubicacion.IdUbicacion
                            };
                            _context.Coordenada.Add(coord);
                        }
                    }
                }

                // 3. Teléfonos
                foreach (var tel in Telefonos?.Where(t => !string.IsNullOrWhiteSpace(t)) ?? [])
                    _context.Telefono.Add(new Telefono { NumeroTelefono = tel, IdSede = sede.IdSede });

                // 4. Servicios
                foreach (var srv in Servicios?.Where(s => !string.IsNullOrWhiteSpace(s)) ?? [])
                    _context.Servicio.Add(new Servicio { NombreServicio = srv, IdSede = sede.IdSede });

                // 5. Niveles
                foreach (var niv in Niveles?.Where(n => !string.IsNullOrWhiteSpace(n)) ?? [])
                    _context.Nivel.Add(new Nivel { NombreNivel = niv, IdSede = sede.IdSede });

                // 6. Jornadas
                foreach (var jor in Jornadas?.Where(j => !string.IsNullOrWhiteSpace(j)) ?? [])
                    _context.Jornada.Add(new Jornada { NombreJornada = jor, IdSede = sede.IdSede });

                // 7. Actividades extracurriculares
                foreach (var act in Actividades?.Where(a => !string.IsNullOrWhiteSpace(a)) ?? [])
                    _context.ActividadesExtracurriculares.Add(new ActividadesExtracurriculares { NombreActividad = act, IdSede = sede.IdSede });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Exito"] = $"Sede '{NombreSede}' creada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                ViewBag.Institucion = institucion;
                return View(new SedeViewModel());
            }
        }

        // ─────────────────────────────────────────────
        // DETALLE / EDITAR SEDE
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Detalle(int id)
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null) return Unauthorized();

            var sede = await _context.Sede
                .Include(s => s.Ubicacion).ThenInclude(u => u.Coordenada)
                .Include(s => s.Telefono)
                .Include(s => s.Servicio)
                .Include(s => s.Nivel)
                .Include(s => s.Jornada)
                .Include(s => s.ActividadesExtracurriculares)
                .FirstOrDefaultAsync(s => s.IdSede == id && s.IdInstitucion == institucion.IdInstitucion);

            if (sede == null) return NotFound();

            ViewBag.Institucion = institucion;
            return View(sede);
        }

        // ─────────────────────────────────────────────
        // ELIMINAR SEDE
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var institucion = await GetInstitucionActualAsync();
            if (institucion == null) return Unauthorized();

            var sede = await _context.Sede
                .FirstOrDefaultAsync(s => s.IdSede == id && s.IdInstitucion == institucion.IdInstitucion);

            if (sede == null) return NotFound();

            _context.Sede.Remove(sede);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Sede eliminada correctamente.";
            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────
        // AJAX: Agregar/Eliminar items individuales
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> GuardarUbicacion(int idSede, int? idUbicacion, string Direccion, string Barrio, string Comuna, string? CoordenadaX, string? CoordenadaY)
        {
            bool parseCoordenadas(out decimal x, out decimal y)
            {
                var cult = System.Globalization.CultureInfo.InvariantCulture;
                var style = System.Globalization.NumberStyles.Any;
                string coordX = CoordenadaX?.Replace(',', '.');
                string coordY = CoordenadaY?.Replace(',', '.');
                return decimal.TryParse(coordX, style, cult, out x) &
                       decimal.TryParse(coordY, style, cult, out y);
            }

            if (idUbicacion.HasValue)
            {
                var ub = await _context.Ubicacion.Include(u => u.Coordenada)
                                       .FirstOrDefaultAsync(u => u.IdUbicacion == idUbicacion);
                if (ub != null)
                {
                    ub.Direccion = Direccion; ub.Barrio = Barrio; ub.Comuna = Comuna;

                    if (parseCoordenadas(out decimal x, out decimal y))
                    {
                        var coord = ub.Coordenada.FirstOrDefault();
                        if (coord != null) { coord.CoordenadaX = x; coord.CoordenadaY = y; }
                        else _context.Coordenada.Add(new Coordenada { CoordenadaX = x, CoordenadaY = y, IdUbicacion = ub.IdUbicacion });
                    }
                }
            }
            else
            {
                var ub = new Ubicacion { Direccion = Direccion, Barrio = Barrio, Comuna = Comuna, IdSede = idSede };
                _context.Ubicacion.Add(ub);
                await _context.SaveChangesAsync();

                if (parseCoordenadas(out decimal x, out decimal y))
                    _context.Coordenada.Add(new Coordenada { CoordenadaX = x, CoordenadaY = y, IdUbicacion = ub.IdUbicacion });
            }

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Ubicación guardada.";
            return RedirectToAction("Detalle", new { id = idSede });
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Geocodificar(string direccion)
        {
            var apiKey = "AIzaSyBgnaroLzBZd8TvJKI_J-xvyDsDV1hp2jQ";
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(direccion + ", Bello, Colombia")}&key={apiKey}";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["status"]?.ToString() == "OK")
            {
                var loc = json["results"]![0]!["geometry"]!["location"]!;
                return Json(new
                {
                    success = true,
                    lat = (double)loc["lat"]!,
                    lng = (double)loc["lng"]!
                });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> AgregarTelefono(int idSede, string numero)
        {
            var tel = new Telefono { NumeroTelefono = numero, IdSede = idSede };
            _context.Telefono.Add(tel);
            await _context.SaveChangesAsync();
            return Json(new { id = tel.IdTelefono });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarTelefono(int id)
        {
            var tel = await _context.Telefono.FindAsync(id);
            if (tel == null) return NotFound();
            _context.Telefono.Remove(tel);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarServicio(int idSede, string nombre)
        {
            var srv = new Servicio { NombreServicio = nombre, IdSede = idSede };
            _context.Servicio.Add(srv);
            await _context.SaveChangesAsync();
            return Json(new { id = srv.IdServicio });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarServicio(int id)
        {
            var srv = await _context.Servicio.FindAsync(id);
            if (srv == null) return NotFound();
            _context.Servicio.Remove(srv);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarNivel(int idSede, string nombre)
        {
            var niv = new Nivel { NombreNivel = nombre, IdSede = idSede };
            _context.Nivel.Add(niv);
            await _context.SaveChangesAsync();
            return Json(new { id = niv.IdNivel });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarNivel(int id)
        {
            var niv = await _context.Nivel.FindAsync(id);
            if (niv == null) return NotFound();
            _context.Nivel.Remove(niv);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarJornada(int idSede, string nombre)
        {
            var jor = new Jornada { NombreJornada = nombre, IdSede = idSede };
            _context.Jornada.Add(jor);
            await _context.SaveChangesAsync();
            return Json(new { id = jor.IdJornada });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarJornada(int id)
        {
            var jor = await _context.Jornada.FindAsync(id);
            if (jor == null) return NotFound();
            _context.Jornada.Remove(jor);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarActividad(int idSede, string nombre)
        {
            var act = new ActividadesExtracurriculares { NombreActividad = nombre, IdSede = idSede };
            _context.ActividadesExtracurriculares.Add(act);
            await _context.SaveChangesAsync();
            return Json(new { id = act.IdActividadesExtracurriculares });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarActividad(int id)
        {
            var act = await _context.ActividadesExtracurriculares.FindAsync(id);
            if (act == null) return NotFound();
            _context.ActividadesExtracurriculares.Remove(act);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────────────────
        // RECTOR (nivel institución)
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarRector(string Nombre, string Apellido, string Telefono, string Email)
        {
            var inst = await GetInstitucionActualAsync();
            if (inst == null) return Unauthorized();

            _context.Rector.Add(new Rector
            {
                Nombre = Nombre,
                Apellido = Apellido,
                Telefono = Telefono,
                Email = Email,
                IdInstitucion = inst.IdInstitucion
            });
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Rector agregado.";
            return RedirectToAction("InfoInstitucion");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRector(int id)
        {
            var rect = await _context.Rector.FindAsync(id);
            if (rect != null) { _context.Rector.Remove(rect); await _context.SaveChangesAsync(); }
            return RedirectToAction("InfoInstitucion");
        }

        // ─────────────────────────────────────────────
        // RESULTADOS ICFES (nivel institución)
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarIcfes(decimal Puntaje, int Anio)
        {
            var inst = await GetInstitucionActualAsync();
            if (inst == null) return Unauthorized();

            _context.ResultadosIcfes.Add(new ResultadosIcfes
            {
                Puntaje = Puntaje,
                Anio = Anio,
                IdInstitucion = inst.IdInstitucion
            });
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Resultado ICFES agregado.";
            return RedirectToAction("InfoInstitucion");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarIcfes(int id)
        {
            var icfes = await _context.ResultadosIcfes.FindAsync(id);
            if (icfes != null) { _context.ResultadosIcfes.Remove(icfes); await _context.SaveChangesAsync(); }
            return RedirectToAction("InfoInstitucion");
        }

        // ─────────────────────────────────────────────
        // EMAILS (nivel institución)
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarEmail(string Email1, string PerteneceA)
        {
            var inst = await GetInstitucionActualAsync();
            if (inst == null) return Unauthorized();

            _context.Email.Add(new Email
            {
                Email1 = Email1,
                PerteneceA = PerteneceA,
                IdInstitucion = inst.IdInstitucion
            });
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Email agregado.";
            return RedirectToAction("InfoInstitucion");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEmail(int id)
        {
            var email = await _context.Email.FindAsync(id);
            if (email != null) { _context.Email.Remove(email); await _context.SaveChangesAsync(); }
            return RedirectToAction("InfoInstitucion");
        }

        // ─────────────────────────────────────────────
        // INFO INSTITUCIÓN (Rector, ICFES, Emails)
        // ─────────────────────────────────────────────
        public async Task<IActionResult> InfoInstitucion()
        {
            var inst = await _context.Institucion
                .Include(i => i.Rector)
                .Include(i => i.ResultadosIcfes)
                .Include(i => i.Email)
                .FirstOrDefaultAsync(i => i.IdUsuario == (
                    _context.Usuario.Where(u => u.Correo == User.Identity!.Name)
                                    .Select(u => u.IdUsuario).FirstOrDefault()
                ));

            if (inst == null) return RedirectToAction("Crear", "Dashboard", new { area = "Panel" });

            ViewBag.Institucion = inst;
            return View(new InstitucionDetalleViewModel
            {
                Institucion = inst,
                Rectores = inst.Rector.ToList(),
                ResultadosIcfes = inst.ResultadosIcfes.ToList(),
                Emails = inst.Email.ToList()
            });
        }
    }
}

