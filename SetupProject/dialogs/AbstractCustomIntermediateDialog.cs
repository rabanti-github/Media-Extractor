using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WixSharp.UI.Forms;

namespace WixSharp.dialogs
{
    public abstract class AbstractCustomIntermediateDialog : ManagedForm, IManagedDialog, IDialog
    {
        private PictureBox banner;
        private Panel topPanel;
        private Label label2;
        private Label label1;
        private Panel bottomPanel;
        private Panel border1;
        private Panel topBorder;
        private TableLayoutPanel tableLayoutPanel1;
        private Button back;
        private Button next;
        private Button cancel;
        private Panel middlePanel;

        public Button GetNextButton()
        {
            return next;
        }

        public Button GetBackButton()
        {
            return back;
        }
        public abstract string GetDialogName();
        public abstract string GetLocalizedSubTitle();
        public abstract string GetLocalizedTitleDescription();
        public abstract void CreateContent();
        public abstract void NextClick(object sender, EventArgs e);
        public abstract void BackClick(object sender, EventArgs e);

        public int GetFullContentWidth()
        {
            return 474;
        }

        public AbstractCustomIntermediateDialog()
        {
            InitializeComponent();
            label1.MakeTransparentOn(banner);
            label2.MakeTransparentOn(banner);
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            string name = Runtime.Session[Constants.PRODUCT_NAME_KEY];
            string ver = Runtime.Session[Constants.PRODUCT_VERSION_KEY];
            this.Text = $"{name} {ver} Setup";
        }

        public void AddControlToTextPanel(Control control)
        {
            if (middlePanel != null)
            {
                middlePanel.Controls.Add(control);
            }
            else
            {
                throw new InvalidOperationException("Middle panel is not initialized.");
            }
        }

        private void AbstractCustomIntermediateDialog_Load(object sender, EventArgs e)
        {
            banner.Image = base.Runtime.Session.GetResourceBitmap("WixUI_Bmp_Banner") ?? base.Runtime.Session.GetResourceBitmap("WixSharpUI_Bmp_Banner");
            if (banner.Image != null)
            {
                ResetLayout();
            }
        }

        private void ResetLayout()
        {
            float num = (float)banner.Image.Width / (float)banner.Image.Height;
            topPanel.Height = (int)((float)banner.Width / num);
            topBorder.Top = topPanel.Height + 1;
            int num2 = (int)((double)next.Height * 2.3) - bottomPanel.Height;
            bottomPanel.Top -= num2;
            bottomPanel.Height += num2;
            middlePanel.Top = topBorder.Bottom + 5;
            middlePanel.Height = bottomPanel.Top - 5 - middlePanel.Top;
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            base.Shell.Cancel();
        }

        private void InitializeComponent()
        {
            this.middlePanel = new Panel();
            this.topBorder = new Panel();
            this.topPanel = new Panel();
            this.label2 = new Label();
            this.label1 = new Label();
            this.banner = new PictureBox();
            this.bottomPanel = new Panel();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.back = new Button();
            this.next = new Button();
            this.cancel = new Button();
            this.border1 = new Panel();

            this.middlePanel.SuspendLayout();
            this.topPanel.SuspendLayout();
            ((ISupportInitialize)this.banner).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            base.SuspendLayout();

            this.middlePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.middlePanel.Location = new System.Drawing.Point(14, 65);
            this.middlePanel.Name = "middlePanel";
            this.middlePanel.Size = new System.Drawing.Size(GetFullContentWidth(), 241);
            this.middlePanel.TabIndex = 12;
            CreateContent();   // Call to create custom content

            this.topBorder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.topBorder.BorderStyle = BorderStyle.FixedSingle;
            this.topBorder.Location = new System.Drawing.Point(0, 58);
            this.topBorder.Name = "topBorder";
            this.topBorder.Size = new System.Drawing.Size(494, 1);
            this.topBorder.TabIndex = 11;
            this.topPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.topPanel.BackColor = System.Drawing.SystemColors.Control;
            this.topPanel.Controls.Add(this.label2);
            this.topPanel.Controls.Add(this.label1);
            this.topPanel.Controls.Add(this.banner);
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(494, 58);
            this.topPanel.TabIndex = 7;

            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(18, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = GetLocalizedTitleDescription();

            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label1.Location = new System.Drawing.Point(10, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = GetLocalizedSubTitle();

            this.banner.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.banner.BackColor = System.Drawing.Color.White;
            this.banner.Location = new System.Drawing.Point(0, 0);
            this.banner.Name = "banner";
            this.banner.Size = new System.Drawing.Size(494, 58);
            this.banner.SizeMode = PictureBoxSizeMode.StretchImage;
            this.banner.TabIndex = 0;
            this.banner.TabStop = false;

            this.bottomPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
            this.bottomPanel.Controls.Add(this.tableLayoutPanel1);
            this.bottomPanel.Controls.Add(this.border1);
            this.bottomPanel.Location = new System.Drawing.Point(0, 312);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(494, 49);
            this.bottomPanel.TabIndex = 6;

            this.tableLayoutPanel1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14f));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.back, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.next, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.cancel, 4, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(491, 46);
            this.tableLayoutPanel1.TabIndex = 7;

            this.back.Anchor = AnchorStyles.Right;
            this.back.AutoSize = true;
            this.back.Location = new System.Drawing.Point(222, 11);
            this.back.MinimumSize = new System.Drawing.Size(75, 0);
            this.back.Name = "back";
            this.back.Size = new System.Drawing.Size(77, 23);
            this.back.TabIndex = 0;
            this.back.Text = "[WixUIBack]";
            this.back.UseVisualStyleBackColor = true;
            this.back.Click += new EventHandler(this.BackClick);

            this.next.Anchor = AnchorStyles.Right;
            this.next.AutoSize = true;
            this.next.Location = new System.Drawing.Point(305, 11);
            this.next.MinimumSize = new System.Drawing.Size(75, 0);
            this.next.Name = "next";
            this.next.Size = new System.Drawing.Size(77, 23);
            this.next.TabIndex = 0;
            this.next.Text = "[WixUINext]";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new EventHandler(this.NextClick);

            this.cancel.Anchor = AnchorStyles.Right;
            this.cancel.AutoSize = true;
            this.cancel.Location = new System.Drawing.Point(402, 11);
            this.cancel.MinimumSize = new System.Drawing.Size(75, 0);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(86, 23);
            this.cancel.TabIndex = 0;
            this.cancel.Text = "[WixUICancel]";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(cancel_Click);

            this.border1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.border1.BorderStyle = BorderStyle.FixedSingle;
            this.border1.Location = new System.Drawing.Point(0, 0);
            this.border1.Name = "border1";
            this.border1.Size = new System.Drawing.Size(494, 1);
            this.border1.TabIndex = 10;

            base.ClientSize = new System.Drawing.Size(494, 361);
            base.Controls.Add(this.middlePanel);
            base.Controls.Add(this.topBorder);
            base.Controls.Add(this.topPanel);
            base.Controls.Add(this.bottomPanel);
            base.KeyPreview = true;

            this.Name = GetDialogName();
            base.Load += new EventHandler(AbstractCustomIntermediateDialog_Load);
            this.middlePanel.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            ((ISupportInitialize)this.banner).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            base.ResumeLayout(false);
        }

    }
}
