using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WixSharp.UI.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace WixSharp.dialogs
{
    public class AdaptedInstallDirDialog : InstallDirDialog
    {
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            bool unattendedInstallation = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.RESUME_INSTALLATION, out _);
            if (unattendedInstallation)
            {
                string path;
                bool pathDefid = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.INSTALLATION_PATH, out path);
                if (pathDefid && !string.IsNullOrEmpty(path))
                {
                    Runtime.Session["INSTALLDIR"] = path;
                    // If unattended, skip this dialog
                    base.Shell.GoNext();
                    return;
                }
            }

            // 1) Build the new default folder based on your INSTALL_SCOPE_KEY
            var scope = Runtime.Session[Constants.INSTALL_SCOPE_KEY];
            string appName = Runtime.Session["ProductName"];
            string baseDir;

            switch (scope)
            {
                case Constants.INSTALLATION_TYPE_SYSTEM:
                    baseDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        appName);
                    break;

                case Constants.INSTALLATION_TYPE_USER:
                    baseDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        appName);
                    break;

                case Constants.INSTALLATION_TYPE_PORTABLE:
                    // For portable, default to current directory + app folder
                    var installerDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    baseDir = Path.Combine(installerDir, $"{appName}-Portable");
                    break;

                default:
                    // Fallback to MSI’s original default
                    baseDir = Runtime.Session["INSTALLDIR"];
                    break;
            }

            // 2) Overwrite the MSI property so upcoming actions see it
            Runtime.Session["INSTALLDIR"] = baseDir;

            // 3) Find the folder‐edit TextBox on the form and update its Text
            TextBox edit = this.GetAllControls()
                           .OfType<TextBox>()
                           .FirstOrDefault();
            if (edit != null)
            {
                edit.Text = baseDir;
                
            }

            string name = Runtime.Session[Constants.PRODUCT_NAME_KEY];
            string ver = Runtime.Session[Constants.PRODUCT_VERSION_KEY];
            this.Text = $"{name} {ver} Setup";
        }

      

    }
    // Extension helper to grab all controls, including nested panels
    static class FormExtensions
    {
        public static Control[] GetAllControls(this Control parent)
        {
            var controls = parent.Controls.Cast<Control>().ToList();
            for (int i = 0; i < controls.Count; i++)
            {
                controls.AddRange(controls[i].Controls.Cast<Control>());
            }
            return controls.ToArray();
        }
    }

}
