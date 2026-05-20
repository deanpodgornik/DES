using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenAutoClicker;

public class MatchResult
{
    public bool IsMatch { get; set; }
    public int MaxDiffR { get; set; }
    public int MaxDiffG { get; set; }
    public int MaxDiffB { get; set; }
}

public class ImageMatcher
{
    public bool IsMatch(Bitmap screenshot, Bitmap template, int tolerance)
    {
        return IsMatchWithDetails(screenshot, template, tolerance).IsMatch;
    }

    public MatchResult IsMatchWithDetails(Bitmap screenshot, Bitmap template, int tolerance)
    {
        var result = new MatchResult { IsMatch = false };

        if (screenshot.Width != template.Width || screenshot.Height != template.Height)
        {
            result.MaxDiffR = 999;
            result.MaxDiffG = 999;
            result.MaxDiffB = 999;
            return result;
        }

        unsafe
        {
            var screenshotData = screenshot.LockBits(
                new Rectangle(0, 0, screenshot.Width, screenshot.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var templateData = template.LockBits(
                new Rectangle(0, 0, template.Width, template.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                byte* screenshotPtr = (byte*)screenshotData.Scan0;
                byte* templatePtr = (byte*)templateData.Scan0;

                int bytes = Math.Abs(screenshotData.Stride) * screenshot.Height;
                int maxDiffR = 0, maxDiffG = 0, maxDiffB = 0;
                bool allMatch = true;

                for (int i = 0; i < bytes; i += 4)
                {
                    int diffB = Math.Abs(screenshotPtr[i] - templatePtr[i]);
                    int diffG = Math.Abs(screenshotPtr[i + 1] - templatePtr[i + 1]);
                    int diffR = Math.Abs(screenshotPtr[i + 2] - templatePtr[i + 2]);

                    maxDiffR = Math.Max(maxDiffR, diffR);
                    maxDiffG = Math.Max(maxDiffG, diffG);
                    maxDiffB = Math.Max(maxDiffB, diffB);

                    if (diffB > tolerance || diffG > tolerance || diffR > tolerance)
                    {
                        allMatch = false;
                    }
                }

                screenshot.UnlockBits(screenshotData);
                template.UnlockBits(templateData);

                result.IsMatch = allMatch;
                result.MaxDiffR = maxDiffR;
                result.MaxDiffG = maxDiffG;
                result.MaxDiffB = maxDiffB;
                return result;
            }
            catch
            {
                screenshot.UnlockBits(screenshotData);
                template.UnlockBits(templateData);
                throw;
            }
        }
    }
}
