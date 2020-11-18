using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class Voice
    {
        public static async void Speak(string text)
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
    }
}
