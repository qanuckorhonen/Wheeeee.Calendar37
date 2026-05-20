using System.Drawing;

namespace Wheeeee.Calendar37.Core.Extensions
{
    public static class ColorExtension
    {
        public static Color Brighten(this Color color, double percentage)
        {
            return Color.FromArgb(
                color.A,
                (int)(color.R + (255 - color.R) * percentage / 100),
                (int)(color.G + (255 - color.G) * percentage / 100),
                (int)(color.B + (255 - color.B) * percentage / 100)
            );
        }

        public static Color Darken(this Color color, double percentage)
        {
            return Color.FromArgb(
                color.A,
                (int)(color.R * (1 - percentage / 100)),
                (int)(color.G * (1 - percentage / 100)),
                (int)(color.B * (1 - percentage / 100))
            );
        }
    }
}
