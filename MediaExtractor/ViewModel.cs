/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2022
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MediaExtractor
{
    /// <summary>
    /// Class for data binding
    /// </summary>
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly float FLOATING_POINT_TOLERANCE = 0.00001f;
        public static readonly int NUMBER_OF_RECENT_FILES = 10;

        private string windowTitle;
        private ObservableCollection<ListViewItem> listViewItems;
        private ICommand saveDefaultCommand;
        private BitmapImage image;
        private List<string> recentFiles = new List<string>();
        private bool saveStatus;
        private string fileName;
        private string statusText;
        private bool saveAllStatus;
        private bool showEmbeddedImages;
        private bool showEmbeddedOther;
        private bool genericTextPreview;
        private bool largeFilePreviewWarning = true;
        private bool keepFolderStructure = true;
        private bool showInExplorer = true;
        private bool useDarkMode = false;
        private bool useSystemLocale = true;
        private bool useEnglishLocale = false;
        private bool useGermanLocale = false;
        private bool saveSelectedIsDefault;
        private bool saveAllIsDefault;
        private float numberOfFiles;
        private float currentFile;
        private int progress;



        /// <summary>
        /// Handler for the combined save button
        /// </summary>
        public SaveFileHandler CurrentSaveFileHandler { get; set; }

        /// <summary>
        /// Command to handle the combined save button
        /// </summary>
        public ICommand SaveDefaultCommand
        {
            get
            {
                return saveDefaultCommand ?? (saveDefaultCommand = new CommandHandler(() => SaveDefault(), () => true));
            }
        }

        /// <summary>
        /// Indicated whether selected files is the default for saving multiple files
        /// </summary>
        public bool SaveSelectedIsDefault
        {
            get { return saveSelectedIsDefault; }
            set
            {
                if (value)
                {
                    SaveAllIsDefault = false;
                }
                CurrentSaveFileHandler.DefaultMethod = SaveFileHandler.DefaultSaveMethod.Selected;
                saveSelectedIsDefault = value;
                NotifyPropertyChanged("SaveSelectedIsDefault");
            }
        }

        /// <summary>
        /// Indicated whether all files is the default for saving multiple files
        /// </summary>
        public bool SaveAllIsDefault
        {
            get { return saveAllIsDefault; }
            set
            {
                if (value)
                {
                    SaveSelectedIsDefault = false;
                }
                CurrentSaveFileHandler.DefaultMethod = SaveFileHandler.DefaultSaveMethod.All;
                saveAllIsDefault = value;
                NotifyPropertyChanged("SaveAllIsDefault");
            }
        }
        /// <summary>
        /// Executes the default save method in the save handler
        /// </summary>
        private void SaveDefault()
        {
            CurrentSaveFileHandler.SaveDefault();
        }

        public ListViewItem[] SelectedItems { get; set; } = new ListViewItem[0];

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
        ///   <c>true</c> if Dark Mode is used, otherwise, <c>false</c> (use Light Mode).
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
        /// If true, embedded Images will be shown
        /// </summary>
        /// <value>
        ///   <c>true</c> if embedded images are shown, otherwise, <c>false</c>.
        /// </value>
        public bool ShowEmbeddedImages
        {
            get { return showEmbeddedImages; }
            set
            {
                showEmbeddedImages = value;
                NotifyPropertyChanged("ShowEmbeddedImages");
            }
        }

        /// <summary>
        /// If true, other embedded files will be shown
        /// </summary>
        /// <value>
        ///   <c>true</c> if other, embedded files are shown, otherwise, <c>false</c>.
        /// </value>
        public bool ShowEmbeddedOther
        {
            get { return showEmbeddedOther; }
            set
            {
                showEmbeddedOther = value;
                NotifyPropertyChanged("ShowEmbeddedOther");
            }
        }

        /// <summary>
        /// If true, unknown file formats are tried to be displayed as text
        /// </summary>
        /// <value>
        ///   <c>true</c> if unknown file formats are previewed as text, otherwise, <c>false</c>.
        /// </value>
        public bool GenericTextPreview
        {
            get { return genericTextPreview; }
            set
            {
                genericTextPreview = value;
                NotifyPropertyChanged("GenericTextPreview");
            }
        }

        /// <summary>
        /// If true, a warning will be displayed when a large file is tried to be displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if size warning on large previews is displayed, otherwise, <c>false</c>.
        /// </value>
        public bool LargeFilePreviewWarning
        {
            get { return largeFilePreviewWarning; }
            set
            {
                largeFilePreviewWarning = value;
                NotifyPropertyChanged("LargeFilePreviewWarning");
            }
        }

        /// <summary>
        /// If true, the Application will be using the system default locale (if available)
        /// </summary>
        /// <value>
        ///   <c>true</c> if the system defines the current locale, otherwise, <c>false</c>
        /// </value>
        public bool UseSystemLocale
        {
            get { return useSystemLocale; }
            set
            {
                if (value)
                {
                    UseGermanLocale = false;
                    UseEnglishLocale = false;
                }
                useSystemLocale = value;
                NotifyPropertyChanged("UseSystemLocale");
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
                    UseSystemLocale = false;
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
                    UseSystemLocale = false;
                }
                useGermanLocale = value;
                NotifyPropertyChanged("UseGermanLocale");
            }
        }

        /// <summary>
        /// Enabled / Disabled State of the button to the selected file(s)
        /// </summary>
        public bool SaveSelectedStatus
        {
            get { return saveStatus; }
            set
            {
                saveStatus = value;
                NotifyPropertyChanged("SaveSelectedStatus");
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
        /// Gets recent file number 1 (may be empty if not existing)
        /// </summary>
        public string RecentFile1
        {
            get { return GetRecentFile(0); }
        }

        /// <summary>
        /// Gets recent file number 2 (may be empty if not existing)
        /// </summary>
        public string RecentFile2
        {
            get { return GetRecentFile(1); }
        }

        /// <summary>
        /// Gets recent file number 3 (may be empty if not existing)
        /// </summary>
        public string RecentFile3
        {
            get { return GetRecentFile(2); }
        }

        /// <summary>
        /// Gets recent file number 4 (may be empty if not existing)
        /// </summary>
        public string RecentFile4
        {
            get { return GetRecentFile(3); }
        }

        /// <summary>
        /// Gets recent file number 5 (may be empty if not existing)
        /// </summary>
        public string RecentFile5
        {
            get { return GetRecentFile(4); }
        }

        /// <summary>
        /// Gets recent file number 6 (may be empty if not existing)
        /// </summary>
        public string RecentFile6
        {
            get { return GetRecentFile(5); }
        }

        /// <summary>
        /// Gets recent file number 7 (may be empty if not existing)
        /// </summary>
        public string RecentFile7
        {
            get { return GetRecentFile(6); }
        }

        /// <summary>
        /// Gets recent file number 8 (may be empty if not existing)
        /// </summary>
        public string RecentFile8
        {
            get { return GetRecentFile(7); }
        }

        /// <summary>
        /// Gets recent file number 9 (may be empty if not existing)
        /// </summary>
        public string RecentFile9
        {
            get { return GetRecentFile(8); }
        }

        /// <summary>
        /// Gets recent file number 10 (may be empty if not existing)
        /// </summary>
        public string RecentFile10
        {
            get { return GetRecentFile(9); }
        }

        /// <summary>
        /// Gets or sets the recent files (0 to 9 considered)
        /// </summary>
        public List<string> RecentFiles
        {
            get { return recentFiles; }
            set
            {
                this.recentFiles = value;
                for (int i = 1; i <= NUMBER_OF_RECENT_FILES; i++)
                {
                    NotifyPropertyChanged("RecentFile" + i.ToString(CultureInfo.InvariantCulture));
                }
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
            SaveSelectedStatus = false;
            FileName = string.Empty;
            StatusText = I18n.T(I18n.Key.StatusReady);
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
            SaveSelectedStatus = false;
        }

        /// <summary>
        /// Method to get a recent file path based on its index
        /// </summary>
        /// <param name="index">Index of the recent file entry</param>
        /// <returns>File path or empty if the index is out of bound</returns>
        private string GetRecentFile(int index)
        {
            if (recentFiles == null || recentFiles.Count < index + 1)
            {
                return string.Empty;
            }
            return recentFiles[index];
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
