using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WixSharp;
using WixSharp.UI.Forms;

using WixSharp.UI.WPF;
using WixSharp.Utilities;
using WixToolset.Dtf.WindowsInstaller;

namespace SetupProject2
{
    /// <summary>
    /// The standard SetupTypeDialog.
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class PortableInstallationDialog : WpfDialog, IWpfDialog
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PortableInstallationDialog" /> class.
        /// </summary>
        public PortableInstallationDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            this.DataContext = model = new PortableInstallationDialogModel { Host = ManagedFormHost };
            var name = this.Session()["ProductName"];
            var ver = this.Session()["ProductVersion"];
            this.DialogTitle = $"{name} {ver} Setup";

            string installDir;
            Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.TARGET_DIR, out installDir);
            string sourceDir = this.Session().Property("WixSourceDir");
            string fileListPath = Path.Combine(sourceDir, Constants.INSTALLER_FILE_LIST);
            foreach (string relativePath in System.IO.File.ReadAllLines(fileListPath))
            {
                if (relativePath.Trim().Length == 0 || relativePath.StartsWith("#"))
                {
                    continue; // skip empty lines / comments
                }
                string source = Path.Combine(sourceDir, relativePath);
                string target = Path.Combine(installDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                System.IO.File.Copy(source, target, true);
            }
            /*
            bool legacyFound = LegacyDetector.HasLegacyInstallation();
            if (!legacyFound)
            {
                // No old install found: skip this dialog immediately
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    this.Shell.GoNext();
                }), DispatcherPriority.ApplicationIdle);
            }
            else
            {
                // Old install found: update message and enable Next
                model.ShowCheckText = false;
                model.LegacyFound = true;
            }
            */

        }

        PortableInstallationDialogModel model;

        void GoPrev_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.GoPrev();

        void GoNext_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.GoNext();

        void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.Cancel();

        /// <summary>
        /// ViewModel for standard SetupTypeDialog.
        /// </summary>
        internal class PortableInstallationDialogModel : NotifyPropertyChangedBase
        {
            private string currentAction = "";
            private int progressValue = 0;

            public ManagedForm Host;

            ISession session => Host?.Runtime.Session;
            IManagedUIShell shell => Host?.Shell;

            public string CurrentAction { get => currentAction; set { currentAction = value; base.NotifyOfPropertyChange(nameof(CurrentAction)); } }
            public int ProgressValue { get => progressValue; set { progressValue = value; base.NotifyOfPropertyChange(nameof(ProgressValue)); } }

            public BitmapImage Banner => session?.GetResourceBitmap("WixSharpUI_Bmp_Banner").ToImageSource() ??
                                     session?.GetResourceBitmap("WixUI_Bmp_Banner").ToImageSource();

            /// <summary>
            /// Initializes a new instance of the <see cref="SetupTypeDialog" /> class.
            /// </summary>
            void JumpToProgressDialog()
            {
                int index = shell.Dialogs.IndexOfDialogImplementing<IProgressDialog>();
                if (index != -1)
                {
                    shell.GoTo(index);
                }
                else
                {
                    shell.GoNext(); // if user did not supply progress dialog then simply go next
                }
            }


            public void GoPrev()
                => shell?.GoPrev();

            public void GoNext()
            {
                /*
                if (LegacyFound)
                {
                    LegacyDetector.RemoveLegacyInstallation();
                }
                */
                shell?.GoNext();
            }

            public void Cancel()
                => shell?.Cancel();
        }
    }
}