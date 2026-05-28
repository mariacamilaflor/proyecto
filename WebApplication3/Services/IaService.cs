using System.Text;
using System.Text.Json;

namespace EduDirectory3.Services
{
    public class IaService
    {
        private const string Model = "nvidia/nemotron-3-nano-omni-30b-a3b-reasoning:free";
        private readonly HttpClient _http;
        private readonly string _ApiKey;

        public IaService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _ApiKey = config["ApiKeys:OpenRouter"]!;
        }

        public async Task<Dictionary<string, JsonElement>> ObtenerIntencionAsync(string pregunta)
        {
            var body = new
            {
                model = Model,
                max_tokens = 500,
                temperature = 0,
                messages = new[]
                {
                    new {
                        role = "system",
                        content = """
                            Eres un asistente que interpreta preguntas sobre instituciones educativas.
                            Devuelve SOLO un JSON con los campos que detectes.
                            NO escribas texto adicional. NO escribas SQL.

                            Campos permitidos:
                            - barrio               (string)
                            - comuna               (string)
                            - tipo_institucion     (Oficial | Privado)
                            - metodologia          (string, ej: Tradicional, Montessori)
                            - calendario           (A | B)
                            - costo_maximo         (número)
                            - niveles              (array: Preescolar, Primaria, Secundaria, Media)
                            - jornadas             (array: Mañana, Tarde, Nocturna, Completa, Fin de semana)
                            - servicios            (array: Restaurante, Transporte, Atención especial)
                            - actividades          (array, ej: Fútbol, Teatro, Robótica)
                        """
                    },
                    new { role = "user", content = pregunta }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_ApiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonSerializer.Deserialize<JsonElement>(json);
         
            var contenido = doc
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;

            // ✅ Si el modelo rechazó la petición, devuelve JSON vacío
            if (!contenido.TrimStart().StartsWith("{"))
                return new Dictionary<string, JsonElement>();

            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(contenido)!;
        }
        public async Task<string> ObtenerRespuestaAsync(List<object> historial,string sistemaPrompt)
        {
            var body = new
            {
                model = Model,
                max_tokens = 800,
                temperature = 0.4,
                messages = historial
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_ApiKey}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonSerializer.Deserialize<JsonElement>(json);

            return doc
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
    }
}
