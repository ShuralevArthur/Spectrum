using System.Windows.Media;

namespace Spectrum
{
    public struct Colors
    {
        // Background Color.
        public static SolidColorBrush ColorOne { get { return (SolidColorBrush) new BrushConverter().ConvertFrom("#3AAFA9"); } }
        // Foreground Color.
        public static SolidColorBrush ColorTwo { get { return (SolidColorBrush) new BrushConverter().ConvertFrom("#C3073F"); } }
    }

    // Fontfamily: Bodoni MT Condensed.
}
