/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace MediaExtractor
{
    /// <summary>
    /// Class for data binding
    /// </summary>
    
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ListViewItem> listViewItems;
        private BitmapImage image;
        private bool saveStatus;
        private string fileName;
        private string statusText;
        private bool saveAllStatus;
        private bool keepFolderStructure = true;
        private bool showInExplorer = true;
        private float numberOfFiles;
        private float currentFile;
        private int progress;
        private bool languageSystem = true;
        private bool languageEnglish;
        private bool languageGerman;
        //public Window WindowInstance { get; set; }

        public bool LanguageSystem
        {
            get { return languageSystem; }
            set
            {
                languageSystem = value; 
                LanguageNotify(I18N.AvailableCultures.system);
            }
        }

        public bool LanguageEnglish
        {
            get { return languageEnglish; }
            set
            {
                languageEnglish = value;
                LanguageNotify(I18N.AvailableCultures.en_US);
            }
        }

        public bool LanguageGerman
        {
            get { return languageGerman; }
            set
            {
                languageGerman = value;
                LanguageNotify(I18N.AvailableCultures.de_DE);
            }
        }

        private void LanguageNotify(I18N.AvailableCultures code)
        {
            languageEnglish = false;
            languageGerman = false;
            languageSystem = false;
            if (code == I18N.AvailableCultures.en_US)
            {
                languageEnglish = true;
            }
            else if (code == I18N.AvailableCultures.de_DE)
            {
                languageGerman = true;
            }
            else
            {
                languageSystem = true;
            }
            NotifyPropertyChanged("LanguageSystem");
            NotifyPropertyChanged("LanguageEnglish");
            NotifyPropertyChanged("LanguageGerman");
            //I18N.Current.SetLanguage(code, this.WindowInstance);
            App.Restart(code.ToString());
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
        [XmlIgnore]
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


/*
        /// <summary>
        /// Default constructor
        /// </summary>
        public ViewModel(Window windowInstance)
        {
            this.ListViewItems = new ObservableCollection<ListViewItem>();
            this.SaveStatus = false;
            this.FileName = string.Empty;
            this.StatusText = "Ready";
            //this.WindowInstance = windowInstance;
        }
*/
        public ViewModel()
        {
            this.ListViewItems = new ObservableCollection<ListViewItem>();
            this.SaveStatus = false;
            this.FileName = string.Empty;
            this.StatusText = "Ready";
        }

        /// <summary>
        /// Method to calculate the current progress when opening a file or archive
        /// </summary>
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

        /// <summary>
        /// Method to clear the listview (file overview)
        /// </summary>
        public void ClearListView()
        {
            this.ListViewItems.Clear();
            NotifyPropertyChanged("ListViewItems");
            this.SaveAllStatus = false;
            this.SaveStatus = false;
        }

        public string Serialize()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ViewModel));
                MemoryStream ms = new MemoryStream();
                ser.Serialize(ms, this);
                ms.Flush();
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                string xml = sr.ReadToEnd();
                ms.Close();
                byte[] bytes = Encoding.Default.GetBytes(xml);
                string hex = Convert.ToBase64String(bytes);
                //string hex = BitConverter.ToString(bytes);
                //hex = hex.Replace("-", "");
                return hex;
            }
            catch (Exception e)
            {
                return "null";
            }

        }

        public static ViewModel Deserialize(string hex)
        {
            if (hex == "null")
            {
                return null;
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(hex);
                string xml = Encoding.Default.GetString(bytes);
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(xml);
                sw.Flush();
                ms.Position = 0;
                XmlSerializer ser = new XmlSerializer(typeof(ViewModel));
                object o = ser.Deserialize(ms);
                ms.Close();
                return (ViewModel)o;
            }
            catch (Exception e)
            {
                return null;
            }
        }


        /// <summary>
        /// Method to propagate changes for the data binding
        /// </summary>
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
