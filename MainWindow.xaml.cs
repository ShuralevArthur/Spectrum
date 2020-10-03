using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Windows.Documents;
using Tesseract;
using System.Windows.Threading;
using System.Diagnostics;

namespace Spectrum
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        extern static Boolean SetProcessWorkingSetSize(IntPtr hProcess,
            Int32 dwMinimumWorkingSetSize, Int32 dwMaximumWorkingSetSize);

        private static Int16 _width = 1;
        private static Int16 _heigth = 1;
        private static Boolean _isEng = false;
        readonly GlobalHook gh = new GlobalHook();

        readonly Action<MainWindow> Events = window =>
        {
            window.ButtonClose.Click += (x, y) =>
            {
                foreach (Window windows in App.Current.Windows) windows.Close();
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
            Loaded += (x, y) =>
            {
                //Speak("Spectrum успешно запустился");
                Events(this);

                gh.KeyDown += (d, f) =>
                {
                    if (f.KeyCode == System.Windows.Forms.Keys.Q) Screen();
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

                SliderZoomValueX.ValueChanged += (f, z) =>
                {
                    _heigth = (Int16)z.NewValue;
                };

                SliderZoomValueY.ValueChanged += (f, z) =>
                {
                    _width = (Int16)z.NewValue;
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

        async void SetBitmapImage()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var rectangle = new Rectangle(
                    System.Windows.Forms.Cursor.Position.X,
                    System.Windows.Forms.Cursor.Position.Y, _width, _heigth);
                    var bmp = new Bitmap(
                        rectangle.Width, rectangle.Height,
                        PixelFormat.Format32bppArgb);
                    var graphics = Graphics.FromImage(bmp);
                    graphics.CopyFromScreen(
                        rectangle.Left,
                        rectangle.Top, 0, 0, bmp.Size,
                        CopyPixelOperation.SourceCopy);
                    Dispatcher.Invoke(delegate
                    {
                        ImageNowImage.Source = BitmapToImageSource(bmp);
                    });
                }
            });
        }

        async void Screen()
        {
            await Task.Run(() =>
            {
                var rectangle = new Rectangle(
                System.Windows.Forms.Cursor.Position.X,
                System.Windows.Forms.Cursor.Position.Y, _width, _heigth);
                var bmp = new Bitmap(
                    rectangle.Width, rectangle.Height,
                    PixelFormat.Format32bppArgb);
                var graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(
                    rectangle.Left,
                    rectangle.Top, 0, 0, bmp.Size,
                    CopyPixelOperation.SourceCopy);
                var textResult = ReturnClearText(ReadTextInImg(bmp));
                Dispatcher.Invoke(delegate
                {
                    SetValues(this, bmp, textResult);
                });
                Speak(textResult);
            });
        }

        readonly Func<Bitmap, BitmapImage> BitmapToImageSource = bitmap =>
        {
            var ms = new MemoryStream() { Position = 0 };
            var bitmapImage = new BitmapImage()
            {
                CacheOption = BitmapCacheOption.None
            };
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            return bitmapImage;
        };

        readonly Func<Bitmap, String> ReadTextInImg = bmp =>
        _isEng == false
        ? new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", "rus", EngineMode.Default)
        .Process(PixConverter.ToPix(bmp)).GetText()
        : new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", "eng", EngineMode.Default)
        .Process(PixConverter.ToPix(bmp)).GetText();

        readonly Func<String, String> ReturnClearText = text =>
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
            return endText;
        };

        async void Speak(String text)
        {
            await Task.Run(() =>
            {
                var speaker = new SpeechSynthesizer()
                {
                    Rate = 1,
                    Volume = 100
                };
                speaker.Speak(text);
            });
        }

        readonly Action<MainWindow, Bitmap, String> SetValues = (MainWindow, bmp, textResult) =>
        {
            MainWindow.ImageNowImage.Source = MainWindow.BitmapToImageSource(bmp);
            MainWindow.RichTextBoxText.Document.Blocks.Clear();
            MainWindow.RichTextBoxText.Document.Blocks.Add(new Paragraph(new Run(textResult)));
        };
    }
}
