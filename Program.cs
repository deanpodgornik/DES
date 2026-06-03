using System.Drawing;
using System.Xml.Serialization;

namespace ScreenAutoClicker;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Screen Auto-Clicker Application (.NET 10)");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        // Preberi konfiguracijo iz XML datoteke
        string configPath = "config.xml";

        if (!File.Exists(configPath))
        {
            Console.WriteLine($"NAPAKA: Konfiguracijska datoteka '{configPath}' ne obstaja!");
            Console.WriteLine("Ustvarjam privzeto konfiguracijo...");
            ConfigLoader.CreateDefaultConfig(configPath);
            Console.WriteLine($"Konfiguracijska datoteka '{configPath}' je bila ustvarjena.");
            Console.WriteLine("Uredi jo in znova zaženi aplikacijo.");
            return;
        }

        AutoClickerConfig config;
        try
        {
            config = ConfigLoader.Load(configPath);
            Console.WriteLine($"Konfiguracija naložena iz: {configPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NAPAKA pri nalaganju konfiguracije: {ex.Message}");
            return;
        }

        if (!File.Exists(config.TemplateImagePath))
        {
            Console.WriteLine($"NAPAKA: Template slika '{config.TemplateImagePath}' ne obstaja!");
            Console.WriteLine("Shrani sliko, ki jo želiš iskati kot 'template.png' v isti mapi kot aplikacija.");
            return;
        }

        Console.WriteLine($"Iščem sliko: {config.TemplateImagePath}");
        Console.WriteLine($"Območje iskanja: ({config.SearchX}, {config.SearchY}) - {config.SearchWidth}x{config.SearchHeight}px");
        Console.WriteLine($"Klik na: ({config.ClickX}, {config.ClickY})");
        Console.WriteLine($"Interval: {config.CheckIntervalMs}ms");
        Console.WriteLine($"Tolerance: {config.MatchTolerance}");
        Console.WriteLine($"Debug mode: {(config.DebugMode ? "VKLJUČEN" : "IZKLJUČEN")}");

        if (config.DebugMode)
        {
            Console.WriteLine();
            Console.WriteLine("⚠ DEBUG MODE: Screenshoti bodo shranjeni v 'debug_screenshots' mapo");
            Directory.CreateDirectory("debug_screenshots");
        }

        Console.WriteLine();
        Console.WriteLine("Pritisni Ctrl+C za izhod...");
        Console.WriteLine();

        var autoClicker = new AutoClicker(config);
        await autoClicker.StartAsync();
    }
}

[XmlRoot("AutoClickerConfig")]
public class AutoClickerConfig
{
    [XmlElement("SearchX")]
    public int SearchX { get; set; }

    [XmlElement("SearchY")]
    public int SearchY { get; set; }

    [XmlElement("SearchWidth")]
    public int SearchWidth { get; set; }

    [XmlElement("SearchHeight")]
    public int SearchHeight { get; set; }

    [XmlElement("ClickX")]
    public int ClickX { get; set; }

    [XmlElement("ClickY")]
    public int ClickY { get; set; }

    [XmlElement("TemplateImagePath")]
    public string TemplateImagePath { get; set; } = "";

    [XmlElement("CheckIntervalMs")]
    public int CheckIntervalMs { get; set; }

    [XmlElement("MatchTolerance")]
    public int MatchTolerance { get; set; }

    [XmlElement("DebugMode")]
    public bool DebugMode { get; set; }

    [XmlElement("ClickDelayMs")]
    public int ClickDelayMs { get; set; }

    [XmlElement("OcrEnabled")]
    public bool OcrEnabled { get; set; }

    [XmlElement("OcrRegion1X")]
    public int OcrRegion1X { get; set; }
    [XmlElement("OcrRegion1Y")]
    public int OcrRegion1Y { get; set; }
    [XmlElement("OcrRegion1Width")]
    public int OcrRegion1Width { get; set; }
    [XmlElement("OcrRegion1Height")]
    public int OcrRegion1Height { get; set; }

    [XmlElement("OcrRegion2X")]
    public int OcrRegion2X { get; set; }
    [XmlElement("OcrRegion2Y")]
    public int OcrRegion2Y { get; set; }
    [XmlElement("OcrRegion2Width")]
    public int OcrRegion2Width { get; set; }
    [XmlElement("OcrRegion2Height")]
    public int OcrRegion2Height { get; set; }
}

public static class ConfigLoader
{
    public static AutoClickerConfig Load(string filePath)
    {
        var serializer = new XmlSerializer(typeof(AutoClickerConfig));
        using var reader = new StreamReader(filePath);
        var config = (AutoClickerConfig?)serializer.Deserialize(reader);

        if (config == null)
            throw new InvalidOperationException("Napaka pri deserializaciji konfiguracije");

        return config;
    }

