using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;

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

public class CardTableRow
{
    public string ContactName { get; set; } = "";
    public string Code { get; set; } = "";
    public string CardType { get; set; } = "";
    public bool IsActive { get; set; }
    public string StatusText => IsActive ? "Aktivna" : "Neaktivna";
    public DateTime? ValidToDate { get; set; }
    public string ValidTo { get; set; } = "";
}

public partial class MainWindow : Window
{
    private readonly ObservableCollection<LogEntryViewModel> _logEntries = new();
    private readonly ObservableCollection<CardTableRow> _table1Rows = new();
    private readonly ObservableCollection<CardTableRow> _table2Rows = new();
    private CancellationTokenSource? _cts;
    private readonly DispatcherTimer _clockTimer;
    private AutoClickerConfig? _config;
    private string _configPath = "config.xml";

    public MainWindow()
    {
        InitializeComponent();
        LogListBox.ItemsSource = _logEntries;
        Table1Grid.ItemsSource = _table1Rows;
        Table2Grid.ItemsSource = _table2Rows;

        Logger.OnLog += OnLogEntry;

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => ClockText.Text = DateTime.Now.ToString("d.M.yyyy HH:mm:ss");
        _clockTimer.Start();
        ClockText.Text = DateTime.Now.ToString("d.M.yyyy HH:mm:ss");

        LoadConfig();
    }

    private void LoadConfig()
    {
        _configPath = "config.xml";
        if (!File.Exists(_configPath)) return;
        try
        {
            _config = ConfigLoader.Load(_configPath);
            if (_config.UseDebugConfigFile)
            {
                string debugPath = "config-debug.xml";
                if (File.Exists(debugPath))
                {
                    _config = ConfigLoader.Load(debugPath);
                    _configPath = debugPath;
                }
                else
                {
                    Logger.Warn($"UseDebugConfigFile je vklučen, a '{debugPath}' ne obstaja. Uporaba '{_configPath}'.");
                }
            }
            Logger.Info($"Nastavitve naložene iz: {_configPath}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Napaka pri nalaganju konfiguracije: {ex.Message}");
            _config = null;
        }
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
            LoadConfig();
            if (_config == null)
            {
                Logger.Error("Konfiguracija ni na voljo. Preveri config.xml.");
                return;
            }
            var config = _config;

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

    private void TablesButton_Click(object sender, RoutedEventArgs e)
    {
        MainTabControl.SelectedIndex = 1;
    }

    private async void OpenTable1_Click(object sender, RoutedEventArgs e)
    {
        MainTabControl.SelectedIndex = 1;
        await LoadCardTableAsync(_table1Rows, Table1Status,
            new[] { "1x TEDENSKO 1h", "2x TEDENSKO 1h", "3x TEDENSKO 2h" });
    }

    private async void OpenTable2_Click(object sender, RoutedEventArgs e)
    {
        MainTabControl.SelectedIndex = 2;
        await LoadCardTableAsync(_table2Rows, Table2Status,
            new[] { "1x tedensko tečaj + mesečna odrasli", "1 X TEDENSKO  tečaj + MESEČNA ODRASLI" });
    }

    private async void RefreshTable1_Click(object sender, RoutedEventArgs e)
    {
        await LoadCardTableAsync(_table1Rows, Table1Status,
            new[] { "1x TEDENSKO 1h", "2x TEDENSKO 1h", "3x TEDENSKO 2h" });
    }

    private async void RefreshTable2_Click(object sender, RoutedEventArgs e)
    {
        await LoadCardTableAsync(_table2Rows, Table2Status,
            new[] { "1x tedensko tečaj + mesečna odrasli", "1 X TEDENSKO  tečaj + MESEČNA ODRASLI" });
    }

    private static readonly string[] Table1CardTypes =
        { "1x TEDENSKO 1h", "2x TEDENSKO 1h", "3x TEDENSKO 2h" };

    private async Task LoadCardTableAsync(
        ObservableCollection<CardTableRow> rows,
        System.Windows.Controls.TextBlock statusBlock,
        string[] cardTypes)
    {
        if (cardTypes.Length == 0)
        {
            rows.Clear();
            statusBlock.Text = "Ni konfiguriranih tipov kart.";
            return;
        }

        statusBlock.Text = "Nalagam...";
        rows.Clear();

        if (_config == null)
        {
            rows.Clear();
            statusBlock.Text = "Konfiguracija ni na voljo.";
            return;
        }
        var cfg = _config;

        var typeParams = Enumerable.Range(0, cardTypes.Length).Select(i => $"@t{i}");
        string inClause = string.Join(",", typeParams);
        string sql = $@"
            WITH Ranked AS (
                SELECT c.Contact, c.Code, tc.Name AS CardType,
                       CAST(CASE WHEN tc.Active=1 AND tsc.Active=1 AND tsc.DateTo >= GETDATE()
                                 THEN 1 ELSE 0 END AS BIT) AS IsActive,
                       tsc.DateTo,
                       ROW_NUMBER() OVER (
                           PARTITION BY tc.idContactUse, tc.Name
                           ORDER BY
                               CASE WHEN tc.Active=1 AND tsc.Active=1 AND tsc.DateTo >= GETDATE()
                                    THEN 0 ELSE 1 END,
                               tsc.DateTo DESC
                       ) AS rn
                FROM Contact c
                JOIN TaskCard tc ON tc.idContactUse = c.idContact
                JOIN TaskScheduleCard tsc ON tsc.idTaskCard = tc.idTaskCard
                WHERE tc.Name IN ({inClause})
            )
            SELECT Contact, Code, CardType, IsActive, DateTo
            FROM Ranked WHERE rn = 1
            ORDER BY IsActive DESC, Contact, CardType";

        try
        {
            await using var conn = new SqlConnection(cfg.DbConnectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            for (int i = 0; i < cardTypes.Length; i++)
                cmd.Parameters.Add(new SqlParameter($"@t{i}", System.Data.SqlDbType.NVarChar, 200)
                    { Value = cardTypes[i] });

            await using var reader = await cmd.ExecuteReaderAsync();
            var temp = new List<CardTableRow>();
            while (await reader.ReadAsync())
            {
                temp.Add(new CardTableRow
                {
                    ContactName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                    Code        = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    CardType    = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    IsActive    = !reader.IsDBNull(3) && reader.GetBoolean(3),
                    ValidToDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    ValidTo     = reader.IsDBNull(4) ? "" : reader.GetDateTime(4).ToString("dd.MM.yyyy")
                });
            }

            Dispatcher.Invoke(() =>
            {
                rows.Clear();
                foreach (var r in temp) rows.Add(r);
                int active = rows.Count(r => r.IsActive);
                statusBlock.Text = $"{rows.Count} zapisov  |  {active} aktivnih";
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => statusBlock.Text = $"Napaka: {ex.Message}");
            Logger.Error($"[Tabela] {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        Logger.OnLog -= OnLogEntry;
        _clockTimer.Stop();
        base.OnClosed(e);
    }
}
