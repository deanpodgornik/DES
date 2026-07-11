using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScreenAutoClicker;

public class LogEntryViewModel
{
    private static readonly SolidColorBrush InfoBrush  = new(Color.FromRgb(0x00, 0x78, 0xD4));
    private static readonly SolidColorBrush DebugBrush = new(Color.FromRgb(0x6C, 0x75, 0x7D));
    private static readonly SolidColorBrush WarnBrush  = new(Color.FromRgb(0xFD, 0x7E, 0x14));
    private static readonly SolidColorBrush ErrorBrush = new(Color.FromRgb(0xDC, 0x35, 0x45));

    public LogEntryViewModel(LogEntry entry)
    {
        TimestampText = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        LevelTag = entry.Level switch
        {
            LogLevel.Info  => "[INFO]   ",
            LogLevel.Debug => "[DEBUG]  ",
            LogLevel.Warn  => "[WARN]   ",
            LogLevel.Error => "[ERROR]  ",
            _              => "[INFO]   "
        };
        LevelBrush = entry.Level switch
        {
            LogLevel.Info  => InfoBrush,
            LogLevel.Debug => DebugBrush,
            LogLevel.Warn  => WarnBrush,
            LogLevel.Error => ErrorBrush,
            _              => InfoBrush
        };
        Message = entry.Message;
    }

    public string TimestampText { get; }
    public string LevelTag      { get; }
    public Brush  LevelBrush    { get; }
    public string Message       { get; }
}

public partial class MainWindow : Window
{
    private readonly ObservableCollection<LogEntryViewModel> _logEntries = new();
    private CancellationTokenSource? _cts;
    private readonly DispatcherTimer _clockTimer;

    public MainWindow()
    {
        InitializeComponent();
        LogListBox.ItemsSource = _logEntries;

        Logger.OnLog += OnLogEntry;

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => ClockText.Text = DateTime.Now.ToString("d.M.yyyy HH:mm:ss");
        _clockTimer.Start();
        ClockText.Text = DateTime.Now.ToString("d.M.yyyy HH:mm:ss");
    }

    private const int MaxLogEntries = 1000;

    private void OnLogEntry(LogEntry entry)
    {
        Dispatcher.Invoke(() =>
        {
            while (_logEntries.Count >= MaxLogEntries)
                _logEntries.RemoveAt(0);

            _logEntries.Add(new LogEntryViewModel(entry));
            LogListBox.ScrollIntoView(_logEntries[^1]);
        });
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        SetRunningState(true);
        Logger.Info("Aplikacija zagnana.");

        _cts = new CancellationTokenSource();
        try
        {
            string configPath = "config.xml";
            if (!File.Exists(configPath))
            {
                Logger.Warn($"Konfiguracijska datoteka '{configPath}' ne obstaja! Ustvarjam privzeto...");
                ConfigLoader.CreateDefaultConfig(configPath);
                Logger.Info($"Datoteka '{configPath}' ustvarjena. Uredi jo in znova zaženi.");
                return;
            }

            AutoClickerConfig config;
            try
            {
                config = ConfigLoader.Load(configPath);
                if (config.UseDebugConfigFile)
                {
                    string debugPath = "config-debug.xml";
                    if (File.Exists(debugPath))
                    {
                        config = ConfigLoader.Load(debugPath);
                        configPath = debugPath;
                    }
                    else
                    {
                        Logger.Warn($"UseDebugConfigFile je vključen, a '{debugPath}' ne obstaja. Uporaba '{configPath}'.");
                    }
                }
                Logger.Info("Nastavitve naložene iz konfiguracijske datoteke.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Napaka pri nalaganju konfiguracije: {ex.Message}");
                return;
            }

            if (!File.Exists(config.TemplateImagePath))
            {
                Logger.Error($"Template slika '{config.TemplateImagePath}' ne obstaja!");
                return;
            }

            if (config.DebugMode)
            {
                Logger.Warn("DEBUG MODE: Screenshoti bodo shranjeni v 'debug_screenshots' mapo.");
                Directory.CreateDirectory("debug_screenshots");
            }

            Logger.Info("Pripravljeno za zagon.");

            var autoClicker = new AutoClicker(config);
            Logger.Info("Spremljanje procesa zagnano.");
            await autoClicker.StartAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal stop via Stop button
        }
        catch (Exception ex)
        {
            Logger.Error($"Napaka: {ex.Message}");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetRunningState(false);
            Logger.Info("Spremljanje ustavljeno.");
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _logEntries.Clear();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        string configPath = "config.xml";
        if (File.Exists(configPath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = configPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ne morem odpreti konfiguracije:\n{ex.Message}",
                    "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show("Konfiguracijska datoteka 'config.xml' ne obstaja.",
                "Nastavitve", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void SetRunningState(bool running)
    {
        StartButton.IsEnabled = !running;
        StopButton.IsEnabled  = running;
        StatusDot.Fill = running
            ? new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45))
            : new SolidColorBrush(Color.FromRgb(0x6C, 0x75, 0x7D));
        StatusText.Text = running ? "Teče..." : "Ustavljeno";
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        Logger.OnLog -= OnLogEntry;
        _clockTimer.Stop();
        base.OnClosed(e);
    }
}
