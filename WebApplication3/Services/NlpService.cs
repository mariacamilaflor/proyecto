using System.Text.Json;

namespace EduDirectory3.Services
{
    public class NlpService
    {
        public List<string> IntencionACondiciones(Dictionary<string, JsonElement> intencion)
        {
            var condiciones = new List<string>();

            // Barrio y Comuna están en tabla Ubicacion
            if (intencion.TryGetValue("barrio", out var barrio))
                condiciones.Add($"u.Barrio = '{barrio.GetString()}'");

            if (intencion.TryGetValue("comuna", out var comuna))
                condiciones.Add($"u.Comuna = '{comuna.GetString()}'");

            // Tipo está en Institucion (Oficial / Privado)
            if (intencion.TryGetValue("tipo_institucion", out var tipo))
                condiciones.Add($"i.Tipo = '{tipo.GetString()}'");

            // Metodologia está en Institucion
            if (intencion.TryGetValue("metodologia", out var met))
                condiciones.Add($"i.Metodologia = '{met.GetString()}'");

            // Calendario está en Institucion (A / B)
            if (intencion.TryGetValue("calendario", out var cal))
                condiciones.Add($"i.Calendario = '{cal.GetString()}'");

            // Costo máximo
            if (intencion.TryGetValue("costo_maximo", out var costo))
                condiciones.Add($"i.Costo <= {costo.GetDecimal()}");

            // Niveles → tabla Nivel (NombreNivel)
            if (intencion.TryGetValue("niveles", out var niveles))
                foreach (var n in niveles.EnumerateArray())
                    condiciones.Add(
                        $"EXISTS (SELECT 1 FROM Nivel nv WHERE nv.IdSede = s.IdSede AND nv.NombreNivel = '{n.GetString()}')"
                    );

            // Jornadas → tabla Jornada (NombreJornada)
            if (intencion.TryGetValue("jornadas", out var jornadas))
                foreach (var j in jornadas.EnumerateArray())
                    condiciones.Add(
                        $"EXISTS (SELECT 1 FROM Jornada jo WHERE jo.IdSede = s.IdSede AND jo.NombreJornada = '{j.GetString()}')"
                    );

            // Servicios → tabla Servicio (NombreServicio)
            if (intencion.TryGetValue("servicios", out var servicios))
                foreach (var sv in servicios.EnumerateArray())
                    condiciones.Add(
                        $"EXISTS (SELECT 1 FROM Servicio sr WHERE sr.IdSede = s.IdSede AND sr.NombreServicio = '{sv.GetString()}')"
                    );

            // Actividades extracurriculares → tabla ActividadesExtracurriculares
            if (intencion.TryGetValue("actividades", out var acts))
                foreach (var a in acts.EnumerateArray())
                    condiciones.Add(
                        $"EXISTS (SELECT 1 FROM ActividadesExtracurriculares ae WHERE ae.IdSede = s.IdSede AND ae.NombreActividad = '{a.GetString()}')"
                    );

            return condiciones;
        }

        public string ConstruirSql(List<string> condiciones)
        {
            // JOIN entre todas las tablas relevantes
            var sql = @"
                SELECT DISTINCT
                    i.IdInstitucion,
                    i.Nombre,
                    i.Tipo,
                    i.Metodologia,
                    i.Calendario,
                    i.Costo,
                    i.PaginaWeb,
                    i.HorarioAtencion,
                    s.NombreSede,
                    u.Direccion,
                    u.Barrio,
                    u.Comuna,
                    t.NumeroTelefono
                FROM Institucion i
                INNER JOIN Sede      s ON s.IdInstitucion = i.IdInstitucion
                INNER JOIN Ubicacion u ON u.IdSede        = s.IdSede
                LEFT  JOIN Telefono  t ON t.IdSede        = s.IdSede";

            if (condiciones.Any())
                sql += " WHERE " + string.Join(" AND ", condiciones);

            return sql;
        }

        public string ObtenerJsonFiltros(Dictionary<string, JsonElement> intencion)
            => JsonSerializer.Serialize(intencion);
    }
}