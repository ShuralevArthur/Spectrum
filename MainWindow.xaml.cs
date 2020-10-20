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
        static extern Boolean SetProcessWorkingSetSize(IntPtr hProcess, Int32 dwMinimumWorkingSetSize,
            Int32 dwMaximumWorkingSetSize);

        private static Int16 _width = 1;
        private static Int16 _heigth = 1;
        private static Boolean _isEng = false;
        private readonly GlobalHook _gh = new GlobalHook();
        private static readonly String _version = "beta";

        private readonly Action<MainWindow> _events = window =>
        {
            window.ButtonClose.Click += (x, y) =>
            {
                foreach (Window windows in Application.Current.Windows) windows.Close();
            };
            window.ButtonMinimize.Click += (x, y) => window.WindowState = WindowState.Minimized;
            window.ButtonPV.Click += (x, y) => new PVWindow().Show();
        };

        public MainWindow()
        {
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            InitializeComponent();
            ButtonRusLang.BorderThickness = new Thickness(2, 2, 2, 2);
            Settings.SettingsAllForm(this);
            _setVersion(this, _version);
            Loaded += (x, y) =>
            {
                //Speak("Spectrum успешно запустился");
                _events(this);

                _gh.KeyDown += (d, f) =>
                {
                    if (f.KeyCode == Keys.PageUp) Screen();
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

                SliderZoomValueX.ValueChanged += (f, z) => _heigth = (Int16)z.NewValue;
                SliderZoomValueY.ValueChanged += (f, z) => _width = (Int16)z.NewValue;
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
                    var rectangle = new Rectangle(
                    System.Windows.Forms.Cursor.Position.X,
                    System.Windows.Forms.Cursor.Position.Y, _width, _heigth);
                    var bmp = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
                    var graphics = Graphics.FromImage(bmp);
                    graphics.CopyFromScreen(rectangle.Left, rectangle.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                    Dispatcher.Invoke(delegate { ImageNowImage.Source = _bitmapToImageSource(bmp); });
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
                var textResult = _readTextInImg(bmp);
                Dispatcher.Invoke(delegate { _setValues(this, bmp, textResult); });
                Speak(textResult);
            });
        }

        private readonly Func<Bitmap, BitmapImage> _bitmapToImageSource = bitmap =>
        {
            var ms = new MemoryStream() { Position = 0 };
            var bitmapImage = new BitmapImage() { CacheOption = BitmapCacheOption.None };
            bitmap.Save(ms, ImageFormat.Bmp);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            return bitmapImage;
        };

        /*private readonly Func<Bitmap, String> _readTextInImg = bmp =>
        _isEng == false
        ? new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", "rus", EngineMode.Default)
        .Process(PixConverter.ToPix(bmp)).GetText()
        : new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", "eng", EngineMode.Default)
        .Process(PixConverter.ToPix(bmp)).GetText();*/ // Tesseract OCR. 

        private readonly Func<Bitmap, String> _readTextInImg = bmp => _isEng
        ? new AutoOcr { Language = English.OcrLanguagePack }.Read(bmp).Text
        : new AutoOcr { Language = Russian.OcrLanguagePack }.Read(bmp).Text;

        /*private readonly Func<String, String> _returnClearText = text =>
        {
            var endText = String.Empty;
            for (Int32 i = 0; i < text.Length; i++)
            {
                if (Char.IsLetter(text, i))
                    endText += text[i];
                else if (Char.IsDigit(text[i]))
                    endText += text[i];
                else continue;
            }
            return endText.ToLower();
        };*/

        private async void Speak(String text)
        {
            await Task.Run(() =>
            {
                new SpeechSynthesizer()
                {
                    Rate = 1,
                    Volume = 100
                }.Speak(text);
            });
        }

        private readonly Action<MainWindow, Bitmap, String> _setValues = (mainWindow, bmp, textResult) =>
        {
            mainWindow.ImageNowImage.Source = mainWindow._bitmapToImageSource(bmp);
            mainWindow.RichTextBoxText.Document.Blocks.Clear();
            mainWindow.RichTextBoxText.Document.Blocks.Add(new Paragraph(new Run(textResult)));
        };

        private readonly Action<MainWindow, String> _setVersion = (mainWindow, version) =>
            mainWindow.LabelVersion.Content = $"Version: {version}";
    }
}
