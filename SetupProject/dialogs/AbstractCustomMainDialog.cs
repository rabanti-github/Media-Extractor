using System;
using System.ComponentModel;
using System.Windows.Forms;
using WixSharp.UI.Forms;

namespace WixSharp.dialogs
{
    public abstract class AbstractCustomMainDialog : ManagedForm, IManagedDialog, IDialog
    {
        // Top‐level containers
        private Panel imgPanel;
        private Panel textPanel;
        private PictureBox image;

        // Bottom button bar
        private Panel bottomPanel;
        private Panel bottomBorder;
        private TableLayoutPanel tableLayoutPanel1;
        private Button back;
        private Button next;
        private Button cancel;

        private Label labelHeader;

        public Button GetNextButton()
        {
            return next;
        }

        public Button GetBackButton()
        {
            return back;
        }

        public abstract string GetLocalizedTitle();
        public abstract string GetDialogName();

        public abstract void CreateContent();

        public abstract void NextClick(object sender, EventArgs e);
        public abstract void BackClick(object sender, EventArgs e);

        public void AddControlToTextPanel(Control control)
        {
            if (textPanel != null)
            {
                textPanel.Controls.Add(control);
            }
            else
            {
                throw new InvalidOperationException("Text panel is not initialized.");
            }
        }

        public void AddLabelHeader(string title)
        {
            labelHeader = new Label();
            labelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelHeader.BackColor = System.Drawing.Color.Transparent;
            labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            labelHeader.Location = new System.Drawing.Point(20, 14);
            labelHeader.Name = "labelHeader";
            labelHeader.Size = new System.Drawing.Size(317, 30);
            labelHeader.TabIndex = 1;
            labelHeader.Text = title;
            AddControlToTextPanel(labelHeader);
        }


        public AbstractCustomMainDialog()
        {
            InitializeComponent();
        }


        private void cancel_Click(object sender, EventArgs e)
        {
            base.Shell.Cancel();
        }

        private void AbstractCustomMainDialog_Load(object sender, EventArgs e)
        {
            image.Image = base.Runtime.Session.GetResourceBitmap("WixUI_Bmp_Dialog") ?? base.Runtime.Session.GetResourceBitmap("WixSharpUI_Bmp_Dialog");
            if (image.Image != null)
            {
                ResetLayout();
            }
        }

        private void InitializeComponent()
        {
            // Instantiate controls
            this.imgPanel = new Panel();
            this.textPanel = new Panel();
            this.image = new PictureBox();
            this.bottomPanel = new Panel();
            this.bottomBorder = new Panel();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.back = new Button();
            this.next = new Button();
            this.cancel = new Button();

            this.imgPanel.SuspendLayout();
            this.textPanel.SuspendLayout();
            ((ISupportInitialize)(this.image)).BeginInit();  // ← BeginInit here
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();

            // imgPanel
            this.imgPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.imgPanel.Controls.Add(this.textPanel);
            this.imgPanel.Controls.Add(this.image);
            this.imgPanel.Location = new System.Drawing.Point(0, 0);
            this.imgPanel.Name = "imgPanel";
            this.imgPanel.Size = new System.Drawing.Size(494, 312);
            this.imgPanel.TabIndex = 4;

            // image (PictureBox – banner slot)
            this.image.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.image.BackColor = System.Drawing.Color.White;
            this.image.Location = new System.Drawing.Point(0, 0);
            this.image.Name = "image";
            this.image.Size = new System.Drawing.Size(156, 312);
            this.image.SizeMode = PictureBoxSizeMode.StretchImage;
            this.image.TabIndex = 0;
            this.image.TabStop = false;

            // textPanel
            this.textPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.textPanel.Location = new System.Drawing.Point(162, 12);
            this.textPanel.Name = "textPanel";
            this.textPanel.Size = new System.Drawing.Size(326, 294);
            this.textPanel.TabIndex = 4;
            CreateContent();  // Call to create custom content

            // bottomPanel
            this.bottomPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
            this.bottomPanel.Controls.Add(this.bottomBorder);
            this.bottomPanel.Controls.Add(this.tableLayoutPanel1);
            this.bottomPanel.Location = new System.Drawing.Point(0, 312);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(494, 49);
            this.bottomPanel.TabIndex = 1;

            // bottomBorder
            this.bottomBorder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.bottomBorder.BorderStyle = BorderStyle.FixedSingle;
            this.bottomBorder.Location = new System.Drawing.Point(0, 0);
            this.bottomBorder.Name = "bottomBorder";
            this.bottomBorder.Size = new System.Drawing.Size(494, 1);
            this.bottomBorder.TabIndex = 5;

            // tableLayoutPanel1 (Back, Next, Cancel)
            this.tableLayoutPanel1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14F));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.back, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.next, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.cancel, 4, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(491, 41);
            this.tableLayoutPanel1.TabIndex = 6;

            // back
            this.back.Anchor = AnchorStyles.Right;
            this.back.AutoSize = true;
            this.back.Enabled = false;  // disabled on first page
            this.back.Location = new System.Drawing.Point(222, 9);
            this.back.MinimumSize = new System.Drawing.Size(75, 0);
            this.back.Name = "back";
            this.back.Size = new System.Drawing.Size(77, 23);
            this.back.TabIndex = 0;
            this.back.Text = "[WixUIBack]";
            this.back.UseVisualStyleBackColor = true;
            this.back.Click += new EventHandler(this.BackClick);

            // next
            this.next.Anchor = AnchorStyles.Right;
            this.next.AutoSize = true;
            this.next.Location = new System.Drawing.Point(305, 9);
            this.next.MinimumSize = new System.Drawing.Size(75, 0);
            this.next.Name = "next";
            this.next.Size = new System.Drawing.Size(77, 23);
            this.next.TabIndex = 0;
            this.next.Text = "[WixUINext]";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new EventHandler(this.NextClick);

            // cancel
            this.cancel.Anchor = AnchorStyles.Right;
            this.cancel.AutoSize = true;
            this.cancel.Location = new System.Drawing.Point(402, 9);
            this.cancel.MinimumSize = new System.Drawing.Size(75, 0);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(86, 23);
            this.cancel.TabIndex = 0;
            this.cancel.Text = "[WixUICancel]";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new EventHandler(this.cancel_Click);

            // (final settings)
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(494, 361);
            this.Controls.Add(this.imgPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = GetDialogName();
            this.Text = GetLocalizedTitle();
            this.Load += new EventHandler(this.AbstractCustomMainDialog_Load);

            // EndInit for PictureBox
            ((ISupportInitialize)(this.image)).EndInit();  // ← EndInit here

            this.imgPanel.ResumeLayout(false);
            this.textPanel.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }

        protected void ResetLayout()
        {
            int num = (int)((double)next.Height * 2.3);
            int num2 = num - bottomPanel.Height;
            bottomPanel.Top -= num2;
            bottomPanel.Height = num;
            imgPanel.Height = base.ClientRectangle.Height - bottomPanel.Height;
            float num3 = (float)image.Image.Width / (float)image.Image.Height;
            image.Width = (int)((float)image.Height * num3);
        }
    }
}
