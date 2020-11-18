using System;
using System.Windows;

namespace Spectrum
{
    class Settings 
    {
        // Общие настройки всех форм.
        public static Action<Window> SettingsAllForm = window =>
        {
            // События.
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.MouseLeftButtonDown += (x, y) => window.DragMove();
            // Свойства.
            window.Title = "Spectrum";
        };

        public static readonly Action<MainWindow, string> SetVersion = (mainWindow, version) =>
            mainWindow.LabelVersion.Content = $"Version: {version}";

        public static readonly Action<MainWindow> SetDefaultBorderThickness = (mainWindow) =>
            mainWindow.ButtonRusLang.BorderThickness = new Thickness(2, 2, 2, 2);
    }
}
