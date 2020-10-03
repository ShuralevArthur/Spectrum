namespace Spectrum
{
    public class BitmapCPP
    {
        int* rr = new int, * gg = new int, * bb = new int;
        for (int i = 0; i < biHeight; i++)
        {
            for (int j = 0; j < biWidth * 3; j++)
            {
                GetPixel(j, i, rr, gg, bb);
                double sr = (*rr + *gg + *bb) / 3;
                if (sr < Y0)
                {
                    PutPixelRGB(j, i, 0, 0, 0);
                }
                else PutPixelRGB(j, i, 255, 255, 255);
            }
        }
    }
}
