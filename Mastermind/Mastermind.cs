namespace Mastermind;

public static class Mastermind
{
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
        if (patronSecreto is null) throw new ArgumentNullException(nameof(patronSecreto));
        if (intentoJugador is null) throw new ArgumentNullException(nameof(intentoJugador));

        var secreto = patronSecreto.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var intento = intentoJugador.Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (secreto.Length != intento.Length)
            throw new ArgumentException("El patrón secreto y el intento deben tener la misma cantidad de colores.");

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

        return (bienPosicionados, colorCorrecto);
    }
}

