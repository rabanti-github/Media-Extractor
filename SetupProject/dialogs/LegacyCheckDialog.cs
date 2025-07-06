using System;
using System.Windows.Forms;
using WixSharp.logic;

namespace WixSharp.dialogs
{
    public class LegacyCheckDialog : AbstractCustomIntermediateDialog
    {
        private Label lblStatus;
        private Label lblStatus2;
        private bool legacyFound;


        public override string GetDialogName()
        {
            return "LegacyCheckDialog";
        }

        public override string GetLocalizedSubTitle()
        {
            return "[LegacyCheck_Title]";
        }

        public override string GetLocalizedTitleDescription()
        {
            return "[LegacyCheck_TitleDescription]";
        }

        public override void CreateContent()
        {
            lblStatus = new Label
            {
                Text = "[ProgressDlgTextChecking]",
                AutoSize = false,
                Location = new System.Drawing.Point(0, 20),
                Width = GetFullContentWidth(),
                Height = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F),
                Name = "lblStatus"
            };

            lblStatus2 = new Label
            {
                Text = "[previousInstallationWasDetected]",
                AutoSize = false,
                Location = new System.Drawing.Point(0, 20),
                Width = GetFullContentWidth(), // Set a width to prevent wrapping
                Height = 400,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F),
                Name = "lblStatus2",
                Visible = false  // Initially hidden, will be shown if legacy install is found
            };
            AddControlToTextPanel(lblStatus);
            AddControlToTextPanel(lblStatus2);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Disable Next until the check completes
            GetNextButton().Enabled = false;
            GetBackButton().Enabled = true;

            // Run the legacy‐check asynchronously so the UI can finish drawing
            this.BeginInvoke((MethodInvoker)(() =>
            {
                legacyFound = LegacyDetector.HasLegacyInstallation();
                if (!legacyFound)
                {
                    // No old install found: skip this dialog immediately
                    Shell.GoNext();
                }
                else
                {
                    // Old install found: update message and enable Next
                    lblStatus.Visible = false;
                    lblStatus2.Visible = true;
                    GetNextButton().Enabled = true;
                }
            }));

            bool unattendedInstallation = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.RESUME_INSTALLATION, out _);
            if (unattendedInstallation)
            {
                if (legacyFound)
                {
                    LegacyDetector.RemoveLegacyInstallation();
                }
                base.Shell.GoNext();
            }
        }

        public override void NextClick(object sender, EventArgs e)
        {
            if (legacyFound)
            {
               LegacyDetector.RemoveLegacyInstallation();
            }
            base.Shell.GoNext();
        }

        public override void BackClick(object sender, EventArgs e)
        {
            base.Shell.GoPrev();
        }
    }
}
