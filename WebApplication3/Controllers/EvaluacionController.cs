using EduDirectory3.Data;
using EduDirectory3.Models;
using EduDirectory3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduDirectory3.Controllers
{
    [Authorize]
    public class EvaluacionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EvaluacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: carga el modal con las sedes
        [HttpGet]
        public IActionResult ModalResena(int idInstitucion)
        {
            var institucion = _context.Institucion
                .FirstOrDefault(i => i.IdInstitucion == idInstitucion);

            if (institucion == null)
                return NotFound();

            var sedes = _context.Sede
                .Where(s => s.IdInstitucion == idInstitucion)
                .Select(s => new SedeSelectItem
                {
                    IdSede = s.IdSede,
                    Nombre = s.NombreSede
                })
                .ToList();

            var vm = new ResenaViewModel
            {
                IdInstitucion = idInstitucion,
                NombreInstitucion = institucion.Nombre,
                Sedes = sedes
            };

            return PartialView("ModalResena", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarResena(ResenaViewModel vm)
        {
            try
            {
                // OBTENER CORREO DEL USUARIO LOGUEADO
                var correo = User.Identity.Name;

                // BUSCAR USUARIO EN TU TABLA
                var usuario = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Correo == correo);

                if (usuario == null)
                {
                    return Json(new
                    {
                        success = false,
                        mensaje = "Usuario no encontrado"
                    });
                }

                // CREAR EVALUACION
                var evaluacion = new Evaluacion
                {
                    Comentario = vm.Comentario,
                    IdUsuario = usuario.IdUsuario,
                    IdSede = vm.IdSede,
                    Reportado = false
                };

                _context.Evaluacion.Add(evaluacion);

                Console.WriteLine("ANTES SAVE EVALUACION");

                await _context.SaveChangesAsync();

                Console.WriteLine("DESPUES SAVE EVALUACION");

                // CREAR CALIFICACION
                var calificacion = new Calificacion
                {
                    IdEvaluacion = evaluacion.IdEvaluacion,

                    CalificacionInfraestructura = vm.CalificacionInfraestructura,
                    CalificacionProfesores = vm.CalificacionProfesores,
                    CalificacionMetodologia = vm.CalificacionMetodologia,
                    CalificacionAmbienteEscolar = vm.CalificacionAmbienteEscolar,
                    CalificacionSeguridad = vm.CalificacionSeguridad,
                    CalificacionActividadesExtracurriculares = vm.CalificacionActividadesExtracurriculares
                };

                _context.Calificacion.Add(calificacion);

                Console.WriteLine("ANTES SAVE CALIFICACION");

                await _context.SaveChangesAsync();

                Console.WriteLine("DESPUES SAVE CALIFICACION");

                return Json(new
                {
                    success = true,
                    mensaje = "Guardado"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:");

                Console.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }

                return Json(new
                {
                    success = false,
                    mensaje = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Reportar([FromBody] Reporte dto)
        {
            var evaluacion = await _context.Evaluacion.FindAsync(dto.IdEvaluacion);
            if (evaluacion == null) return NotFound();

            evaluacion.Reportado = true;
            evaluacion.MotivoReporte = dto.MotivoReporte;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}