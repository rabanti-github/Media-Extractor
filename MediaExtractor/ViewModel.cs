/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace MediaExtractor
{
    /// <summary>
    /// Class for data binding
    /// </summary>
    public class ViewModel : INotifyPropertyChanged
    {
        private string windowTitle;
        private ObservableCollection<ListViewItem> listViewItems;
        private BitmapImage image;
        private bool saveStatus;
        private string fileName;
        private string statusText;
        private bool saveAllStatus;
        private bool keepFolderStructure = true;
        private bool showInExplorer = true;
        private bool useDarkMode = false;
        private bool useEnglishLocale = false;
        private bool useGermanLocale = false;
        private float numberOfFiles;
        private float currentFile;
        private int progress;
        private readonly float FLOATING_POINT_TOLERANCE = 0.00001f;

        /// <summary>
        /// The text of the main window
        /// </summary>
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                NotifyPropertyChanged("WindowTitle");
            }
        }

        /// <summary>
        /// The current progress of extraction or rendering (progress bar from 0 to 100 %)
        /// </summary>
        public int Progress
        {
            get { return progress; }
            set 
            {
                progress = value;
                NotifyPropertyChanged("Progress");
            }
        }
        
        /// <summary>
        /// Current file index as float (to avoid multiple casting when calculating the progress)
        /// </summary>
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

        /// <summary>
        /// Total number of files in the currently loaded archive as float (to avoid multiple casting when calculating the progress)
        /// </summary>
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
        
        /// <summary>
        /// Enabled / Disabled State of the button to save all files
        /// </summary>
        public bool SaveAllStatus
        {
            get { return saveAllStatus; }
            set 
            {
                saveAllStatus = value;
                NotifyPropertyChanged("SaveAllStatus");
            }
        }
        
        /// <summary>
        /// If true, the folder structure of the file / archive will be kept when extracted (save all files)
        /// </summary>
        public bool KeepFolderStructure
        {
            get { return keepFolderStructure; }
            set
            {
                keepFolderStructure = value;
                NotifyPropertyChanged("KeepFolderStructure");
            }
        }

        /// <summary>
        /// If true, Windows Explorer will be opened at the selected location after the extraction
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Explorer shall be opened, otherwise, <c>false</c>.
        /// </value>
        public bool ShowInExplorer
        {
            get { return showInExplorer; }
            set
            {
                showInExplorer = value;
                NotifyPropertyChanged("ShowInExplorer");
            }
        }


        /// <summary>
        /// If true, the Application will be rendered in Dark Mode
        /// </summary>
        /// <value>
        ///   <c>true</c> if dark Mode is used, otherwise, <c>false</c> (use Light Mode).
        /// </value>
        public bool UseDarkMode
        {
            get { return useDarkMode; }
            set
            {
                useDarkMode = value;
                NotifyPropertyChanged("UseDarkMode");
            }
        }


        /// <summary>
        /// If true, the Application will be using English (en) as locale
        /// </summary>
        /// <value>
        ///   <c>true</c> if English is the current locale, otherwise, <c>false</c>
        /// </value>
        public bool UseEnglishLocale
        {
            get { return useEnglishLocale; }
            set
            {
                if (value)
                {
                    UseGermanLocale = false;
                }
                useEnglishLocale = value;
                NotifyPropertyChanged("UseEnglishLocale");
            }
        }

        /// <summary>
        /// If true, the Application will be using German (de-DE) as locale
        /// </summary>
        /// <value>
        ///   <c>true</c> if German is the current locale, otherwise, <c>false</c>
        /// </value>
        public bool UseGermanLocale
        {
            get { return useGermanLocale; }
            set
            {
                if (value)
                {
                    UseEnglishLocale = false;
                }
                useGermanLocale = value;
                NotifyPropertyChanged("UseGermanLocale");
            }
        }


        /// <summary>
        /// Enabled / Disabled State of the button to save a single files
        /// </summary>
        public bool SaveStatus
        {
            get { return saveStatus; }
            set
            {
                saveStatus = value;
                NotifyPropertyChanged("SaveStatus");
            }
        }

        /// <summary>
        /// The text of the status bar
        /// </summary>
        public string StatusText
        {
            get { return statusText; }
            set 
            { 
                statusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }
        
        /// <summary>
        /// The name / full path of the currently loaded file
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set 
            { 
                fileName = value;
                NotifyPropertyChanged("FileName");
            }
        }
        
        /// <summary>
        /// Currently displayed preview image
        /// </summary>
        public BitmapImage Image
        {
            get { return image; }
            set
            {
                image = value;
                NotifyPropertyChanged("Image");
            }
        }
        
        /// <summary>
        /// Items of the listview (file overview)
        /// </summary>
        public ObservableCollection<ListViewItem> ListViewItems
        {
            get { return listViewItems; }
            set
            { 
                listViewItems = value;
                NotifyPropertyChanged("ListViewItems");
            }
        }



        /// <summary>
        /// Default constructor
        /// </summary>
        public ViewModel()
        {
            ListViewItems = new ObservableCollection<ListViewItem>();
            SaveStatus = false;
            FileName = string.Empty;
            StatusText = "Ready";
        }

        /// <summary>
        /// Method to calculate the current progress when opening a file or archive
        /// </summary>
        public void CalculateProgress()
        {
            if (Math.Abs(numberOfFiles) < FLOATING_POINT_TOLERANCE)
            {
                Progress = 0;
            }
            else
            {
                float p = currentFile / numberOfFiles * 100;
                Progress = (int)p;
            }
        }

        /// <summary>
        /// Method to clear the listview (file overview)
        /// </summary>
        public void ClearListView()
        {
            ListViewItems.Clear();
            NotifyPropertyChanged("ListViewItems");
            SaveAllStatus = false;
            SaveStatus = false;
        }

        /// <summary>
        /// Method to propagate changes for the data binding
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies a property change
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
