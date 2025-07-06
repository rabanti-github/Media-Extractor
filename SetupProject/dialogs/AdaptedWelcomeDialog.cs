using System;
using System.Reflection;
using System.Windows.Forms;
using WixSharp.UI.Forms;

namespace WixSharp.dialogs
{
    public partial class AdaptedWelcomeDialog : WelcomeDialog
    {
        public AdaptedWelcomeDialog() : base()
        {
            var backField = typeof(WelcomeDialog).GetField("back", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backField != null)
            {
                var backButton = backField.GetValue(this) as Button;
                if (backButton != null)
                {
                    backButton.Enabled = true;
                }
            }
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            string name = Runtime.Session[Constants.PRODUCT_NAME_KEY];
            string ver = Runtime.Session[Constants.PRODUCT_VERSION_KEY];
            this.Text = $"{name} {ver} Setup";

            bool unattendedInstallation = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.RESUME_INSTALLATION, out _);
            if (unattendedInstallation)
            {
                base.Shell.GoNext();
            }
        }
    }
}
