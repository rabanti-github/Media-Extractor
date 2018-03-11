using System;
using System.Windows;
using System.Windows.Input;

namespace MediaExtractor
{
    /// <summary>
    /// Logic of the ExistingFileDialog Class
    /// </summary>
    public partial class ExistingFileDialog : Window
    {
        /// <summary>
        /// Enum of possible dialog results
        /// </summary>
        public enum Result
        {
            /// <summary>
            /// Extraction of the item will be skipped
            /// </summary>
            Skip,
            /// <summary>
            /// The existing file will be overwritten
            /// </summary>
            Overwrite,
            /// <summary>
            /// The new file will be renamed
            /// </summary>
            Rename,
            /// <summary>
            /// The extraction will be canceled
            /// </summary>
            Cancel,
            /// <summary>
            /// No action / Default
            /// </summary>
            None,
        }

        /// <summary>
        /// Static dialog result
        /// </summary>
        public static Result DialogResult = Result.None;
        /// <summary>
        /// Boolean indicates whether the dialog shall be reoccurring (false) or be skipped with the last decision as default result (true)
        /// </summary>
        public static bool? RemeberDecision;

        /// <summary>
        /// Resets the dialog (dialog result and remeber decision)
        /// </summary>
        public static void ResetDialog()
        {
            ExistingFileDialog.DialogResult = Result.None;
            ExistingFileDialog.RemeberDecision = null;
        }

        /// <summary>
        /// Name of the existing file
        /// </summary>
        public string ExisitingName { get; set; }
        /// <summary>
        /// Name of the new file (in archive)
        /// </summary>
        public string NewName { get; set; }
        /// <summary>
        /// Last change date of the existing file
        /// </summary>
        public DateTime ExistingDate { get; set; }
        /// <summary>
        /// Last change date of the new file (in archive)
        /// </summary>
        public DateTime NewDate { get; set; }
        /// <summary>
        /// Size of the existing file in bytes
        /// </summary>
        public long ExistingSize { get; set; }
        /// <summary>
        /// Size of the new file (in archive) in bytes
        /// </summary>
        public long NewSize { get; set; }
        /// <summary>
        /// CRC32 hash of the existing file
        /// </summary>
        public uint ExistingCrc { get; set; }
        /// <summary>
        /// CRC32 hash of the new file (in archive)
        /// </summary>
        public uint NewCrc { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExistingFileDialog()
        {
            InitializeComponent();
            ExistingFileDialog.ResetDialog();
            this.cancelButton.Focus();
        }

        /// <summary>
        /// Constructor with all arguments
        /// </summary>
        /// <param name="eName">Name of the existing file</param>
        /// <param name="eDate">Last change date of the existing file</param>
        /// <param name="eSize">Size of the existing file in byte</param>
        /// <param name="eCrc">CRC32 hash of the existing file</param>
        /// <param name="nName">Name of the new file (in archive)</param>
        /// <param name="nDate">Last change date of the new file (in archive)s</param>
        /// <param name="nSize">Size of the new file (in archive) in bytes</param>
        /// <param name="nCrc">CRC32 hash of the new file (in archive)</param>
        public ExistingFileDialog(string eName, DateTime eDate, long eSize, uint eCrc, string nName, DateTime nDate, long nSize, uint nCrc)
        {
            InitializeComponent();
            ExisitingName = eName;
            ExistingDate = eDate;
            ExistingSize = eSize;
            ExistingCrc = eCrc;
            NewName = nName;
            NewDate = nDate;
            NewSize = nSize;
            NewCrc = nCrc;
            SetValues();
            ExistingFileDialog.ResetDialog();
            this.cancelButton.Focus();
        }

        /// <summary>
        /// Sets the arguments in the GUI
        /// </summary>
        private void SetValues()
        {
            this.existingNameLabel.Content = ExisitingName;
            this.exisitingSizeLabel.Content = Utils.ConvertFileSize(ExistingSize);
            this.existingDateLabel.Content = ExistingDate.ToString("G");
            this.exisitingCrcLabel.Content = ExistingCrc.ToString("X");
            this.archiveNameLabel.Content = NewName;
            this.archiveSizeLabel.Content = Utils.ConvertFileSize(NewSize);
            this.archiveDateLabel.Content = NewDate.ToString("G");
            this.archiveCrcLabel.Content = NewCrc.ToString("X");
        }
        
        /// <summary>
        /// Prepares the closing of the dialog
        /// </summary>
        /// <param name="result">Dialog result to set</param>
        private void CloseDialog(Result result)
        {
            ExistingFileDialog.DialogResult = result;
            ExistingFileDialog.RemeberDecision = this.rememberCheckbox.IsChecked;
            try
            {
                this.Close();
            }
            catch {}
            
        }

        /// <summary>
        /// Method called from the overwrite button
        /// </summary>
        /// <param name="sender">Sender of the button</param>
        /// <param name="e">Button arguments</param>
        private void overwriteButton_Click(object sender, RoutedEventArgs e)
        {
           CloseDialog(Result.Overwrite);            
        }

        /// <summary>
        /// Method called from the skip button
        /// </summary>
        /// <param name="sender">Sender of the button</param>
        /// <param name="e">Button arguments</param>
        private void skipButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = Result.Skip;
            RemeberDecision = this.rememberCheckbox.IsChecked;
            this.Close();
        }

        /// <summary>
        /// Method called from the rename button
        /// </summary>
        /// <param name="sender">Sender of the button</param>
        /// <param name="e">Button arguments</param>
        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog(Result.Rename);          
        }

        /// <summary>
        /// Method called from the cancel button
        /// </summary>
        /// <param name="sender">Sender of the button</param>
        /// <param name="e">Button arguments</param>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog(Result.Cancel);          
        }

        /// <summary>
        /// Method called when the windows is closing
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == Result.None)
            {
                CloseDialog(Result.Cancel);          
            }
        }

        /// <summary>
        /// Method called if info icon is hovered
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void infoImage_MouseEnter(object sender, MouseEventArgs e)
        {
            infoBox.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Method called if info icon lost the mouse focus (hover ends)
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void infoImage_MouseLeave(object sender, MouseEventArgs e)
        {
            infoBox.Visibility = Visibility.Hidden;
        }

    }
}
