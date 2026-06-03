using System.Drawing;
using Tesseract;

namespace ScreenAutoClicker;

class OcrReader : IDisposable
{
    private TesseractEngine? _engine;

    public bool IsAvailable => _engine != null;

    public OcrReader(string tessDataPath = "tessdata", string language = "eng")
    {
        try
        {
            // TesseractOnly: whitelist deluje samo z legacy engine, ne z LSTM
            _engine = new TesseractEngine(tessDataPath, language, EngineMode.TesseractOnly);
            _engine.SetVariable("tessedit_char_whitelist", "0123456789");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OCR] Opozorilo: Tesseract ni na voljo ({ex.Message})");
            Console.WriteLine("[OCR] Prenesi eng.traineddata in ga shrani v mapo 'tessdata/'");
            _engine = null;
        }
    }

    public string ReadNumber(Bitmap image, string? debugSavePath = null)
    {
        if (_engine == null) return "N/A";

        try
        {
            using var prepared = Preprocess(image);

            if (debugSavePath != null)
                prepared.Save(debugSavePath);

            using var ms = new System.IO.MemoryStream();
            prepared.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            using var pix = Pix.LoadFromMemory(ms.ToArray());
            using var page = _engine.Process(pix, PageSegMode.SingleWord);
            var text = page.GetText().Trim().Replace("\n", "").Replace(" ", "");
            // Obdrži samo cifre (whitelist backup)
            text = new string(text.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(text) ? "?" : text;
        }
        catch (Exception ex)
        {
            return $"ERR:{ex.Message}";
        }
    }

    private static Bitmap Preprocess(Bitmap src)
    {
        int pad = 10;
        int scale = 4;
        int outW = (src.Width + pad * 2) * scale;
        int outH = (src.Height + pad * 2) * scale;

        var scaled = new Bitmap(outW, outH);
        using (var g = Graphics.FromImage(scaled))
        {
            g.Clear(Color.White);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.DrawImage(src,
                new Rectangle(pad * scale, pad * scale, src.Width * scale, src.Height * scale),
                new Rectangle(0, 0, src.Width, src.Height),
                GraphicsUnit.Pixel);
        }

        // Binarizacija: pretvori v čisto črno/belo za boljši OCR
        var data = scaled.LockBits(
            new Rectangle(0, 0, scaled.Width, scaled.Height),
            System.Drawing.Imaging.ImageLockMode.ReadWrite,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        unsafe
        {
            byte* ptr = (byte*)data.Scan0;
            int stride = data.Stride;
            for (int y = 0; y < scaled.Height; y++)
            {
                for (int x = 0; x < scaled.Width; x++)
                {
                    int offset = y * stride + x * 4;
                    int brightness = (ptr[offset] + ptr[offset + 1] + ptr[offset + 2]) / 3;
                    byte val = brightness < 128 ? (byte)0 : (byte)255;
                    ptr[offset] = val;     // B
                    ptr[offset + 1] = val; // G
                    ptr[offset + 2] = val; // R
                    ptr[offset + 3] = 255; // A
                }
            }
        }
        scaled.UnlockBits(data);

        return scaled;
    }

    public void Dispose() => _engine?.Dispose();
}
