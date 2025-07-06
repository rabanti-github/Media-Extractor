using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WixSharp.UI.Forms;

namespace WixSharp.dialogs
{
    public class AdaptedLicenseDialog : LicenceDialog
    {
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
