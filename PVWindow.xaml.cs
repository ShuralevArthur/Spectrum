using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace Spectrum
{
    public partial class PVWindow : Window
    {
        public PVWindow()
        {
            InitializeComponent();
            Settings.SettingsAllForm(this);
            Loaded += (x, y) =>
            {
                SetCpuValue();
                SetRamValue();
            };

            ButtonClose.Click += (x, y) => Close();
        }

        async void SetCpuValue()
        {
            await Task.Run(() =>
            {
                using (var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    while (true)
                    {
                        Dispatcher.Invoke(delegate
                        {
                            CPULabel.Content = $"CPU: {(Int32)pc.NextValue()}%";
                        });
                    }
                }
            });
        }

        async void SetRamValue()
        {
            await Task.Run(() =>
            {
                using (var pc = new PerformanceCounter("Memory", "% Committed Bytes In Use"))
                {
                    while (true)
                    {
                        Dispatcher.Invoke(delegate
                        {
                            RAMLabel.Content = $"RAM: {(Int32)pc.NextValue()}%";
                        });
                    }
                }
            });
        }
    }
}
