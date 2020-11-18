using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class Other
    {
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

        /*private readonly Func<Bitmap, String> _readTextInImg = bmp =>
        _isEng == false
        ? new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", "rus", EngineMode.Default)
        .Process(PixConverter.ToPix(bmp)).GetText()
        : new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", "eng", EngineMode.Default)
        .Process(PixConverter.ToPix(bmp)).GetText();*/ // Tesseract OCR. 
    }
}
