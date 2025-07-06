using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WixToolset.Dtf.WindowsInstaller;

namespace WixSharp.dialogs
{
    public class CustomSetupTypeDialog : AbstractCustomIntermediateDialog
    {
        private Button btnForMe;
        private Button btnForAll;
        private Button btnPortable;
        private Label lblForMe;
        private Label lblForAll;
        private Label lblPortable;
        private string selectedScope;

        public override string GetDialogName()
        {
            return "setupType";
        }

        public override string GetLocalizedSubTitle()
        {
            return "[SetupTypeDlgTitle]";
        }

        public override string GetLocalizedTitleDescription()
        {
            return "[SetupTypeDlgDescription]";
        }
        public override void CreateContent()
        {

            GetNextButton().Enabled = false;
            // 1) Instantiate buttons + labels

            btnForMe = new Button { Text = "[InstallScopeDlgPerUser]", AutoSize = true, Name = "btnForMe" };
            lblForMe = new Label { Text = "[InstallScopeDlgPerUserDescription]"};

            btnForAll = new Button { Text = "[InstallScopeDlgPerMachine]", AutoSize = true, Name = "btnForAll" };
            lblForAll = new Label { Text = "[InstallScopeDlgPerMachineDescription]"};

            btnPortable = new Button { Text = "[InstallScopeDlgPortable]", AutoSize = true, Name = "btnPortable" };
            lblPortable = new Label { Text = "[InstallScopeDlgPortableDescription]"};

            ConfigureWrappingLabel(lblForMe, GetFullContentWidth());
            ConfigureWrappingLabel(lblForAll, GetFullContentWidth());
            ConfigureWrappingLabel(lblPortable, GetFullContentWidth());

            // 2) Wire up clicks
            btnForMe.Click += (s, e) => Select(Constants.INSTALLATION_TYPE_USER);
            btnForAll.Click += (s, e) => Select(Constants.INSTALLATION_TYPE_SYSTEM);
            btnPortable.Click += (s, e) => Select(Constants.INSTALLATION_TYPE_PORTABLE);

            // 3) Build a single-column table: 6 rows (btn, lbl, btn, lbl, btn, lbl)
            var tbl = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 6,
                Dock = DockStyle.Top,
                AutoSize = true,
                // top padding to push first button ~20px below banner
                Padding = new Padding(0, 25, 0, 0)
            };

            // Make each row autosize
            for (int i = 0; i < 6; i++)
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Add rows: button then its label
            tbl.Controls.Add(btnForMe, 0, 0);
            tbl.Controls.Add(lblForMe, 0, 1);
            tbl.Controls.Add(btnForAll, 0, 2);
            tbl.Controls.Add(lblForAll, 0, 3);
            tbl.Controls.Add(btnPortable, 0, 4);
            tbl.Controls.Add(lblPortable, 0, 5);

            // 4) Tweak margins to add spacing
            btnForMe.Margin = new Padding(0, 0, 0, 5);
            lblForMe.Margin = new Padding(0, 0, 0, 15);
            btnForAll.Margin = new Padding(0, 0, 0, 5);
            lblForAll.Margin = new Padding(0, 0, 0, 15);
            btnPortable.Margin = new Padding(0, 0, 0, 5);
            lblPortable.Margin = new Padding(0, 0, 0, 0);

            // 5) Add to the text panel
            AddControlToTextPanel(tbl);
        }

        private void ConfigureWrappingLabel(Label lbl, int maxWidth)
        {
            lbl.AutoSize = true;                // let height grow
            lbl.MaximumSize = new Size(maxWidth, 0); // constrain width, unlimited height
            lbl.AutoEllipsis = false;               // ellipsis not needed when wrapping
            lbl.TextAlign = ContentAlignment.TopLeft;
        }

        private void Select(string scope)
        {
            // Save choice
            selectedScope = scope;

            // Highlight selection (simple visual feedback)
            btnForMe.FlatStyle = scope == Constants.INSTALLATION_TYPE_USER ? FlatStyle.Popup : FlatStyle.Standard;
            btnForAll.FlatStyle = scope == Constants.INSTALLATION_TYPE_SYSTEM ? FlatStyle.Popup : FlatStyle.Standard;
            btnPortable.FlatStyle = scope == Constants.INSTALLATION_TYPE_PORTABLE ? FlatStyle.Popup : FlatStyle.Standard;

            // Enable Next
            GetNextButton().Enabled = true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            bool unattendedInstallation = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.RESUME_INSTALLATION, out _);
            bool scopeDefined = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.INSTALLATION_TYPE, out selectedScope);
            if (unattendedInstallation && scopeDefined)
            {
                base.Runtime.Session[Constants.INSTALL_SCOPE_KEY] = selectedScope;
                if (selectedScope == Constants.INSTALLATION_TYPE_SYSTEM)
                {
                    Runtime.Session["OPEN_AS_ADMIN"] = "true";
                }
                base.Shell.GoNext();
            }
        }

        public override void NextClick(object sender, EventArgs e)
        {
            base.Runtime.Session[Constants.INSTALL_SCOPE_KEY] = selectedScope;
            Constants.AddSecureProperty(this.Session(), Constants.SecureProperties.INSTALLATION_TYPE, selectedScope);
            if (selectedScope == Constants.INSTALLATION_TYPE_SYSTEM)
            {
                Runtime.Session["OPEN_AS_ADMIN"] = "true";
            }

            base.Shell.GoNext();
        }

        public override void BackClick(object sender, EventArgs e)
        {
            base.Shell.GoPrev();
        }


    }
}
