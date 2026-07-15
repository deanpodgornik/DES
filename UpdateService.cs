using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Windows;

namespace ScreenAutoClicker;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("body")]
    public string Body { get; set; } = "";

    [JsonPropertyName("assets")]
    public List<GitHubAsset> Assets { get; set; } = [];
}

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = "";

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

public static class UpdateService
{
    private const string Owner = "deanpodgornik";
    private const string Repo  = "DES";
    private static readonly Uri LatestReleaseUri =
        new($"https://api.github.com/repos/{Owner}/{Repo}/releases/latest");

    private static readonly HttpClient Http = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"{Owner}/{Repo}-updater");
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

    /// <summary>
    /// Checks GitHub for a newer release.
    /// Returns the release if one is available, otherwise null.
    /// </summary>
    public static async Task<GitHubRelease?> CheckForUpdateAsync()
    {
        try
        {
            var release = await Http.GetFromJsonAsync<GitHubRelease>(LatestReleaseUri)
                                    .ConfigureAwait(false);
            if (release is null) return null;

            string versionStr = release.TagName.TrimStart('v', 'V');
            if (!Version.TryParse(versionStr, out var latest)) return null;

            return latest > CurrentVersion ? release : null;
        }
        catch
        {
            // Network errors, API errors, etc. — silently ignore.
            return null;
        }
    }

    /// <summary>
    /// Downloads the release asset, extracts it (if zip), launches an updater script
    /// that waits for this process to exit, copies the new files, and restarts the app.
    /// Returns false if the release has no downloadable asset.
    /// </summary>
    public static async Task<bool> DownloadAndInstallAsync(
        GitHubRelease release,
        IProgress<(int Percent, string Message)>? progress = null)
    {
        var zipAsset = release.Assets.FirstOrDefault(a =>
            a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

        var exeAsset = zipAsset is null
            ? release.Assets.FirstOrDefault(a =>
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            : null;

        if (zipAsset is null && exeAsset is null)
            return false;

        string tempDir = Path.Combine(Path.GetTempPath(), "DES_Update");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        if (zipAsset is not null)
        {
            string zipPath = Path.Combine(tempDir, zipAsset.Name);
            progress?.Report((0, "Prenašam posodobitev..."));

            await DownloadFileAsync(zipAsset.BrowserDownloadUrl, zipPath, zipAsset.Size, progress)
                .ConfigureAwait(false);

            progress?.Report((95, "Razpakiram..."));
            string extractDir = Path.Combine(tempDir, "extracted");
            ZipFile.ExtractToDirectory(zipPath, extractDir);

            progress?.Report((99, "Pripravljam posodobitev..."));
            LaunchZipUpdaterScript(extractDir);
        }
        else
        {
            string downloadPath = Path.Combine(tempDir, exeAsset!.Name);
            string targetExe = Process.GetCurrentProcess().MainModule?.FileName
                               ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                               "ScreenAutoClicker.exe");

            progress?.Report((0, "Prenašam posodobitev..."));
            await DownloadFileAsync(exeAsset.BrowserDownloadUrl, downloadPath, exeAsset.Size, progress)
                .ConfigureAwait(false);

            progress?.Report((99, "Pripravljam posodobitev..."));
            LaunchExeUpdaterScript(downloadPath, targetExe);
        }

        return true;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static async Task DownloadFileAsync(
        string url, string destPath, long knownSize,
        IProgress<(int Percent, string Message)>? progress)
    {
        using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                                       .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        long totalBytes = response.Content.Headers.ContentLength ?? knownSize;
        await using var src = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var dst = File.Create(destPath);

        var buffer = new byte[65536];
        long downloaded = 0;
        int read;
        while ((read = await src.ReadAsync(buffer).ConfigureAwait(false)) > 0)
        {
            await dst.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
            downloaded += read;
            if (totalBytes > 0)
            {
                int pct = (int)(downloaded * 94L / totalBytes);
                progress?.Report((pct, $"Prenašam...  {downloaded / 1024} kB / {totalBytes / 1024} kB"));
            }
        }
    }

    /// <summary>
    /// Launches a PowerShell script that waits for this process to exit, then copies
    /// the extracted zip contents into the app directory and restarts the exe.
    /// </summary>
    private static void LaunchZipUpdaterScript(string extractDir)
    {
        string appDir  = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
        string exePath = Process.GetCurrentProcess().MainModule?.FileName
                         ?? Path.Combine(appDir, "ScreenAutoClicker.exe");
        int    pid     = Environment.ProcessId;

        string scriptPath = Path.Combine(Path.GetTempPath(), "DES_updater.ps1");

        // Escape single quotes for PowerShell string literals
        string safeAppDir     = appDir.Replace("'", "''");
        string safeExtractDir = extractDir.Replace("'", "''");
        string safeExePath    = exePath.Replace("'", "''");

        string script = $$"""
            $appPid     = {{pid}}
            $appDir     = '{{safeAppDir}}'
            $extractDir = '{{safeExtractDir}}'
            $exePath    = '{{safeExePath}}'

            # Wait until the running app exits
            while (Get-Process -Id $appPid -ErrorAction SilentlyContinue) {
                Start-Sleep -Seconds 1
            }

            # If the zip had a single sub-folder, use that as the source
            $exe = Get-ChildItem $extractDir -Filter '*.exe' -Recurse -File | Select-Object -First 1
            $sourceDir = if ($exe) { $exe.DirectoryName } else { $extractDir }

            Copy-Item "$sourceDir\*" $appDir -Recurse -Force

            Start-Process $exePath
            Remove-Item $PSCommandPath -Force -ErrorAction SilentlyContinue
            """;

        File.WriteAllText(scriptPath, script);
        Process.Start(new ProcessStartInfo
        {
            FileName  = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -NoProfile -WindowStyle Hidden -File \"{scriptPath}\"",
            UseShellExecute  = true,
            WindowStyle      = ProcessWindowStyle.Hidden
        });

        Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
    }

    /// <summary>
    /// Launches a PowerShell script that waits for this process to exit, then replaces
    /// the exe with the downloaded file and restarts.
    /// </summary>
    private static void LaunchExeUpdaterScript(string downloadedExe, string targetExe)
    {
        int pid = Environment.ProcessId;
        string scriptPath = Path.Combine(Path.GetTempPath(), "DES_updater.ps1");

        string safeDownloaded = downloadedExe.Replace("'", "''");
        string safeTarget     = targetExe.Replace("'", "''");

        string script = $$"""
            $appPid     = {{pid}}
            $downloaded = '{{safeDownloaded}}'
            $target     = '{{safeTarget}}'

            while (Get-Process -Id $appPid -ErrorAction SilentlyContinue) {
                Start-Sleep -Seconds 1
            }

            Copy-Item $downloaded $target -Force
            Start-Process $target
            Remove-Item $PSCommandPath -Force -ErrorAction SilentlyContinue
            """;

        File.WriteAllText(scriptPath, script);
        Process.Start(new ProcessStartInfo
        {
            FileName  = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -NoProfile -WindowStyle Hidden -File \"{scriptPath}\"",
            UseShellExecute  = true,
            WindowStyle      = ProcessWindowStyle.Hidden
        });

        Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
    }
}
