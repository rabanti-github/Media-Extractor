using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WixSharp;
using WixSharp.UI.Forms;

using WixSharp.UI.WPF;
using WixSharp.Utilities;
using WixToolset.Dtf.WindowsInstaller;

namespace SetupProject
{
    /// <summary>
    /// The standard WelcomeDialog.
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class LanguageSelectionDialog : WpfDialog, IWpfDialog
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeDialog" /> class.
        /// </summary>
        public LanguageSelectionDialog()
        {
           InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSharp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            Session wixSession = this.Session();
            string culture = null;

            bool languageDefined = Constants.GetSecureProperty(wixSession, Constants.SecureProperties.UI_LANGUAGE, out culture);
            SelectLanguage(culture ?? Constants.LANGUAGE_ENGLISH);
            this.DataContext = model = new LanguageSelectionDialogModel { Host = ManagedFormHost, SelectedLanguage = culture };
            var name = this.Session()["ProductName"];
            var ver = this.Session()["ProductVersion"];
            this.DialogTitle = $"{name} {ver} Setup";
           
            bool unattendedInstallation = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.RESUME_INSTALLATION, out _);
            if (unattendedInstallation)
            {
                bool legacyInstallationFound = LegacyDetector.HasLegacyInstallation();
                if (legacyInstallationFound)
                {
                    LegacyDetector.RemoveLegacyInstallation();
                }

                if (languageDefined)
                {
                    SelectLanguage(culture);
                }
                string installationPath;
                string installationType;
                bool installationPathDefined = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.TARGET_DIR, out installationPath);
                bool installationTypeDefined = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.INSTALLATION_TYPE, out installationType);
                if (installationPathDefined)
                {
                    this.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        this.Session()["INSTALLDIR"] = installationPath;
                        base.Shell.GoTo<ProgressDialog>();
                    }), DispatcherPriority.ApplicationIdle);
                }
                string desktopIcon, startMenuIcon, explorerRegistration;
                bool desktopIconDefined = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.ADD_DESKTOP_ICON, out desktopIcon);
                bool startMenuIconDefined = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.ADD_STARTMENU_ICON, out startMenuIcon);
                bool explorerRegistrationDefined = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.REGISTER_EXPLORER, out explorerRegistration);
                
            }
            else if (!unattendedInstallation)
            {
                int pid = Process.GetCurrentProcess().Id;
                Constants.AddSecureProperty(this.Session(), Constants.SecureProperties.SETUP_PID, pid.ToString());
            }

        }

        LanguageSelectionDialogModel model;

        void GoPrev_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.GoPrev();

        void GoNext_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectLanguage(model.SelectedLanguage);
            model.GoNext();
        }


        void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.Cancel();

        private void SelectLanguage(string selectedLanguage)
        {
            // 1) Set thread culture // TODO Auto-detect based on system settings or user selection
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);

            // 2) Reinitialize MSI UI text from correct .wxl
            MsiRuntime runtime = Shell.MsiRuntime();
            Session wixSession = this.Session();

            Constants.AddSecureProperty(wixSession, Constants.SecureProperties.UI_LANGUAGE, selectedLanguage);

            switch (selectedLanguage)
            {
                case Constants.LANGUAGE_GERMAN:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("de_wxl"));
                    break;

                case Constants.LANGUAGE_FRENCH:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("fr_wxl"));
                    break;

                case Constants.LANGUAGE_SPANISH:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("es_wxl"));
                    break;

                case Constants.LANGUAGE_JAPANESE:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("jp_wxl"));
                    break;

                default:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("en_wxl"));
                    break;
            }
        }

        private void WpfDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }
    }

    /// <summary>
    /// ViewModel for standard WelcomeDialog.
    /// </summary>
    internal class LanguageSelectionDialogModel : NotifyPropertyChangedBase
    {
        public bool EnChecked { get; set; }
        public bool FrChecked { get; set; }
        public bool JpChecked { get; set; }
        public bool EsChecked { get; set; }
        public bool DeChecked { get; set; }

        public ManagedForm Host;

        ISession session => Host?.Runtime?.Session;
        IManagedUIShell shell => Host?.Shell;

        public string SelectedLanguage 
        {
            get
            {
                if (DeChecked) { return Constants.LANGUAGE_GERMAN; }
                else if (FrChecked) { return Constants.LANGUAGE_FRENCH; }
                else if (EsChecked) { return Constants.LANGUAGE_SPANISH; }
                else if (JpChecked) { return Constants.LANGUAGE_JAPANESE; }
                else { return Constants.LANGUAGE_ENGLISH; }
            } 
            internal set 
            {   switch (value)
                {
                    case Constants.LANGUAGE_GERMAN:
                        DeChecked = true;
                        break;
                    case Constants.LANGUAGE_FRENCH:
                        FrChecked = true;
                        break;
                    case Constants.LANGUAGE_SPANISH:
                        EsChecked = true;
                        break;
                    case Constants.LANGUAGE_JAPANESE:
                        JpChecked = true;
                        break;
                    default:
                        EnChecked = true;
                        break;
                }
            } 
        }

        public BitmapImage Banner => session?.GetResourceBitmap("WixSharpUI_Bmp_Dialog")?.ToImageSource() ??
                                     session?.GetResourceBitmap("WixUI_Bmp_Dialog")?.ToImageSource();

        public void GoPrev()
            => shell?.GoPrev();

        public void GoNext()
            => shell?.GoNext();

        public void Cancel()
            => shell?.Cancel();
    }
}