using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace MediaExtractor
{
    public class ViewModel : INotifyPropertyChanged
    {
       // public PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ListViewItem> listViewItems;
        private BitmapImage image;
        private bool saveStatus;
        private string fileName;
        private string statusText;
        private bool saveAllStatus;
        private float numberOfFiles;
        private float currentFile;
        private int progress;

        public int Progress
        {
            get { return progress; }
            set 
            {
                progress = value;
                NotifyPropertyChanged("Progress");
            }
        }
        

        public float CurrentFile
        {
            get { return currentFile; }
            set 
            {
                currentFile = value;
                NotifyPropertyChanged("CurrentFile");
                CalculateProgress();
            }
        }
        

        public float NumberOfFiles
        {
            get { return numberOfFiles; }
            set 
            {
                numberOfFiles = value;
                NotifyPropertyChanged("NumberOfFiles");
                CalculateProgress();
            }
        }
        

        public bool SaveAllStatus
        {
            get { return saveAllStatus; }
            set 
            {
                saveAllStatus = value;
                NotifyPropertyChanged("SaveAllStatus");
            }
        }
        

        public string StatusText
        {
            get { return statusText; }
            set 
            { 
                statusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }
        

        public string FileName
        {
            get { return fileName; }
            set 
            { 
                fileName = value;
                NotifyPropertyChanged("FileName");
            }
        }
        

        public bool SaveStatus
        {
            get { return saveStatus; }
            set 
            { 
                saveStatus = value;
                NotifyPropertyChanged("SaveStatus");
            }
        }
        

        public BitmapImage Image
        {
            get { return image; }
            set
            {
                image = value;
                NotifyPropertyChanged("Image");
            }
        }
        
        public ObservableCollection<ListViewItem> ListViewItems
        {
            get { return listViewItems; }
            set
            { 
                listViewItems = value;
                NotifyPropertyChanged("ListViewItems");
            }
        }
      
  


        public ViewModel()
        {
            this.ListViewItems = new ObservableCollection<ListViewItem>();
            this.SaveStatus = false;
            this.FileName = string.Empty;
            this.StatusText = "Ready";
        }


        public void CalculateProgress()
        {
            if (this.numberOfFiles == 0)
            {
                this.Progress = 0;
            }
            else
            {
                float p = this.currentFile / this.numberOfFiles * 100;
                this.Progress = (int)p;
            }
        }

        public void ClearListView()
        {
            this.ListViewItems.Clear();
            NotifyPropertyChanged("ListViewItems");
            this.SaveAllStatus = false;
            this.SaveStatus = false;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
