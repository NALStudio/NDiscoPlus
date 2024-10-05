namespace NDiscoPlus.Shared.Models.Color;
public readonly partial struct NDPColor
{
    /// <summary>
    /// Create an NDPColor from a given correlated color temperature.
    /// The value is converted using CIE Illuminant D Series method.
    /// </summary>
    public static NDPColor FromCCT(double T, double brightness = 1d)
    {
        // https://en.wikipedia.org/wiki/Standard_illuminant#Computation

        double T2 = Math.Pow(T, 2);
        double T3 = Math.Pow(T, 3);

        const double _10pow3 = 1_000;
        const double _10pow6 = 1_000_000;
        const double _10pow9 = 1_000_000_000;

        double x = T switch
        {
            >= 4000d and <= 7000d => 0.244063d + (0.09911d * (_10pow3 / T)) + (2.9678d * (_10pow6 / T2)) - (4.6070d * (_10pow9 / T3)),
            > 7000d and <= 25000d => 0.237040d + (0.24748d * (_10pow3 / T)) + (1.9018d * (_10pow6 / T2)) - (2.0064d * (_10pow9 / T3)),
            _ => throw new ArgumentOutOfRangeException(nameof(T))
        };
        double y = (-3.000 * Math.Pow(x, 2)) + (2.870 * x) - 0.275;

        return new NDPColor(x, y, brightness);
    }
}
