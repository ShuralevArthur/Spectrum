using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using IronOcr;
using IronOcr.Languages;
using Application = System.Windows.Application; 
//using Tesseract;
//using System.Media;

namespace Spectrum
{
    public partial class MainWindow
    {
        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
        private static short _width = 1;
        private static short _heigth = 1;
        private static bool _isEng = false;
        private readonly GlobalHook _gh = new GlobalHook();
        private static readonly string _version = "beta";

        private readonly Action<MainWindow> Events = window =>
        {
            window.ButtonClose.Click += (x, y) =>
            {
                foreach (Window windows in Application.Current.Windows) windows.Close();
            };
            window.ButtonMinimize.Click += (x, y) => window.WindowState = WindowState.Minimized;
            window.SliderZoomValueX.ValueChanged += (f, z) => _heigth = (short)z.NewValue;
            window.SliderZoomValueY.ValueChanged += (f, z) => _width = (short)z.NewValue;
        };

        public MainWindow()
        {
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            InitializeComponent();
            Settings.SetDefaultBorderThickness(this);
            Settings.SettingsAllForm(this);
            Settings.SetVersion(this, _version);
            Loaded += (x, y) =>
            {
                //Voice.Speak("Spectrum успешно запустился");

                Events(this);
                _gh.KeyDown += (d, f) =>
                {
                    if (f.KeyCode == Keys.PageUp) Screen();
                };

                _gh.KeyDown += (d, f) =>
                {
                    if (f.KeyCode == Keys.PageDown)
                    {
                        var result = System.Windows.Clipboard.GetText();
                        Voice.Speak(result);
                        Dispatcher.Invoke(delegate 
                        {
                            RichTextBoxText.Document.Blocks.Clear();
                            RichTextBoxText.Document.Blocks.Add(new Paragraph(new Run(result)));
                        });
                    }
                };

                MouseWheel += (k, l) =>
                {
                    if (l.Delta < 0)
                    {
                        if ((_width != 999) && (_heigth != 999))
                        {
                            _width += 15;
                            _heigth += 15;
                            SliderZoomValueY.Value += 15;
                            SliderZoomValueX.Value += 15;
                        }
                    }

                    else if (l.Delta > 0)
                    {
                        if ((_width > 20) && (_heigth > 20))
                        {
                            _width -= 15;
                            _heigth -= 15;
                            SliderZoomValueY.Value -= 15;
                            SliderZoomValueX.Value -= 15;
                        }
                    }
                };

                SetBitmapImage();
            };

            ButtonRusLang.Click += (x, y) =>
            {
                ButtonEngLang.BorderThickness = new Thickness(0, 0, 0, 0);
                ButtonRusLang.BorderThickness = new Thickness(2, 2, 2, 2);
                _isEng = false;
            };

            ButtonEngLang.Click += (x, y) =>
            {
                ButtonRusLang.BorderThickness = new Thickness(0, 0, 0, 0);
                ButtonEngLang.BorderThickness = new Thickness(2, 2, 2, 2);
                _isEng = true;
            };
        }

        private async void SetBitmapImage()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var rectangle = new Rectangle(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, _width, _heigth);
                    var bmp = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
                    var graphics = Graphics.FromImage(bmp);
                    graphics.CopyFromScreen(rectangle.Left, rectangle.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                    Dispatcher.Invoke(delegate { ImageNowImage.Source = BitmapToImageSource(bmp); });
                }
            });
        }

        private async void Screen()
        {
            await Task.Run(() =>
            {
                var rectangle = new Rectangle(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, _width, _heigth);
                var bmp = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
                var graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(rectangle.Left, rectangle.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                var textResult = ReadTextInImg(bmp);
                Dispatcher.Invoke(delegate { SetValues(this, bmp, textResult); });
                Voice.Speak(textResult);
            });
        }

        private readonly Func<Bitmap, BitmapImage> BitmapToImageSource = bitmap =>
        {
            var ms = new MemoryStream() { Position = 0 };
            var bitmapImage = new BitmapImage() { CacheOption = BitmapCacheOption.None };
            bitmap.Save(ms, ImageFormat.Bmp);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            return bitmapImage;
        };

        private readonly Func<Bitmap, string> ReadTextInImg = bmp => _isEng
        ? new AutoOcr { Language = English.OcrLanguagePack }.Read(bmp).Text
        : new AutoOcr { Language = Russian.OcrLanguagePack }.Read(bmp).Text;

        private readonly Action<MainWindow, Bitmap, string> SetValues = (mainWindow, bmp, textResult) =>
        {
            mainWindow.ImageNowImage.Source = mainWindow.BitmapToImageSource(bmp);
            mainWindow.RichTextBoxText.Document.Blocks.Clear();
            mainWindow.RichTextBoxText.Document.Blocks.Add(new Paragraph(new Run(textResult)));
        };
    }
}
