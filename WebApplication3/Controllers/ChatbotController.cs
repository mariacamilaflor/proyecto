using EduDirectory3.Data;
using EduDirectory3.Models;
using EduDirectory3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace TuProyecto.Controllers
{
    [Authorize]
    public class ChatbotController : Controller
    {
        private readonly IaService _ia;
        private readonly NlpService _nlp;
        private readonly ApplicationDbContext _db;     


        public ChatbotController(IaService ia, NlpService nlp, ApplicationDbContext db)
        {
            _ia = ia;
            _nlp = nlp;
            _db = db;
        }

        // GET /Chatbot
        public IActionResult Index() => View();

        // POST /Chatbot/Chat
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
        {
            var texto = req.Mensaje.ToLower().Trim();

            // Respuestas rápidas
            if (EsSaludo(texto))
                return Json(new { respuesta = "¡Hola! 😊 ¿Qué institución educativa estás buscando?" });
            if (texto.Contains("gracias"))
                return Json(new { respuesta = "Con gusto 😊 ¿Necesitas algo más?" });
            if (texto.Contains("adios") || texto.Contains("chao"))
                return Json(new { respuesta = "¡Hasta luego! 😊" });

            // Obtener intención

            var intencion = await _ia.ObtenerIntencionAsync(req.Mensaje);

            // ✅ Si la IA rechazó o no entendió, responde directo
            if (intencion == null || !intencion.Any())
                return Json(new { respuesta = "Solo puedo ayudarte a encontrar instituciones educativas en Medellín. 😊 ¿Qué colegio estás buscando?" });
            // Guardar en BD
            var chatbot = new Chatbot
            {
                Fecha = DateOnly.FromDateTime(DateTime.Today),
                Mensaje = req.Mensaje,
                IdUsuario = req.IdUsuario
            };
            _db.Chatbot.Add(chatbot);
            await _db.SaveChangesAsync();

            _db.Filtro.Add(new Filtro
            {
                NombreFiltro = _nlp.ObtenerJsonFiltros(intencion),
                IdChatbot = chatbot.IdChatbot
            });
            await _db.SaveChangesAsync();

            // Consultar colegios
            var condiciones = _nlp.IntencionACondiciones(intencion);
            var sql = _nlp.ConstruirSql(condiciones);
            var resultados = await _db.InstitucionResultados
                                       .FromSqlRaw(sql)
                                       .ToListAsync();

            if (!resultados.Any())
                return Json(new { respuesta = "No encontré instituciones con esos criterios. ¿Quieres intentar con otro barrio o tipo?" });

            // ✅ Construir contexto con los datos reales
            var contextoColegios = string.Join("\n", resultados.Take(5).Select(r =>
                $"- {r.Nombre} ({r.Tipo}) | Sede: {r.NombreSede} | Dir: {r.Direccion} | " +
                $"Barrio: {r.Barrio}, {r.Comuna} | Tel: {r.NumeroTelefono} | " +
                $"Costo: ${r.Costo:N0} | Horario: {r.HorarioAtencion}"));

            // ✅ Armar historial con contexto para la IA
            var historialCompleto = new List<object>
            {
                new {
                    role = "system",
                    content = $"""
                        Eres un asistente amable que ayuda a familias a encontrar colegios en Bello.
                        Responde de forma natural y conversacional, máximo 5 líneas.
                        Usa emojis con moderación. Si el usuario pregunta por una sede específica, 
                        da solo esa información.
                
                        Datos encontrados en la base de datos:
                        {contextoColegios}
                    """
                }
            };
            if (EsContenidoInapropiado(texto))
                return Json(new { respuesta = "Lo siento, solo puedo ayudarte a encontrar instituciones educativas en Medellín. 😊 ¿Qué colegio estás buscando?" });
            // Agregar historial previo de la conversación
            if (req.Historial != null)
                historialCompleto.AddRange(req.Historial.Select(h => (object)new
                {
                    role = h.Role,
                    content = h.Content
                }));

            historialCompleto.Add(new { role = "user", content = req.Mensaje });

            string respuesta;
            try
            {
                respuesta = await _ia.ObtenerRespuestaAsync(historialCompleto, "");
            }
            catch (Exception)
            {
                respuesta = "No pude procesar esa consulta. ¿Puedes reformularla? 😊";
            }

            return Json(new { respuesta, total = resultados.Count });

            
        }
        private bool EsContenidoInapropiado(string texto)
        {
            var palabrasClave = new[]
            {
                "cortar", "suicidar", "morir", "matar", "sangre",
                "droga", "arma", "explotar", "bomba", "hack"
            };

            return palabrasClave.Any(p => texto.Contains(p));
        }
        private bool EsSaludo(string texto)
        {
            var saludos = new[]
            {
                "hola",
                "buenas",
                "hey",
                "holi",
                "qué tal",
                "buen día",
                "buenos dias",
                "buenas tardes",
                "buenas noches"
            };

            return saludos.Any(s =>
                texto.Contains(s));
        }

    }

    public class ChatRequest
    {
        public string Mensaje { get; set; } = "";
        public int IdUsuario { get; set; }
        public List<MensajeHistorial>? Historial { get; set; }
    }
    public class MensajeHistorial
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
