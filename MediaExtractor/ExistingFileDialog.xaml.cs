using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MediaExtractor
{
    /// <summary>
    /// Logic of the ExistingFileDialog Class
    /// </summary>
    public partial class ExistingFileDialog : Window
    {
        public enum Result
        {
            Skip,
            Overwrite,
            Rename,
            Cancel,
            None,
        }

        public static Result DialogResult = Result.None;
        public static bool? RemeberDecision;

        public static void ResetDialog()
        {
            ExistingFileDialog.DialogResult = Result.None;
            ExistingFileDialog.RemeberDecision = null;
        }

        public string ExisitingName { get; set; }
        public string NewName { get; set; }
        public DateTime ExistingDate { get; set; }
        public DateTime NewDate { get; set; }
        public long ExistingSize { get; set; }
        public long NewSize { get; set; }
        public uint ExistingCrc { get; set; }
        public uint NewCrc { get; set; }

        public ExistingFileDialog()
        {
            InitializeComponent();
            ExistingFileDialog.ResetDialog();
            this.cancelButton.Focus();
        }

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


        private void overwriteButton_Click(object sender, RoutedEventArgs e)
        {
           CloseDialog(Result.Overwrite);            
        }

        private void skipButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = Result.Skip;
            RemeberDecision = this.rememberCheckbox.IsChecked;
            this.Close();
        }

        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog(Result.Rename);          
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog(Result.Cancel);          
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == Result.None)
            {
                CloseDialog(Result.Cancel);          
            }
        }

        private void infoImage_MouseEnter(object sender, MouseEventArgs e)
        {
            infoBox.Visibility = Visibility.Visible;
        }

        private void infoImage_MouseLeave(object sender, MouseEventArgs e)
        {
            infoBox.Visibility = Visibility.Hidden;
        }

    }
}
