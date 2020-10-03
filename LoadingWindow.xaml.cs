using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Spectrum
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            Settings.SettingsAllForm(this);
            Loaded += async (x, y) =>
            {
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
