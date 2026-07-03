using Mastermind;

namespace Mastermind.Tests;

public class UnitTest1
{
    [Fact]
    public void EvaluarIntento_CompleteMatch_ReturnsExpectedValues()
    {
        var (bienPosicionados, colorCorrecto) = Mastermind.EvaluarIntento(
            "ROJO-VERDE-AZUL-AMARILLO",
            "ROJO-VERDE-AZUL-AMARILLO");

        Assert.Equal(4, bienPosicionados);
        Assert.Equal(0, colorCorrecto);
    }

    [Fact]
    public void EvaluarIntento_PartialMatch_ReturnsExpectedValues()
    {
        var (bienPosicionados, colorCorrecto) = Mastermind.EvaluarIntento(
            "ROJO-VERDE-AZUL-AMARILLO",
            "ROJO-VERDE-AMARILLO-AZUL");

        Assert.Equal(2, bienPosicionados);
        Assert.Equal(2, colorCorrecto);
    }

    [Fact]
    public void EvaluarIntento_NoMatch_ReturnsExpectedValues()
    {
        var (bienPosicionados, colorCorrecto) = Mastermind.EvaluarIntento(
            "ROJO-ROJO-ROJO-ROJO",
            "AZUL-AZUL-AZUL-AZUL");

        Assert.Equal(0, bienPosicionados);
        Assert.Equal(0, colorCorrecto);
    }

    [Fact]
    public void EvaluarIntento_DuplicateHandling_ReturnsExpectedValues()
    {
        var (bienPosicionados, colorCorrecto) = Mastermind.EvaluarIntento(
            "ROJO-ROJO-AZUL-VERDE",
            "AZUL-ROJO-ROJO-ROJO");

        Assert.Equal(1, bienPosicionados);
        Assert.Equal(2, colorCorrecto);
    }

    [Fact]
    public void EvaluarIntento_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Mastermind.EvaluarIntento(null!, "ROJO-VERDE-AZUL-AMARILLO"));
        Assert.Throws<ArgumentNullException>(() => Mastermind.EvaluarIntento("ROJO-VERDE-AZUL-AMARILLO", null!));
    }
}

