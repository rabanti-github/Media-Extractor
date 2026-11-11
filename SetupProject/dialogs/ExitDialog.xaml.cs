using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WixSharp;
using WixSharp.UI.Forms;
using WixSharp.UI.WPF;
using IO = System.IO;

namespace SetupProject
{
    /// <summary>
    /// Data required after MSI execution has completed. The live MSI session is
    /// no longer safe to query when the exit dialog is created.
    /// </summary>
    internal static class InstallerUiState
    {
        public static string ProductName { get; private set; }
        public static string ProductVersion { get; private set; }
        public static string LogFile { get; private set; }
        public static BitmapImage ExitBanner { get; private set; }

        public static void Capture(ISession session)
        {
            if (session == null)
                return;

            ProductName = session["ProductName"];
            ProductVersion = session["ProductVersion"];
            LogFile = session.LogFile;

            try
            {
                ExitBanner = session.GetResourceBitmap("WixSharpUI_Bmp_Dialog").ToImageSource() ??
                             session.GetResourceBitmap("WixUI_Bmp_Dialog").ToImageSource();

                if (ExitBanner?.CanFreeze == true)
                    ExitBanner.Freeze();
            }
            catch
            {
                // The completion page remains usable if an optional bitmap is
                // unavailable. Never let decorative UI abort the installation.
                ExitBanner = null;
            }
        }
    }

    /// <summary>
    /// The standard ExitDialog.
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ExitDialog : WpfDialog, IWpfDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExitDialog"/> class.
        /// </summary>
        public ExitDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            // MSI execution has finished here; do not query Runtime.Session.
            if (!string.IsNullOrEmpty(InstallerUiState.ProductName))
                DialogTitle = $"{InstallerUiState.ProductName} {InstallerUiState.ProductVersion} Setup";

            UpdateTitles();

            DataContext = model = new ExitDialogModel { Host = ManagedFormHost };
        }

        ExitDialogModel model;

        /// <summary>
        /// Updates the titles of the dialog depending on the success of the installation action.
        /// </summary>
        public void UpdateTitles()
        {
            if (Shell.UserInterrupted || Shell.Log.Contains("User canceled installation."))
            {
                DialogTitleLabel.Text = "[UserExitTitle]";
                DialogDescription.Text = "[UserExitDescription1]";
            }
            else if (Shell.ErrorDetected)
            {
                DialogTitleLabel.Text = "[FatalErrorTitle]";
                DialogDescription.Text = Shell.CustomErrorDescription ?? "[FatalErrorDescription1]";
            }

            // `Localize` resolves [...] titles and descriptions into the localized strings stored in MSI resources tables
            this.Localize();
        }

        void ViewLog_Click(object sender, RoutedEventArgs e)
            => model.ViewLog();

        void GoExit_Click(object sender, RoutedEventArgs e)
            => model.GoExit();

        void Cancel_Click(object sender, RoutedEventArgs e)
            => model.Cancel();
    }

    /// <summary>
    /// ViewModel for standard ExitDialog.
    /// </summary>
    internal class ExitDialogModel
    {
        public ManagedForm Host { get; set; }
        IManagedUIShell shell => Host?.Shell;

        public BitmapImage Banner => InstallerUiState.ExitBanner;

        public void GoExit()
            => shell?.Exit();

        public void Cancel()
            => shell?.Exit();

        public void ViewLog()
        {
            if (shell != null)
                try
                {
                    string logFile = InstallerUiState.LogFile;

                    if (logFile.IsEmpty())
                    {
                        string wixSharpDir = Path.GetTempPath().PathCombine("WixSharp");

                        if (!Directory.Exists(wixSharpDir))
                            Directory.CreateDirectory(wixSharpDir);

                        string productName = InstallerUiState.ProductName ?? "Installer";
                        logFile = wixSharpDir.PathCombine(productName + ".log");
                        IO.File.WriteAllText(logFile, shell.Log);
                    }
                    Process.Start("notepad.exe", logFile);
                }
                catch
                {
                    // Catch all, we don't want the installer to crash in an
                    // attempt to view the log.
                }
        }
    }
}
