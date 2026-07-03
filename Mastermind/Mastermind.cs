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

        if (patronSecreto.Length != intentoJugador.Length)
            throw new ArgumentException("El patrón secreto y el intento deben tener la misma longitud.");

        int bienPosicionados = 0;

        // Para evitar doble conteo, primero sacamos las coincidencias por posición.
        // Luego contamos la intersección multiconjunto para las coincidencias de color en posiciones distintas.
        var secretoCounts = new Dictionary<char, int>();
        var jugadorCounts = new Dictionary<char, int>();

        for (int i = 0; i < patronSecreto.Length; i++)
        {
            char s = patronSecreto[i];
            char j = intentoJugador[i];

            if (s == j)
            {
                bienPosicionados++;
                continue;
            }

            if (secretoCounts.TryGetValue(s, out int sCount)) secretoCounts[s] = sCount + 1;
            else secretoCounts[s] = 1;

            if (jugadorCounts.TryGetValue(j, out int jCount)) jugadorCounts[j] = jCount + 1;
            else jugadorCounts[j] = 1;
        }

        int colorCorrecto = 0;
        foreach (var (color, sCount) in secretoCounts)
        {
            if (jugadorCounts.TryGetValue(color, out int jCount))
            {
                colorCorrecto += Math.Min(sCount, jCount);
            }
        }

        return (bienPosicionados, colorCorrecto);
    }
}

