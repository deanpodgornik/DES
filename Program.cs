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

            if (config.UseDebugConfigFile)
            {
                string debugConfigPath = "config-debug.xml";
                if (File.Exists(debugConfigPath))
                {
                    config = ConfigLoader.Load(debugConfigPath);
                    configPath = debugConfigPath;
                }
                else
                {
                    Console.WriteLine($"OPOZORILO: UseDebugConfigFile je vključen, vendar '{debugConfigPath}' ne obstaja. Uporaba '{configPath}'.");
                }
            }

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

    [XmlElement("UseDebugConfigFile")]
    public bool UseDebugConfigFile { get; set; }

    [XmlElement("ClickDelayMs")]
    public int ClickDelayMs { get; set; }

    [XmlElement("DbEnabled")]
    public bool DbEnabled { get; set; }

    [XmlElement("DbConnectionString")]
    public string DbConnectionString { get; set; } = "";

    /// <summary>
    /// How many characters from the END of the keyboard-captured code to use
    /// as the database search key.  0 = use the full captured string.
    /// </summary>
    [XmlElement("DbCodeSearchLength")]
    public int DbCodeSearchLength { get; set; } = 10;

    [XmlElement("DisplayEnabled")]
    public bool DisplayEnabled { get; set; }

    [XmlElement("DisplayPort")]
    public string DisplayPort { get; set; } = "COM3";

    [XmlElement("DisplayGreeting")]
    public string DisplayGreeting { get; set; } = "Pozdravljeni!";

    [XmlElement("TemplateNotValidImagePath")]
    public string TemplateNotValidImagePath { get; set; } = "";

    [XmlElement("Search2X")]
    public int Search2X { get; set; }
    [XmlElement("Search2Y")]
    public int Search2Y { get; set; }
    [XmlElement("Search2Width")]
    public int Search2Width { get; set; }
    [XmlElement("Search2Height")]
    public int Search2Height { get; set; }

    [XmlElement("DisplayMessageNotValid")]
    public string DisplayMessageNotValid { get; set; } = "Dobrodošli!";
    [XmlElement("DisplayMessageTimeBased")]
    public string DisplayMessageTimeBased { get; set; } = "Velja do: {0}";

    [XmlElement("DisplayMessageEntries")]
    public string DisplayMessageEntries { get; set; } = "Vhodov: {0}/{1} do {2}";}

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
            DbEnabled = false,
            DbConnectionString = "Server=localhost\\SQLEXPRESS;Database=SIS;Trusted_Connection=True;TrustServerCertificate=True;",
            DbCodeSearchLength = 10,
            TemplateNotValidImagePath = "",
            Search2X = 100, Search2Y = 100, Search2Width = 50, Search2Height = 50,
            DisplayMessageNotValid = "Dobrodošli!",
            DisplayMessageTimeBased = "Velja do: {0}",
            DisplayMessageEntries = "Vhodov: {0}/{1} do {2}"
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
    private readonly Bitmap? _template2Image;
    private readonly KeyboardHook? _keyboardHook;
    private readonly DatabaseService? _dbService;
    private readonly Cd7220Display? _display;
    private string? _lastDisplayMessage;

    public AutoClicker(AutoClickerConfig config)
    {
        _config = config;
        _imageMatcher = new ImageMatcher();
        _mouseController = new MouseController();
        _templateImage = new Bitmap(config.TemplateImagePath);
        if (!string.IsNullOrEmpty(config.TemplateNotValidImagePath) && File.Exists(config.TemplateNotValidImagePath))
        {
            _template2Image = new Bitmap(config.TemplateNotValidImagePath);
            Console.WriteLine($"Druga template slika naložena: {config.TemplateNotValidImagePath}");
        }
        if (config.DbEnabled)
        {
            _keyboardHook = new KeyboardHook();
            _dbService = new DatabaseService(config.DbConnectionString, config.DbCodeSearchLength);
            Console.WriteLine("[KB] Globalni tipkovniški hook aktiven");
        }
        if (config.DisplayEnabled)
        {
            _display = new Cd7220Display(config.DisplayPort);
            try
            {
                _display.Open();
                Console.WriteLine($"[CD7220] Prikazovalnik odprt na {config.DisplayPort}");
                _display.ShowMessage(config.DisplayGreeting);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CD7220] Napaka pri odpiranju {config.DisplayPort}: {ex.Message}");
                _display.Dispose();
                _display = null;
            }
        }
    }

    private void ShowDisplay(string message)
    {
        if (message == _lastDisplayMessage) return;
        _lastDisplayMessage = message;
        _display?.ShowMessage(message);
        if (_config.DebugMode)
            Console.WriteLine($"[CD7220] Prikaz: '{message}'");
    }

    private void ShowDisplayValues(string line1, string line2)
    {
        string key = line1 + "|" + line2;
        if (key == _lastDisplayMessage) return;
        _lastDisplayMessage = key;
        _display?.ShowValues(line1, line2);
        if (_config.DebugMode)
            Console.WriteLine($"[CD7220] Prikaz vrednosti: '{line1}' / '{line2}'");
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
                    //KARTA JE VELJAVNA (template1)
                    
                    matchCount++;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ UJEMANJE #{matchCount}!");

                    // Iskanje podatkov iz baze po ID-ju z bralnika (tipkovnica)
                    if (_config.DbEnabled && _dbService != null)
                    {
                        string capturedCode = _keyboardHook?.LastCode ?? "";
                        if (!string.IsNullOrEmpty(capturedCode))
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] KB → Zajet ID: '{capturedCode}'");
                            var info = await _dbService.GetUserEntriesAsync(capturedCode);
                            if (info != null)
                            {
                                if (info.Unlimited)
                                {
                                    string validTo = info.ValidTo.ToString("dd.MM.yyyy");
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] DB → {info.Name} | Mesečna/letna kartica, veljavna do: {validTo}");
                                    if (_display != null)
                                        ShowDisplayValues(string.Format(_config.DisplayMessageTimeBased, validTo), info.Name);
                                }
                                else
                                {
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] DB → {info.Name} | Vstopi: {info.UsedEntries:0}/{info.TotalEntries:0}");
                                    if (_display != null)
                                        ShowDisplayValues(string.Format(_config.DisplayMessageEntries, info.UsedEntries.ToString("0"), info.TotalEntries.ToString("0"), info.ValidTo.ToString("dd.MM.yy")), info.Name);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] DB → Uporabnik z ID '{capturedCode}' ni najden.");
                            }
                        }
                        else
                        {
                            if (_config.DebugMode)
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] KB → Še ni zajetega ID-ja.");
                        }
                    }                    

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ UJEMANJE #{matchCount}! Čakam {_config.ClickDelayMs / 1000.0:0.#} sekunde pred klikom...");
                    await Task.Delay(_config.ClickDelayMs, cts.Token);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Klikam na ({_config.ClickX}, {_config.ClickY})");

                    // Premakni miško in klikni
                    _mouseController.Click(_config.ClickX, _config.ClickY);
                    
                }
                else if (_template2Image != null)
                {
                    // PREVERI ČE KARTA NI VELJAVNA (template2)
                    //zajameš drugo območje, kjer se nahaja template2

                    using var screenshot2 = ScreenCapture.CaptureRegion(
                        _config.Search2X, _config.Search2Y, _config.Search2Width, _config.Search2Height);
                    if (_config.DebugMode)
                        screenshot2.Save($"debug_screenshots/capture2_{checkCount:D4}.png");

                    var match2 = _imageMatcher.IsMatchWithDetails(screenshot2, _template2Image, _config.MatchTolerance);
                    if (match2.IsMatch)
                    {
                        matchCount++;
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ UJEMANJE (template2) #{matchCount}!");
                        ShowDisplay(_config.DisplayMessageNotValid);
                    }
                    else
                    {
                        if (_config.DebugMode)
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✗ Preverjanje #{checkCount} - NI ujemanja (niti template2)");
                            Console.WriteLine($"  → Najv. razlika: R={matchResult.MaxDiffR}, G={matchResult.MaxDiffG}, B={matchResult.MaxDiffB}");
                            Console.WriteLine($"  → Screenshot: debug_screenshots/capture_{checkCount:D4}.png");
                        }
                        else
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✗ Preverjanje #{checkCount} - slika se ne ujema");
                        }

                        //Prikaži pozdravno sporočilo
                        ShowDisplay(_config.DisplayGreeting);
                    }
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
        _template2Image?.Dispose();
        _keyboardHook?.Dispose();
        _display?.Dispose();
        Console.WriteLine($"\nZaključeno. Skupaj pregledov: {checkCount}, Ujemanj: {matchCount}");
    }
}
