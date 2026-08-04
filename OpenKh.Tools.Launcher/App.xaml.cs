using System.Windows;

namespace OpenKh.Tools.Launcher;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (LegacyInstallationMigration.TryStartModManager())
        {
            Shutdown();
            return;
        }

        LegacyInstallationMigration.ScheduleCleanupIfNeeded();
        new MainWindow().Show();
    }
}
