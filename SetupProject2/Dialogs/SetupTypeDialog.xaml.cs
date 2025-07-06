using System.Linq;
using System.Windows.Media.Imaging;
using WixSharp;
using WixSharp.UI.Forms;

using WixSharp.UI.WPF;

namespace SetupProject2
{
    /// <summary>
    /// The standard SetupTypeDialog.
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class SetupTypeDialog : WpfDialog, IWpfDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupTypeDialog" /> class.
        /// </summary>
        public SetupTypeDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            this.DataContext = model = new SetupTypeDialogModel { Host = ManagedFormHost };
            var name = this.Session()["ProductName"];
            var ver = this.Session()["ProductVersion"];
            this.DialogTitle = $"{name} {ver} Setup";
        }

        SetupTypeDialogModel model;

        void GoPrev_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.GoPrev();

        void GoNext_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.GoNext();

        void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.Cancel();

        void PerUser_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.PerUser();

        void PerMachine_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.PerMachine();

        void Portable_Click(object sender, System.Windows.RoutedEventArgs e)
            => model.Portable();
    }

    /// <summary>
    /// ViewModel for standard SetupTypeDialog.
    /// </summary>
    internal class SetupTypeDialogModel : NotifyPropertyChangedBase
    {
        public ManagedForm Host;

        ISession session => Host?.Runtime.Session;
        IManagedUIShell shell => Host?.Shell;

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

        public void PerUser()
        {
            if (shell != null)
            {
                // JumpToProgressDialog();
                Constants.AddSecureProperty(Host.Session(), Constants.SecureProperties.INSTALLATION_TYPE, Constants.INSTALLATION_TYPE_USER);
                //session[Constants.INSTALL_SCOPE_KEY] = Constants.INSTALLATION_TYPE_USER;
                shell.GoNext();
            }

        }

        public void Portable()
        {
            if (shell != null)
            {
                // mark all features to be installed
                // string[] names = session.Features.Select(x => x.Name).ToArray();
                // session["ADDLOCAL"] = names.JoinBy(",");

                //JumpToProgressDialog();
                Constants.AddSecureProperty(Host.Session(), Constants.SecureProperties.INSTALLATION_TYPE, Constants.INSTALLATION_TYPE_PORTABLE);
                //session[Constants.INSTALL_SCOPE_KEY] = Constants.INSTALLATION_TYPE_PORTABLE;
                session[Constants.INSTALL_SCOPE_KEY] = "portable";
                shell.GoNext();
            }
        }

        public void PerMachine()
        {
            if (shell != null)
            {
                //session[Constants.INSTALL_SCOPE_KEY] = Constants.INSTALLATION_TYPE_SYSTEM;
                Constants.AddSecureProperty(Host.Session(), Constants.SecureProperties.INSTALLATION_TYPE, Constants.INSTALLATION_TYPE_SYSTEM);
                session[Constants.OPEN_AS_ADMIN_KEY] = "true";
                shell.GoNext();
            }

                //      => shell?.GoNext(); // let the dialog flow through
        }


        public void GoPrev()
            => shell?.GoPrev();

        public void GoNext()
            => shell?.GoNext();

        public void Cancel()
            => shell?.Cancel();
    }
}