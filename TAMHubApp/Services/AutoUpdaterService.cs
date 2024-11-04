using AutoUpdaterDotNET;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace TAMHubApp.Services
{
    public static class AutoUpdaterService
    {
        private static Version GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version;
        }
        public static void CheckForUpdate()
        {
            AutoUpdater.UpdateMode = Mode.Normal;
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.ReportErrors = true;
            AutoUpdater.InstalledVersion = GetCurrentVersion();
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var settingValue = Properties.Settings.Default.GitHubUpdateVersionFile;
                if (!string.IsNullOrEmpty(settingValue))
                {
                    AutoUpdater.Start(settingValue);
                }
            }));
        }


        private static void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    DialogResult dialogResult;
                    if (args.Mandatory.Value)
                    {
                        dialogResult =
                            System.Windows.Forms.MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is required update. Press Ok to begin updating the application.", @"Update Available",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    else
                    {
                        dialogResult =
                            System.Windows.Forms.MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. Do you want to update the application now?", @"Update Available",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                    }

                    // Uncomment the following line if you want to show standard update dialog instead.
                    // AutoUpdater.ShowUpdateForm(args);

                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                Properties.Settings.Default.IsMigrateDb = true;
                                Properties.Settings.Default.Save();
                                Properties.Settings.Default.Reload();
                                //Application.Exit();
                                System.Windows.Application.Current.Shutdown();
                            }
                        }
                        catch (Exception exception)
                        {
                            System.Windows.Forms.MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    //MessageBox.Show(@"There is no update available please try again later.", @"No update available",
                    //    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Windows.Forms.MessageBox.Show(
                        @"There is no update available please try again later.",
                        @"Dont Have Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (args.Error is WebException)
                {
                    System.Windows.Forms.MessageBox.Show(
                        @"There is a problem reaching update server. Please check your internet connection and try again later.",
                        @"Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(args.Error.Message,
                        args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