    public static void CreateDefaultConfig(string filePath)
    {
        var defaultConfig = new AutoClickerConfig
        {
            SearchX = 100,
            SearchY = 100,
            SearchWidth = 50,
            SearchHeight = 50,
            ClickX = 500,
            ClickY = 500,
            TemplateImagePath = "template.png",
            CheckIntervalMs = 3000,
            MatchTolerance = 30,
            DebugMode = false,
            ClickDelayMs = 3000,
            OcrEnabled = false,
            OcrRegion1X = 0, OcrRegion1Y = 0, OcrRegion1Width = 40, OcrRegion1Height = 20,
            OcrRegion2X = 0, OcrRegion2Y = 0, OcrRegion2Width = 40, OcrRegion2Height = 20
        };

        var serializer = new XmlSerializer(typeof(AutoClickerConfig));
        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, defaultConfig);
    }
}

class AutoClicker
{
    private readonly AutoClickerConfig _config;
    private readonly ImageMatcher _imageMatcher;
    private readonly MouseController _mouseController;
    private readonly Bitmap _templateImage;
    private readonly OcrReader? _ocrReader;

    public AutoClicker(AutoClickerConfig config)
    {
        _config = config;
        _imageMatcher = new ImageMatcher();
        _mouseController = new MouseController();
        _templateImage = new Bitmap(config.TemplateImagePath);
        if (config.OcrEnabled)
            _ocrReader = new OcrReader();
    }

    public async Task StartAsync()
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine("\nUstavljam aplikacijo...");
        };

        int checkCount = 0;
        int matchCount = 0;

        while (!cts.Token.IsCancellationRequested)
        {
            checkCount++;

            try
            {
                // Zajemi screenshot določenega območja
                using var screenshot = ScreenCapture.CaptureRegion(
                    _config.SearchX,
                    _config.SearchY,
                    _config.SearchWidth,
                    _config.SearchHeight
                );

                // Debug: Shrani screenshot
                if (_config.DebugMode)
                {
                    string debugPath = $"debug_screenshots/capture_{checkCount:D4}.png";
                    screenshot.Save(debugPath);
                }

                // Preveri, če se slika ujema
                var matchResult = _imageMatcher.IsMatchWithDetails(screenshot, _templateImage, _config.MatchTolerance);

                if (matchResult.IsMatch)
                {
                    matchCount++;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ UJEMANJE #{matchCount}!");

                    // OCR: preberi dve številki
                    if (_config.OcrEnabled && _ocrReader != null)
                    {
                        using var ocr1 = ScreenCapture.CaptureRegion(_config.OcrRegion1X, _config.OcrRegion1Y, _config.OcrRegion1Width, _config.OcrRegion1Height);
                        using var ocr2 = ScreenCapture.CaptureRegion(_config.OcrRegion2X, _config.OcrRegion2Y, _config.OcrRegion2Width, _config.OcrRegion2Height);

                        if (_config.DebugMode)
                        {
                            ocr1.Save($"debug_screenshots/ocr1_{checkCount:D4}.png");
                            ocr2.Save($"debug_screenshots/ocr2_{checkCount:D4}.png");
                        }

                        string val1 = _ocrReader.ReadNumber(ocr1, _config.DebugMode ? $"debug_screenshots/ocr1_{checkCount:D4}_prep.png" : null);
                        string val2 = _ocrReader.ReadNumber(ocr2, _config.DebugMode ? $"debug_screenshots/ocr2_{checkCount:D4}_prep.png" : null);
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] OCR → Številka 1: {val1}  |  Številka 2: {val2}");
                    }                    

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ UJEMANJE #{matchCount}! Čakam {_config.ClickDelayMs / 1000.0:0.#} sekunde pred klikom...");
                    await Task.Delay(_config.ClickDelayMs, cts.Token);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Klikam na ({_config.ClickX}, {_config.ClickY})");

                    // Premakni miško in klikni
                    _mouseController.Click(_config.ClickX, _config.ClickY);
                }
                else
                {
                    if (_config.DebugMode)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✗ Preverjanje #{checkCount} - NI ujemanja");
                        Console.WriteLine($"  → Najv. razlika: R={matchResult.MaxDiffR}, G={matchResult.MaxDiffG}, B={matchResult.MaxDiffB}");
                        Console.WriteLine($"  → Screenshot: debug_screenshots/capture_{checkCount:D4}.png");
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✗ Preverjanje #{checkCount} - slika se ne ujema");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] NAPAKA: {ex.Message}");
            }

            // Počakaj pred naslednjo iteracijo
            await Task.Delay(_config.CheckIntervalMs, cts.Token);
        }

        _templateImage.Dispose();
        _ocrReader?.Dispose();
        Console.WriteLine($"\nZaključeno. Skupaj pregledov: {checkCount}, Ujemanj: {matchCount}");
    }
}
