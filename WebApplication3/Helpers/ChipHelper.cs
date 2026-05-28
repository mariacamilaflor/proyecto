using EduDirectory3.ViewModels;

// Helpers/ChipHelper.cs
public static class ChipHelper
{
    public static Dictionary<string, string> SinFiltro(BusquedaFiltrosViewModel m, string campo)
    {
        var d = ToDictionary(m);
        d[campo] = "";
        return d;
    }

    public static Dictionary<string, string> SinNivel(BusquedaFiltrosViewModel m, string nivel)
    {
        var d = ToDictionary(m);
        var niveles = m.NivelesSeleccionados.Where(n => n != nivel).ToList();
        // los checkboxes van como índices: NivelesSeleccionados[0], [1]...
        for (int i = 0; i < niveles.Count; i++)
            d[$"NivelesSeleccionados[{i}]"] = niveles[i];
        return d;
    }

    public static Dictionary<string, string> SinJornada(BusquedaFiltrosViewModel m, string jornada)
    {
        var d = ToDictionary(m);
        var jornadas = m.JornadasSeleccionadas.Where(j => j != jornada).ToList();
        for (int i = 0; i < jornadas.Count; i++)
            d[$"JornadasSeleccionadas[{i}]"] = jornadas[i];
        return d;
    }

    private static Dictionary<string, string> ToDictionary(BusquedaFiltrosViewModel m)
    {
        return new Dictionary<string, string>
        {
            ["Nombre"] = m.Nombre ?? "",
            ["Tipo"] = m.Tipo ?? "",
            ["Metodologia"] = m.Metodologia ?? "",
            ["Calendario"] = m.Calendario ?? "",
            ["Comuna"] = m.Comuna ?? "",
            ["CostoMax"] = m.CostoMax.HasValue ? m.CostoMax.Value.ToString() : "",
        };
    }
}