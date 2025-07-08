using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WixSharp;
using WixSharp.UI.Forms;

using WixSharp.UI.WPF;
using WixSharp.Utilities;

namespace SetupProject
{
    /// <summary>
    /// The standard SetupTypeDialog.
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class LegacyCheckDialog : WpfDialog, IWpfDialog
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyCheckDialog" /> class.
        /// </summary>
        public LegacyCheckDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            this.DataContext = model = new LegacyCheckDialogModel { Host = ManagedFormHost };
            var name = this.Session()["ProductName"];
            var ver = this.Session()["ProductVersion"];
            this.DialogTitle = $"{name} {ver} Setup";

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

        }

        LegacyCheckDialogModel model;

        void GoPrev_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.GoPrev();

        void GoNext_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.GoNext();

        void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.Cancel();

        /// <summary>
        /// ViewModel for standard SetupTypeDialog.
        /// </summary>
        internal class LegacyCheckDialogModel : NotifyPropertyChangedBase
        {
            public ManagedForm Host;

            ISession session => Host?.Runtime.Session;
            IManagedUIShell shell => Host?.Shell;

            public bool ShowCheckText { get; set; } = true;
            public bool LegacyFound { get; set; } = false;

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
                if (LegacyFound)
                {
                    LegacyDetector.RemoveLegacyInstallation();
                }
                shell?.GoNext();
            }

            public void Cancel()
                => shell?.Cancel();
        }
    }
}