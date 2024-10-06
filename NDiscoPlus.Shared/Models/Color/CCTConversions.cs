namespace NDiscoPlus.Shared.Models.Color;
public readonly partial struct NDPColor
{
    /// <summary>
    /// Create an NDPColor for a given correlated color temperature.
    /// </summary>
    public static class FromCCT
    {
        /// <summary>
        /// Compute color using CIE Illuminant D Series method.
        /// </summary>
        /// <param name="T">
        /// The correlated color temperature in kelvin.
        /// </param>
        public static NDPColor Daylight(double T, double brightness = 1d)
        {
            // https://en.wikipedia.org/wiki/Standard_illuminant#Computation

            double T2 = Math.Pow(T, 2);
            double T3 = Math.Pow(T, 3);

            const double _10pow3 = 1_000;
            const double _10pow6 = 1_000_000;
            const double _10pow9 = 1_000_000_000;

            double x = T switch
            {
                >= 4000 and <= 7000 => 0.244063d + (0.09911d * (_10pow3 / T)) + (2.9678d * (_10pow6 / T2)) - (4.6070d * (_10pow9 / T3)),
                > 7000 and <= 25000 => 0.237040d + (0.24748d * (_10pow3 / T)) + (1.9018d * (_10pow6 / T2)) - (2.0064d * (_10pow9 / T3)),
                _ => throw new ArgumentOutOfRangeException(nameof(T), "CIE Illuminant D Series is defined between 4000 and 25 000 kelvin.")
            };
            double y = (-3.000 * Math.Pow(x, 2)) + (2.870 * x) - 0.275;

            return new NDPColor(x, y, brightness);
        }

        /// <summary>
        /// Compute color using a variant of Planckian locus approximation.
        /// </summary>
        /// <param name="T">
        /// The correlated color temperature in kelvin.
        /// </param>
        public static NDPColor BlackBody(double T, double brightness = 1d)
        {
            const string _kOutOfRangeMsg = "The Planckian locus approximation used is defined between 1667 and 25 000 kelvin.";

            // https://en.wikipedia.org/wiki/Planckian_locus#Approximation

            double T2 = Math.Pow(T, 2);
            double T3 = Math.Pow(T, 3);

            const double _10pow3 = 1_000;
            const double _10pow6 = 1_000_000;
            const double _10pow9 = 1_000_000_000;

            double x = T switch
            {
                >= 1667 and <= 4000 => (-0.2661239d * (_10pow9 / T3)) - (0.2343589d * (_10pow6 / T2)) + (0.8776956d * (_10pow3 / T)) + 0.179910d,
                >= 4000 and <= 25000 => (-3.0258469d * (_10pow9 / T3)) + (2.1070379d * (_10pow6 / T2)) + (0.2226347d * (_10pow3 / T)) + 0.240390d,
                _ => throw new ArgumentOutOfRangeException(nameof(T), _kOutOfRangeMsg)
            };

            double x2 = Math.Pow(x, 2);
            double x3 = Math.Pow(x, 3);

            double y = T switch
            {
                >= 1667 and <= 2222 => (-1.1063814d * x3) - (1.34811020d * x2) + (2.18555832d * x) - 0.20219683d,
                >= 2222 and <= 4000 => (-0.9549476d * x3) - (1.37418593d * x2) + (2.09137015d * x) - 0.16748867d,
                >= 4000 and <= 25000 => (3.0817580d * x3) - (5.87338670d * x2) + (3.75112997d * x) - 0.37001483d,
                _ => throw new ArgumentOutOfRangeException(nameof(T), _kOutOfRangeMsg)
            };

            return new NDPColor(x, y, brightness);
        }
    }
}
