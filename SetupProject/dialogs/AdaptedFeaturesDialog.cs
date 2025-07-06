using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WixSharp.UI.Forms;
using WixToolset.Dtf.WindowsInstaller;


namespace WixSharp.dialogs
{
    public class AdaptedFeaturesDialog : FeaturesDialog
    {
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            string name = Runtime.Session[Constants.PRODUCT_NAME_KEY];
            string ver = Runtime.Session[Constants.PRODUCT_VERSION_KEY];
            this.Text = $"{name} {ver} Setup";

            bool isunattended = LocalizeFeatureNames(this);
            if (isunattended)
            {
                // If unattended, skip this dialog
                base.Shell.GoNext();
            }

        }

        private static bool LocalizeFeatureNames(ManagedForm form)
        {
            Session wixSession = form.Session();
            bool unattendedInstallation = Constants.GetSecureProperty(wixSession, Constants.SecureProperties.RESUME_INSTALLATION, out _);

            // 1) Recurse controls
            void recurseControls(Control parent)
            {
                foreach (Control ctl in parent.Controls)
                {
                    if (ctl is TreeView tv)
                    {
                        tv.AfterCheck -= (s, e) => FeatureTree_AfterCheck(s, e, wixSession);
                        tv.AfterCheck += (s, e) => FeatureTree_AfterCheck(s, e, wixSession);
                        recurseNodes(tv.Nodes, form);
                    }

                    // Recurse children controls
                    if (ctl.HasChildren)
                    {
                        recurseControls(ctl);
                    }
                }
            }

            // 2) Recurse TreeNodeCollection
            void recurseNodes(TreeNodeCollection nodes, ManagedForm frm)
            {
                foreach (TreeNode node in nodes)
                {
                    if (node.Text.StartsWith("[") && node.Text.EndsWith("]"))
                    {
                        // Can we hook an event of node?
                        if (unattendedInstallation)
                        {
                            bool isChecked = GetCheckedState(node.Text, wixSession);
                            if (isChecked)
                            {
                                node.Checked = true;
                            }
                        }
                        string key = node.Text.Trim('[', ']');
                        node.Text = frm.Runtime.Localize(key);
                    }
                    // Recurse child nodes
                    if (node.Nodes.Count > 0)
                        recurseNodes(node.Nodes, frm);
                }
            }

            recurseControls(form);
            return unattendedInstallation;
        }

        private static void FeatureTree_AfterCheck(object sender, TreeViewEventArgs e, Session session)
        {
            TreeNode node = e.Node;
            if (node == null)
            {
                return;
            }

            // Map the TreeNode to one of our enum values
            string enumName = null;
            string tagOrText = node.Tag?.ToString() ?? node.Text;
            switch (tagOrText)
            {
                case Constants.INSTALLATION_FEATURE_DESKTOP:
                    enumName = Constants.InstallationFeatures.desktop.ToString();
                    break;
                case Constants.INSTALLATION_FEATURE_STARTMENU:
                    enumName = Constants.InstallationFeatures.start.ToString();
                    break;
                case Constants.INSTALLATION_FEATURE_QUICKLAUNCH:
                    enumName = Constants.InstallationFeatures.quicklaunch.ToString();
                    break;
                case Constants.INSTALLATION_FEATURE_EXPLORER:
                    enumName = Constants.InstallationFeatures.explorer.ToString();
                    break;
                default:
                    return;
            }

            // 1) Read existing features
            string featuresString;
            bool got = Constants.GetSecureProperty(
                session,
                Constants.SecureProperties.INSTALLATION_FEATURES,
                out featuresString);

            if (featuresString == null)
            {
                featuresString = "";
            }

            // 2) Split into a set (ignore empty entries)
            HashSet<string> featuresSet = new HashSet<string>(
                featuresString.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries),
                StringComparer.OrdinalIgnoreCase);

            bool isChecked = node.Checked;
            if (isChecked)
            {
                featuresSet.Add(enumName);
            }
            else
            {
                featuresSet.Remove(enumName);
            }

            // 3) Rebuild the pipe-separated string
            string newFeaturesValue = string.Join("|", featuresSet);

            // 4) Write it back as a secure property
            Constants.AddSecureProperty(
                session,
                Constants.SecureProperties.INSTALLATION_FEATURES,
                newFeaturesValue);
        }

        private static bool GetCheckedState(string featureId, Session session)
        {
            // 1) Map the UI featureId to our enum name
            string enumName = null;

            switch (featureId)
            {
                case Constants.INSTALLATION_FEATURE_DESKTOP:
                    enumName = Constants.InstallationFeatures.desktop.ToString();
                    break;
                case Constants.INSTALLATION_FEATURE_STARTMENU:
                    enumName = Constants.InstallationFeatures.start.ToString();
                    break;
                case Constants.INSTALLATION_FEATURE_QUICKLAUNCH:
                    enumName = Constants.InstallationFeatures.quicklaunch.ToString();
                    break;
                case Constants.INSTALLATION_FEATURE_EXPLORER:
                    enumName = Constants.InstallationFeatures.explorer.ToString();
                    break;
                default:
                    // Unknown feature
                    return false;
            }

            // 2) Read the INSTALLATION_FEATURES property
            string featuresString;
            bool hasProp = Constants.GetSecureProperty(session,Constants.SecureProperties.INSTALLATION_FEATURES, out featuresString);

            if (hasProp == false || string.IsNullOrEmpty(featuresString))
            {
                // Nothing selected yet
                return false;
            }

            // 3) Split and check membership
            HashSet<string> selected = featuresString
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return selected.Contains(enumName);
        }

    }
}
