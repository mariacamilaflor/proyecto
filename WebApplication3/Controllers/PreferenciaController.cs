using EduDirectory3.Data;
using EduDirectory3.Models;
using EduDirectory3.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduDirectory3.Controllers
{
    public class PreferenciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PreferenciaController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View(new FormularioPreferenciaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FormularioPreferenciaViewModel vm)
        {
            int idUsuario = 1;
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == email);
                if (usuario != null) idUsuario = usuario.IdUsuario;
            }

            var formulario = new FormularioPreferencia
            {
                Presupuesto = vm.Presupuesto,
                EdadEstudiante = vm.EdadEstudiante,
                TipoPreferencia = vm.TipoPreferencia,
                MetodologiaPreferencia = vm.MetodologiaPreferencia,
                IdUsuario = idUsuario
            };

            _context.FormularioPreferencia.Add(formulario);
            await _context.SaveChangesAsync();

            foreach (var s in vm.ServiciosPreferencia ?? new List<string>())
                _context.ServiciosPreferencia.Add(new ServiciosPreferencia
                {
                    NombreServiciosPreferencia = s,
                    IdFormularioPreferencia = formulario.IdFormularioPreferencia
                });

            foreach (var j in vm.JornadasPreferencia ?? new List<string>())
                _context.JornadaPreferencia.Add(new JornadaPreferencia
                {
                    NombreJornadaPreferencia = j,
                    IdFormularioPreferencia = formulario.IdFormularioPreferencia
                });

            foreach (var n in vm.NivelesPreferencia ?? new List<string>())
                _context.NivelPreferencia.Add(new NivelPreferencia
                {
                    NombreNivelPreferencia = n,
                    IdFormularioPreferencia = formulario.IdFormularioPreferencia
                });

            foreach (var a in vm.ActividadesPreferencia ?? new List<string>())
                _context.ActividadExtracurricularPreferencia.Add(new ActividadExtracurricularPreferencia
                {
                    NombreActividadExtracurricularPreferencia = a,
                    IdFormularioPreferencia = formulario.IdFormularioPreferencia
                });

            await _context.SaveChangesAsync();

            TempData["IdFormulario"] = formulario.IdFormularioPreferencia;
            return RedirectToAction("Resultado");
        }

        // GET — Mostrar resultado (top 5)
        public async Task<IActionResult> Resultado()
        {
            var idFormulario = TempData["IdFormulario"] as int?;
            if (idFormulario == null) return RedirectToAction("Index");

            var preferencia = await _context.FormularioPreferencia
                .Include(f => f.ServiciosPreferencia)
                .Include(f => f.JornadaPreferencia)
                .Include(f => f.NivelPreferencia)
                .Include(f => f.ActividadExtracurricularPreferencia)
                .FirstOrDefaultAsync(f => f.IdFormularioPreferencia == idFormulario);

            if (preferencia == null) return RedirectToAction("Index");

            // Cargar instituciones con sus sedes y atributos
            var instituciones = await _context.Institucion
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Servicio)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Jornada)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.Nivel)
                .Include(i => i.Sede)
                    .ThenInclude(s => s.ActividadesExtracurriculares)
                .ToListAsync();

            // Calcular score
            var serviciosPref = preferencia.ServiciosPreferencia.Select(s => s.NombreServiciosPreferencia).ToList();
            var jornadasPref = preferencia.JornadaPreferencia.Select(j => j.NombreJornadaPreferencia).ToList();
            var nivelesPref = preferencia.NivelPreferencia.Select(n => n.NombreNivelPreferencia).ToList();
            var actividadesPref = preferencia.ActividadExtracurricularPreferencia.Select(a => a.NombreActividadExtracurricularPreferencia).ToList();

            var resultados = instituciones.Select(inst =>
            {
                int score = 0;

                // Tipo — 25 pts
                if (inst.Tipo == preferencia.TipoPreferencia) score += 25;

                // Presupuesto — 25 pts
                if (inst.Costo <= preferencia.Presupuesto) score += 25;

                // Metodología — 20 pts
                if (inst.Metodologia == preferencia.MetodologiaPreferencia) score += 20;

                var serviciosInst = inst.Sede.SelectMany(s => s.Servicio).Select(s => s.NombreServicio).Distinct().ToList();
                var jornadasInst = inst.Sede.SelectMany(s => s.Jornada).Select(j => j.NombreJornada).Distinct().ToList();
                var nivelesInst = inst.Sede.SelectMany(s => s.Nivel).Select(n => n.NombreNivel).Distinct().ToList();
                var actividadesInst = inst.Sede.SelectMany(s => s.ActividadesExtracurriculares).Select(a => a.NombreActividad).Distinct().ToList();

                // Servicios — 10 pts
                if (serviciosPref.Any() && serviciosPref.Any(s => serviciosInst.Contains(s))) score += 10;

                // Jornadas — 10 pts
                if (jornadasPref.Any() && jornadasPref.Any(j => jornadasInst.Contains(j))) score += 10;

                // Niveles — 5 pts
                if (nivelesPref.Any() && nivelesPref.Any(n => nivelesInst.Contains(n))) score += 5;

                // Actividades — 5 pts
                if (actividadesPref.Any() && actividadesPref.Any(a => actividadesInst.Contains(a))) score += 5;

                
                return new
                {
                    Institucion = inst,
                    Score = score,
                    Porcentaje = score,
                    ScoreTipo = inst.Tipo == preferencia.TipoPreferencia ? 25 : 0,
                    ScorePresupuesto = inst.Costo <= preferencia.Presupuesto ? 25 : 0,
                    ScoreMetodologia = inst.Metodologia == preferencia.MetodologiaPreferencia ? 20 : 0,
                    ScoreServicios = serviciosPref.Any() && serviciosPref.Any(s => serviciosInst.Contains(s)) ? 10 : 0,
                    ScoreJornadas = jornadasPref.Any() && jornadasPref.Any(j => jornadasInst.Contains(j)) ? 10 : 0,
                    ScoreNiveles = nivelesPref.Any() && nivelesPref.Any(n => nivelesInst.Contains(n)) ? 5 : 0,
                    ScoreActividades = actividadesPref.Any() && actividadesPref.Any(a => actividadesInst.Contains(a)) ? 5 : 0
                };
            
            })
            .OrderByDescending(r => r.Score)
            .Take(5)
            .ToList();

            ViewBag.Resultados = resultados;
            return View();
        }
    }
}