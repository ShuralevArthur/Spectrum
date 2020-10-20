using System.Windows;

namespace Spectrum
{
    struct Settings 
    {
        // Общие настройки всех форм.
        public static System.Action<Window> SettingsAllForm = window =>
        {
            // События.
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.MouseLeftButtonDown += (x, y) => window.DragMove();
            // Свойства.
            window.Title = "Spectrum";
        };
    }
}
