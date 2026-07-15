using System.Diagnostics;
using System.Windows;

namespace ScreenAutoClicker;

public partial class UpdateWindow : Window
{
    private readonly GitHubRelease _release;

    public UpdateWindow(GitHubRelease release)
    {
        InitializeComponent();

        _release = release;

        string current = UpdateService.CurrentVersion.ToString(3);
        string latest  = release.TagName.TrimStart('v', 'V');
        VersionText.Text = $"Trenutna različica:  v{current}    →    Nova različica:  v{latest}";

        ReleaseNotesText.Text = string.IsNullOrWhiteSpace(release.Body)
            ? "(Ni opisanih sprememb.)"
            : release.Body;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private async void Update_Click(object sender, RoutedEventArgs e)
    {
        UpdateBtn.IsEnabled  = false;
        CancelBtn.IsEnabled  = false;
        ProgressPanel.Visibility = Visibility.Visible;

        var progress = new Progress<(int Percent, string Message)>(report =>
        {
            UpdateProgressBar.Value = report.Percent;
            ProgressText.Text       = report.Message;
        });

        try
        {
            bool assetFound = await UpdateService.DownloadAndInstallAsync(_release, progress);

            if (!assetFound)
            {
                MessageBox.Show(
                    "V tej verziji ni najdene datoteke za prenos.\n" +
                    "Posodobitev prosim prenesi ročno z GitHub strani.",
                    "Posodobitev ni mogoča",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Open the GitHub release page so the user can download manually
                Process.Start(new ProcessStartInfo
                {
                    FileName        = _release.HtmlUrl,
                    UseShellExecute = true
                });

                RestoreButtons();
            }
            // If assetFound == true the updater script is running and the app will shut down.
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Napaka pri posodabljanju:\n{ex.Message}",
                "Napaka",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            RestoreButtons();
        }
    }

    private void RestoreButtons()
    {
        UpdateBtn.IsEnabled      = true;
        CancelBtn.IsEnabled      = true;
        ProgressPanel.Visibility = Visibility.Collapsed;
    }
}
