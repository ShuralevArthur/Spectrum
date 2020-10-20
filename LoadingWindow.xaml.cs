using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Spectrum
{
    public partial class LoadingWindow
    {
        private readonly List<String> _dllNames = new List<String>
        {
            "Tesseract.dll", "System.Speech.dll", "IronOcr.xml",
            "IronOcr.Russian.xml", "IronOcr.Russian.dll", "IronOcr.dll"
        };

        public LoadingWindow()
        {
            InitializeComponent();
            Settings.SettingsAllForm(this);
            Loaded += async (x, y) =>
            {
                foreach (var dllName in _dllNames)
                {
                    if (!File.Exists(dllName))
                    {
                        MessageBox.Show("The necessary files are missing, reinstall the program! \n Отсутствуют необходимые файлы, переустановите программу!", "Error/Ошибка!");
                        foreach (Window windows in Application.Current.Windows) windows.Close();
                    }
                }

                await Task.Run(() =>
                {
                    Thread.Sleep(1500);
                    Dispatcher.Invoke(delegate
                    {
                        new MainWindow().Show();
                        Close();
                    });
                });
            };
        }
    }
}
