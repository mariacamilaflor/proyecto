using EduDirectory3.Data;
using EduDirectory3.Models;
using EduDirectory3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduDirectory3.Controllers
{
    public class ColegiosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ColegiosController(ApplicationDbContext context)
            => _context = context;

        // ── GET /Colegios ─────────────────────────────
        public async Task<IActionResult> Index(BusquedaFiltrosViewModel vm)
        {
            // Cargar listas para los filtros
            vm.Niveles = await _context.Nivel
                .Select(n => new SelectListItem
                {
                    Value = n.IdNivel.ToString(),
                    Text = n.NombreNivel
                })
                .ToListAsync();

            vm.Servicios = await _context.Servicio
                .Select(se => new SelectListItem
                {
                    Value = se.IdServicio.ToString(),
                    Text = se.NombreServicio
                })
                .ToListAsync();

            vm.Jornadas = await _context.Jornada
                .Select(j => new SelectListItem
                {
                    Value = j.IdJornada.ToString(),
                    Text = j.NombreJornada
                })
                .ToListAsync();

            vm.Comunas = await _context.Ubicacion
                .Where(u => u.Comuna != null)
                .Select(u => u.Comuna!)
                .Distinct().OrderBy(c => c)
                .ToListAsync();

            // Construir consulta base
            var query = _context.Institucion
                    .Include(i => i.Sede)
                        .ThenInclude(s => s.Nivel)
                    .Include(i => i.Sede)
                        .ThenInclude(s => s.Jornada)
                    .Include(i => i.Sede)
                        .ThenInclude(s => s.Servicio)
                    .Include(i => i.Sede)
                        .ThenInclude(s => s.Ubicacion)
                    .Include(i => i.Sede)
                        .ThenInclude(s => s.Evaluacion)
                            .ThenInclude(e => e.Calificacion)
                    .Include(i => i.ResultadosIcfes)
                    .Include(i => i.Rector)
                    .Include(i => i.IdUsuarioNavigation)
                .AsSplitQuery()
                .AsQueryable();

            // Aplicar filtros opcionales
            if (!string.IsNullOrEmpty(vm.Nombre))
                query = query.Where(i =>
                    i.Nombre.Contains(vm.Nombre));

            if (!string.IsNullOrEmpty(vm.Tipo))
                query = query.Where(i => i.Tipo == vm.Tipo);

            if (!string.IsNullOrEmpty(vm.Metodologia))
                query = query.Where(i => i.Metodologia == vm.Metodologia);

            if (!string.IsNullOrEmpty(vm.Calendario))
                query = query.Where(i => i.Calendario == vm.Calendario);

            if (vm.CostoMax.HasValue)
                query = query.Where(i =>
                    i.Costo == null || i.Costo <= vm.CostoMax);

            if (vm.NivelesSeleccionados.Any())
                query = query.Where(i => i.Sede
                    .Any(s => s.Nivel
                        .Any(n => vm.NivelesSeleccionados
                            .Contains(n.IdNivel))));

            if (vm.JornadasSeleccionadas.Any())
                query = query.Where(i => i.Sede
                    .Any(s => s.Jornada
                        .Any(j => vm.JornadasSeleccionadas
                            .Contains(j.IdJornada))));

            if (vm.ServiciosSeleccionados.Any())
                query = query.Where(i => i.Sede
                    .Any(s => s.Servicio
                        .Any(se => vm.ServiciosSeleccionados
                            .Contains(se.IdServicio))));

            if (!string.IsNullOrEmpty(vm.Comuna))
                query = query.Where(i => i.Sede
                    .Any(s => s.Ubicacion
                        .Any(u => u.Comuna == vm.Comuna)));

            // Total para paginación
            vm.TotalItems = await query.CountAsync();

            // Proyectar a ViewModel
            var proyectado = query.Select(i => new ColegioResultadoViewModel
            {
               
                    IdInstitucion = i.IdInstitucion,
                    Nombre = i.Nombre,
                    Imagen = i.Imagen,
                    Tipo = i.Tipo,
                    Metodologia = i.Metodologia,
                    Costo = i.Costo,
                    Descripcion = i.Descripcion,
                    Calendario = i.Calendario,
                    PaginaWeb = i.PaginaWeb,
                    HorarioAtencion = i.HorarioAtencion,
                
                Academico = new AcademicoViewModel
                {
                    Niveles = i.Sede.Select(s => s.Nivel.Select(a => a.NombreNivel).Distinct().ToList()).ToList(),
                    Jornadas = i.Sede.Select(s => s.Jornada.Select(a => a.NombreJornada).Distinct().ToList()).ToList(),
                    Servicios = i.Sede.Select(s => s.Servicio.Select(a => a.NombreServicio).Distinct().ToList()).ToList(),
                    Puntajes = i.ResultadosIcfes.Select(r => r.Puntaje).ToList(),
                    Anios = i.ResultadosIcfes.Select(r => r.Anio).ToList(),
                    ActividadesExtracurriculares = i.Sede.Select(s => s.ActividadesExtracurriculares.Select(a => a.NombreActividad).Distinct().ToList()).ToList()
                },
                Sedes = new SedesViewModel
                {
                    NombreSede = i.Sede.Select(s => s.NombreSede).ToList(),
                    Email = i.Email.Select(e => e.Email1).ToList(),
                    PerteneceA = i.Email.Select(e => e.PerteneceA).ToList(),
                    Telefono = i.Sede.Select(s => s.Telefono.Select(a => a.NumeroTelefono).Distinct().ToList()).ToList(),
                    Comuna = i.Sede.SelectMany(s => s.Ubicacion).Select(c => c.Comuna).FirstOrDefault(),
                    Barrio = i.Sede.SelectMany(s => s.Ubicacion).Select(c => c.Barrio).FirstOrDefault(),
                    Direccion = i.Sede.SelectMany(s => s.Ubicacion).Select(c => c.Direccion).FirstOrDefault()
                },
                Opinion = new OpinionViewModel
                {
                    Comentarios = i.Sede.SelectMany(s => s.Evaluacion).Select(e => e.Comentario).ToList(),
                    PromedioRating = i.Sede.SelectMany(s => s.Evaluacion).SelectMany(e => e.Calificacion).Any()
                    ? (double)i.Sede.SelectMany(s => s.Evaluacion).SelectMany(e => e.Calificacion)
                        .Average(e => (
                        e.CalificacionAmbienteEscolar +
                        e.CalificacionMetodologia +
                        e.CalificacionSeguridad +
                        e.CalificacionProfesores +
                        e.CalificacionActividadesExtracurriculares +
                        e.CalificacionInfraestructura
                    ) / 6.0m): 0,
                    Ambiente=i.Sede.SelectMany(s=>s.Evaluacion).SelectMany(e => e.Calificacion).Select(c => c.CalificacionAmbienteEscolar).ToList(),
                    Metodologia = i.Sede.SelectMany(s => s.Evaluacion).SelectMany(e => e.Calificacion).Select(c => c.CalificacionMetodologia).ToList(),
                    Seguridad = i.Sede.SelectMany(s => s.Evaluacion).SelectMany(e => e.Calificacion).Select(c => c.CalificacionSeguridad).ToList(),
                    Profesores = i.Sede.SelectMany(s => s.Evaluacion).SelectMany(e => e.Calificacion).Select(c => c.CalificacionProfesores).ToList(),
                    ActividadesEx = i.Sede.SelectMany(s => s.Evaluacion).SelectMany(e => e.Calificacion).Select(c => c.CalificacionActividadesExtracurriculares).ToList(),
                    Infraestructura = i.Sede.SelectMany(s => s.Evaluacion).SelectMany(e => e.Calificacion).Select(c => c.CalificacionInfraestructura).ToList(),
                    TotalReseñas = i.Sede.SelectMany(s => s.Evaluacion).Count(),
                    Nombres = i.Sede.SelectMany(s => s.Evaluacion).Select(e => e.Comentario).ToList(),
                    Apellidos = i.Sede.SelectMany(s => s.Evaluacion).Select(e => e.Comentario).ToList()
                },
                Contacto = new ContactoViewModel
                {
                    RectorNombre = i.Rector.Select(r => r.Nombre).FirstOrDefault(),
                    RectorApellido = i.Rector.Select(r => r.Apellido).FirstOrDefault(),
                    RectorTelefono = i.Rector.Select(r => r.Telefono).FirstOrDefault(),
                    RectorEmail = i.Rector.Select(r => r.Email).FirstOrDefault()
                }
            });

            // Ordenar
            proyectado = vm.OrdenarPor switch
            {
                "costo" => proyectado.OrderBy(c => c.Costo ?? 0),
                "nombre" => proyectado.OrderBy(c => c.Nombre),
                _ => proyectado.OrderByDescending(c => c.Opinion.PromedioRating)
            };

            // Paginar
            vm.Resultados = await proyectado
                .Skip((vm.Pagina - 1) * vm.TamañoPagina)
                .Take(vm.TamañoPagina)
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> Comparar(List<int> idInstituciones)
        {
            if (idInstituciones == null || !idInstituciones.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var instituciones = await _context.Institucion

                .Include(i => i.Sede)
                    .ThenInclude(s => s.Nivel)

                .Include(i => i.Sede)
                    .ThenInclude(s => s.Jornada)

                .Include(i => i.Sede)
                    .ThenInclude(s => s.Servicio)

                .Include(i => i.Sede)
                    .ThenInclude(s => s.Ubicacion)

                .Include(i => i.Sede)
                    .ThenInclude(s => s.Evaluacion)
                        .ThenInclude(e => e.Calificacion)

                .Include(i => i.ResultadosIcfes)

                .Where(i => idInstituciones.Contains(i.IdInstitucion))

                .Select(i => new ColegioResultadoViewModel
                {
                    IdInstitucion = i.IdInstitucion,
                    Nombre = i.Nombre,
                    Imagen = i.Imagen,
                    Tipo = i.Tipo,
                    Metodologia = i.Metodologia,
                    Calendario = i.Calendario,
                    Costo = i.Costo,

                    Academico = new AcademicoViewModel
                    {
                        Niveles = i.Sede.Select(s => s.Nivel.Select(a => a.NombreNivel).Distinct().ToList()).ToList(),
                        Jornadas = i.Sede.Select(s => s.Jornada.Select(a => a.NombreJornada).Distinct().ToList()).ToList(),
                        Servicios = i.Sede.Select(s => s.Servicio.Select(a => a.NombreServicio).Distinct().ToList()).ToList(),

                        Puntajes = i.ResultadosIcfes
                            .Select(r => r.Puntaje)
                            .ToList()
                    },

                    Opinion = new OpinionViewModel
                    {
                        PromedioRating =
                            i.Sede.SelectMany(s => s.Evaluacion)
                            .SelectMany(e => e.Calificacion)
                            .Any()

                            ? (double)i.Sede
                                .SelectMany(s => s.Evaluacion)
                                .SelectMany(e => e.Calificacion)
                                .Average(c =>
                                    (
                                        c.CalificacionAmbienteEscolar +
                                        c.CalificacionMetodologia +
                                        c.CalificacionSeguridad +
                                        c.CalificacionProfesores +
                                        c.CalificacionActividadesExtracurriculares +
                                        c.CalificacionInfraestructura
                                    ) / 6.0m)

                            : 0,

                        TotalReseñas = i.Sede
                            .SelectMany(s => s.Evaluacion)
                            .Count()
                    }
                })
                .ToListAsync();

            return View(instituciones);
        }
    }
}
