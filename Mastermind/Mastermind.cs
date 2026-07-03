using System.Text.Json;

namespace Mastermind;

public static class Mastermind
{
    private static readonly object CheatSessionLock = new();
    private static bool CheatSessionLoaded;
    private static Dictionary<string, int> CheatRemainingUses = new(StringComparer.OrdinalIgnoreCase);

    private static readonly string[] DefaultValidColors =
    {
        "ROJO", "VERDE", "AZUL", "AMARILLO", "CYAN", "NEGRO", "PURPURA", "BLANCO"
    };

    /// <summary>
    /// Evalúa un intento contra un patrón secreto.
    /// </summary>
    /// <param name="patronSecreto">Secuencia de colores del patrón secreto.</param>
    /// <param name="intentoJugador">Secuencia de colores del intento del jugador.</param>
    /// <returns>(bienPosicionados, colorCorrecto)</returns>
    public static (int bienPosicionados, int colorCorrecto) EvaluarIntento(
        string patronSecreto,
        string intentoJugador)
    {
        string _;
        return EvaluarIntento(patronSecreto, intentoJugador, out _, cheatCode: null, validColors: null);
    }

    /// <summary>
    /// Evalúa un intento contra un patrón secreto.
    /// </summary>
    /// <param name="patronSecreto">Secuencia de colores del patrón secreto.</param>
    /// <param name="intentoJugador">Secuencia de colores del intento del jugador.</param>
    /// <param name="log">Mensaje de log con el formato requerido.</param>
    /// <param name="cheatCode">Código opcional para revelar una posición (solo afecta al log).</param>
    /// <param name="validColors">Colores válidos opcionales (si se provee, reemplaza la lista por defecto).</param>
    /// <returns>(bienPosicionados, colorCorrecto)</returns>
    public static (int bienPosicionados, int colorCorrecto) EvaluarIntento(
        string patronSecreto,
        string intentoJugador,
        out string log,
        string? cheatCode = null,
        string[]? validColors = null)
    {
        if (patronSecreto is null) throw new ArgumentNullException(nameof(patronSecreto));
        if (intentoJugador is null) throw new ArgumentNullException(nameof(intentoJugador));

        var allowedColors = (validColors ?? DefaultValidColors).Where(c => c is not null).Select(c => c!).ToArray();
        if (allowedColors.Length == 0)
            throw new ArgumentException("validColors no puede estar vacío.", nameof(validColors));

        var secreto = patronSecreto.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var intento = intentoJugador.Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (secreto.Length != intento.Length)
            throw new ArgumentException("El patrón secreto y el intento deben tener la misma cantidad de colores.");

        if (secreto.Length != 4 && secreto.Length != 6)
            throw new ArgumentException("El patrón secreto y el intento deben tener 4 o 6 colores.");

        var allowedSet = new HashSet<string>(allowedColors, StringComparer.OrdinalIgnoreCase);

        foreach (var c in secreto)
        {
            if (!allowedSet.Contains(c)) throw new ArgumentException($"Color inválido: {c}");
        }

        foreach (var c in intento)
        {
            if (!allowedSet.Contains(c)) throw new ArgumentException($"Color inválido: {c}");
        }

        int bienPosicionados = 0;

        // Evitar doble conteo: primero contamos aciertos por posición.
        // Luego contamos intersección multiconjunto de colores en posiciones incorrectas.
        var secretoCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var intentoCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < secreto.Length; i++)
        {
            if (string.Equals(secreto[i], intento[i], StringComparison.OrdinalIgnoreCase))
            {
                bienPosicionados++;
                continue;
            }

            if (secretoCounts.TryGetValue(secreto[i], out int sCount)) secretoCounts[secreto[i]] = sCount + 1;
            else secretoCounts[secreto[i]] = 1;

            if (intentoCounts.TryGetValue(intento[i], out int jCount)) intentoCounts[intento[i]] = jCount + 1;
            else intentoCounts[intento[i]] = 1;
        }

        int colorCorrecto = 0;
        foreach (var (color, sCount) in secretoCounts)
        {
            if (intentoCounts.TryGetValue(color, out int jCount))
            {
                colorCorrecto += Math.Min(sCount, jCount);
            }
        }

        var now = DateTime.Now;
        var baseLog = $"En la fecha {now:dd-MM-yyyy HH:mm:ss} se evaluó el intento {intentoJugador} contra el patrón {patronSecreto}. Resultado: {bienPosicionados} bien posicionados con {colorCorrecto} color correcto.";

        string cheatLogSuffix = "";
        if (!string.IsNullOrWhiteSpace(cheatCode))
        {
            cheatLogSuffix = TryApplyCheatToLog(cheatCode, secreto, intento);
        }

        log = baseLog + cheatLogSuffix;
        return (bienPosicionados, colorCorrecto);
    }

    private static string TryApplyCheatToLog(string cheatCode, string[] secreto, string[] intento)
    {
        EnsureCheatSessionLoaded();

        int remaining;
        lock (CheatSessionLock)
        {
            if (!CheatRemainingUses.TryGetValue(cheatCode, out remaining) || remaining <= 0)
                return "";

            // El cheat se consume siempre cuando se usa (cuando existe con usos restantes).
            CheatRemainingUses[cheatCode] = remaining - 1;
        }

        var incorrectIndices = new List<int>();
        for (int i = 0; i < secreto.Length; i++)
        {
            if (!string.Equals(secreto[i], intento[i], StringComparison.OrdinalIgnoreCase))
                incorrectIndices.Add(i);
        }

        if (incorrectIndices.Count == 0)
            return " Revelación: sin suerte.";

        var rnd = Random.Shared;
        int chosen = incorrectIndices[rnd.Next(incorrectIndices.Count)];
        return $" Revelación: posición {chosen + 1}, color {secreto[chosen]}.";
    }

    private static void EnsureCheatSessionLoaded()
    {
        if (CheatSessionLoaded) return;

        lock (CheatSessionLock)
        {
            if (CheatSessionLoaded) return;

            var cheatPath = Path.Combine(AppContext.BaseDirectory, "cheatcodes.json");
            if (!File.Exists(cheatPath))
            {
                // Si no existe el archivo, no hay cheats disponibles.
                CheatRemainingUses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                CheatSessionLoaded = true;
                return;
            }

            var json = File.ReadAllText(cheatPath);
            var items = JsonSerializer.Deserialize<List<CheatItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CheatItem>();

            CheatRemainingUses = items
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .ToDictionary(x => x.Code!, x => Math.Max(0, x.Usos), StringComparer.OrdinalIgnoreCase);

            CheatSessionLoaded = true;
        }
    }

    private sealed class CheatItem
    {
        public string? Code { get; set; }
        public int Usos { get; set; }
        // Mapeo desde la estructura requerida: [{'code': 'XYZABC', 'usos': 2}]
        public string? code { get => Code; set => Code = value; }
        public int usos { get => Usos; set => Usos = value; }
    }
}



