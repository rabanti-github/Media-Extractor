/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using AdonisUI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaExtractor
{
    /// <summary>
    /// Logic of the MainWindow Class
    /// </summary>
    public partial class MainWindow
    {
        #region properties
        /// <summary>
        /// Extractor instance
        /// </summary>
        private Extractor CurrentExtractor { get; set; }
        /// <summary>
        /// Application name
        /// </summary>
        private string ProductName { get; set; }
        /// <summary>
        /// Save file handler instance
        /// </summary>
        private SaveFileHandler saveFileHandler { get; set; }
        /// <summary>
        /// If true, a change of the locale is performed when the window is closed, otherwise it's a normal shutdown
        /// </summary>
        public bool HandleLocaleChange { get; set; }
        /// <summary>
        /// View model instance
        /// </summary>
        public ViewModel CurrentModel { get; set; }
        /// <summary>
        /// Current locale string
        /// </summary>
        public string CurrentLocale { get; set; } = null;
        #endregion

        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            CurrentModel = new ViewModel();
            DataContext = CurrentModel;
            saveFileHandler = new SaveFileHandler(CurrentModel);
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            ProductName = versionInfo.ProductName;
            CurrentModel.WindowTitle = versionInfo.ProductName;
            HandleArguments();
        }
        #endregion

        #region publicMethods
        /// <summary>
        /// Method to change the currently displayed cursor
        /// </summary>
        /// <param name="cursor">Cursor to show</param>
        public void ChangeCursor(Cursor cursor)
        {
            Dispatcher.Invoke
                (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => Cursor = cursor)
                );
        }

        /// <summary>
        /// Restores the user settings
        /// </summary>
        public void RestoreSettings()
        {
            CurrentModel.ShowEmbeddedImages = Properties.Settings.Default.DocumentShowImages;
            CurrentModel.ShowEmbeddedOther = Properties.Settings.Default.DocumentShowOther;
            CurrentModel.GenericTextPreview = Properties.Settings.Default.DocumentGenericTextPreview;
            CurrentModel.LargeFilePreviewWarning = Properties.Settings.Default.DocumentShowLargeFileWarning;
            CurrentModel.KeepFolderStructure = Properties.Settings.Default.DocumentPreserveStructure;
            CurrentModel.ShowInExplorer = Properties.Settings.Default.DocumentShowExplorer;
            CurrentModel.UseDarkMode = Properties.Settings.Default.AppearanceDarkMode;
            if (Properties.Settings.Default.ExtractSaveAll)
            {
                CurrentModel.SaveAllIsDefault = true;
            }
            else if (Properties.Settings.Default.ExtractSaveSelected)
            {
                CurrentModel.SaveSelectedIsDefault = true;
            }
            LoadRecentFiles(Properties.Settings.Default.RecentFiles);
            HandleDarkMode();
            string imageExts = Properties.Settings.Default.ImageExtensions;
            string textExts = Properties.Settings.Default.TextExtensions;
            string xmlExts = Properties.Settings.Default.XmlExtensions;
            if (!ExtractorItem.GetExtensions(textExts, imageExts, xmlExts))
            {
                Properties.Settings.Default.ImageExtensions = ExtractorItem.FALLBACK_IMAGE_EXTENSIONS;
                Properties.Settings.Default.TextExtensions = ExtractorItem.FALLBACK_TEXT_EXTENSIONS;
                Properties.Settings.Default.XmlExtensions = ExtractorItem.FALLBACK_XML_EXTENSIONS;
                MessageBox.Show(I18n.T(I18n.Key.DialogInvalidExtensions), I18n.T(I18n.Key.DialogErrorTitle), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Sets the image preview visible
        /// </summary>
        public void SetImagePreviewVisible()
        {
            ImageBox.Visibility = Visibility.Visible;
            TextBox.Visibility = Visibility.Hidden;
            NoPreviewBox.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Sets the text preview visible
        /// </summary>
        public void SetTextPreviewVisible(bool noPreview = false)
        {
            ImageBox.Visibility = Visibility.Hidden;
            if (noPreview)
            {
                TextBox.Visibility = Visibility.Hidden;
                NoPreviewBox.Visibility = Visibility.Visible;
            }
            else
            {
                TextBox.Visibility = Visibility.Visible;
                NoPreviewBox.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region privateMethods

        /// <summary>
        /// Method to handle command line arguments (open file as...)
        /// </summary>
        private void HandleArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string fileName = args[1];
                if (File.Exists(fileName))
                {
                    CurrentModel.FileName = args[1];
                    CurrentModel.StatusText = I18n.T(I18n.Key.StatusLoading);
                    Thread t = new Thread(LoadFile);
                    t.Start(this);
                }
            }
        }

        /// <summary>
        /// Method to open a file or archive
        /// </summary>
        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = I18n.T(I18n.Key.DialogLoadTitle);
            ofd.DefaultExt = ".docx";
            ofd.Filter = I18n.T(I18n.Key.DialogLoadFilter); // "All Office Formats|*.docx;*.dotx;*.docm;*.dotm;*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|Word documents|*.docx;*.dotx;*.docm;*.dotm|Excel documents|*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm|PowerPoint documents|*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|Common Archive Formats|*.zip;*.7z;*.rar;*.bzip2,*.gz;*.tar;*.cab;*.chm;*.lzh;*.iso|All files|*.*
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                LoadFile(ofd.FileName);
            }
        }

        /// <summary>
        /// Method to actually load a file, based on its path
        /// </summary>
        /// <param name="path">File path</param>
        private void LoadFile(string path)
        {
            TextBox.Text = string.Empty;
            ImageBox.Source = null;
            CurrentModel.FileName = path;
            CurrentModel.StatusText = I18n.T(I18n.Key.StatusLoading);
            Thread t = new Thread(LoadFile);
            t.Start(this);
        }

        /// <summary>
        /// Method to add a file path to the recent files 
        /// </summary>
        /// <param name="path">Path to add</param>
        private void AddRecentFile(string path)
        {
            List<string> recentFiles = CurrentModel.RecentFiles;
            if (recentFiles.Contains(path))
            {
                recentFiles.Remove(path);
            }
            recentFiles.Insert(0, path);
            if (recentFiles.Count >= ViewModel.NUMBER_OF_RECENT_FILES)
            {
                List<string> temp = recentFiles.Take(ViewModel.NUMBER_OF_RECENT_FILES).ToList();
                recentFiles = temp;
            }
            CurrentModel.RecentFiles = recentFiles;
            SaveRecentFiles();
        }

        /// <summary>
        /// Method to remove file paths from the recent files
        /// </summary>
        /// <param name="entryToRemove">Optional parameter to remove a single path. If null, the recent files are cleared</param>
        private void RemoveRecentFiles(string entryToRemove = null)
        {
            string[] entriesToRemove;
            if (entryToRemove == null)
            {
                entriesToRemove = CurrentModel.RecentFiles.ToArray();
            }
            else
            {
                entriesToRemove = new string[] { entryToRemove };
            }
            List<string> recentFiles = CurrentModel.RecentFiles;
            foreach (string entry in entriesToRemove)
            {
                if (recentFiles.Contains(entry))
                {
                    recentFiles.Remove(entry);
                }
            }
            CurrentModel.RecentFiles = recentFiles;
            SaveRecentFiles();
        }

        /// <summary>
        /// Method to save the recent files to the settings 
        /// </summary>
        private void SaveRecentFiles()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string file in CurrentModel.RecentFiles)
            {
                sb.Append(file).Append('|');
            }
            string recentFiles = sb.ToString().TrimEnd('|');
            Properties.Settings.Default.RecentFiles = recentFiles;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Method to load the recent files from a string (usually settings)
        /// </summary>
        /// <param name="splitString">Recent file paths, split by a pipe character</param>
        private void LoadRecentFiles(string splitString)
        {
            if (string.IsNullOrEmpty(splitString))
            {
                CurrentModel.RecentFiles = new List<string>();
                return;
            }
            string[] files = splitString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> recentFiles = new List<string>();
            foreach (string file in files)
            {
                recentFiles.Add(file);
                if (recentFiles.Count >= ViewModel.NUMBER_OF_RECENT_FILES)
                {
                    break;
                }
            }
            CurrentModel.RecentFiles = recentFiles;
        }

        /// <summary>
        /// Method to save the currently selected entry or entries as file(s)
        /// </summary>
        private void SaveSelectedFile()
        {
            this.saveFileHandler.SaveSelectedFiles();
        }

        /// <summary>
        /// Sets either Dark or Light Mode
        /// </summary>
        private void HandleDarkMode()
        {
            if (CurrentModel.UseDarkMode)
            {
                AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);
            }
            else
            {
                AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.LightColorScheme);
            }
        }

        /// <summary>
        /// Method to handle changing the locale
        /// </summary>
        /// <param name="locale">Locale as string</param>
        private void ChangeLocale(string locale)
        {
            HandleLocaleChange = true;
            CurrentLocale = locale;
            Close();
        }

        /// <summary>
        /// Method to restore the window position, size and state
        /// </summary>
        private void RestoreWindowState()
        {
            string screenName = Properties.Settings.Default.ScreenName;
            double left = Properties.Settings.Default.WindowLeft;
            double top = Properties.Settings.Default.WindowTop;
            double width = Properties.Settings.Default.WindowWidth;
            double height = Properties.Settings.Default.WindowHeight;

            ScreenHandler targetScreen;
            bool validScreen = false;
            if (ScreenHandler.GetScreenByName(screenName, out targetScreen))
            {
                validScreen = targetScreen.ValidateScreen(left, top, width, height);
            }

            if (validScreen)
            {

                if (Properties.Settings.Default.WindowMaximized)
                {
                    this.Left = targetScreen.X;
                    this.Top = targetScreen.Y;
                    this.Width = targetScreen.Width;
                    this.Height = targetScreen.Height;
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.Left = left;
                    this.Top = top;
                    this.Width = width;
                    this.Height = height;
                    this.WindowState = WindowState.Normal;
                }
            }
            else
            {
                ScreenHandler screen = ScreenHandler.GetPrimaryScreen();
                this.Left = screen.X +  (screen.Width / 2) - (this.Width / 2);
                this.Top = screen.Y  + (screen.Height / 2) - (this.Height / 2);
            }
        }

        /// <summary>
        /// Stores the user settings
        /// </summary>
        private void StoreSettings()
        {
            Properties.Settings.Default.DocumentShowImages = CurrentModel.ShowEmbeddedImages;
            Properties.Settings.Default.DocumentShowOther = CurrentModel.ShowEmbeddedOther;
            Properties.Settings.Default.DocumentGenericTextPreview = CurrentModel.GenericTextPreview;
            Properties.Settings.Default.DocumentShowLargeFileWarning = CurrentModel.LargeFilePreviewWarning;
            Properties.Settings.Default.DocumentPreserveStructure = CurrentModel.KeepFolderStructure;
            Properties.Settings.Default.DocumentShowExplorer = CurrentModel.ShowInExplorer;
            Properties.Settings.Default.AppearanceDarkMode = CurrentModel.UseDarkMode;
            Properties.Settings.Default.ExtractSaveAll = CurrentModel.SaveAllIsDefault;
            Properties.Settings.Default.ExtractSaveSelected = CurrentModel.SaveSelectedIsDefault;
            Properties.Settings.Default.Locale = CurrentLocale;
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.WindowTop = this.Top;
            Properties.Settings.Default.WindowWidth = this.Width;
            Properties.Settings.Default.WindowHeight = this.Height;
            if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.WindowMaximized = true;
            }
            else
            {
                Properties.Settings.Default.WindowMaximized = false;
            }
            Properties.Settings.Default.ScreenName = ScreenHandler.GetScreenNameByRect(this.Left, this.Top);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Shows the preview of the currently selected embedded file
        /// </summary>
        private void ShowPreview()
        {
            Cursor c = Cursor;
            CurrentModel.StatusText = I18n.T(I18n.Key.StatusLoadingEmbedded);
            Cursor = Cursors.Wait;
            try
            {
                ListViewItem[] selected = new ListViewItem[ImagesListView.SelectedItems.Count];
                ImagesListView.SelectedItems.CopyTo(selected, 0);
                CurrentModel.SelectedItems = selected;

                ListViewItem item = selected.Last();

                if (CurrentModel.LargeFilePreviewWarning)
                {
                    long threshold = Properties.Settings.Default.LargeFileThreshold;
                    if (item.FileSize.Value > threshold)
                    {
                        MessageBoxResult result = MessageBox.Show(I18n.R(I18n.Key.DialogSizeWarning, Utils.ConvertFileSize(threshold)), I18n.T(I18n.Key.DialogSizeWarningTitle), MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result != MessageBoxResult.Yes)
                        {
                            ImageBox.Source = null;
                            SetTextPreviewVisible(true);
                            CurrentExtractor.ResetErrors();
                            CurrentModel.StatusText = I18n.R(I18n.Key.StatusPreviewSkipped, item.FileName);
                            Cursor = c;
                            return;
                        }
                    }
                }

                if (item.Type == ExtractorItem.Type.Image)
                {
                    CurrentExtractor.GetImageSourceByName(item.FileName, out var img);
                    SetImagePreviewVisible();
                    if (CurrentExtractor.HasErrors)
                    {
                        if (CurrentModel.GenericTextPreview)
                        {
                            SetTextPreview(item);
                        }
                        else
                        {
                            CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoadEmbeddedImageFailure, CurrentExtractor.LastError);
                            SetTextPreviewVisible(true);
                        }
                        ImageBox.Source = null;
                        CurrentExtractor.ResetErrors();
                    }
                    else
                    {
                        ImageBox.Source = img;
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusEmbeddedLoaded, item.FileName);
                    }
                }
                else if (item.Type == ExtractorItem.Type.Xml || item.Type == ExtractorItem.Type.Text || CurrentModel.GenericTextPreview)
                {
                    CurrentExtractor.GetGenericTextByName(item.FileName, out var text);

                    if (CurrentExtractor.HasErrors)
                    {
                        SetTextPreviewVisible();
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoadEmbeddedTextFailure, CurrentExtractor.LastError);
                        TextBox.Text = string.Empty;
                        SetTextPreviewVisible(true);
                        CurrentExtractor.ResetErrors();
                    }
                    else
                    {
                        SetTextPreview(item);
                    }
                }
                else
                {
                    SetTextPreviewVisible(true);
                    CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoadEmbeddedOtherFailure, item.FileName);
                    TextBox.Text = string.Empty;

                    // Fall-back
                }
            }
            catch
            {
                CurrentModel.SelectedItems = new ListViewItem[0];
                // ignore
            }

            Cursor = c;
            if (ImagesListView.Items.Count == 0)
            {
                CurrentModel.SaveSelectedStatus = false;
            }
            else
            {
                CurrentModel.SaveSelectedStatus = true;
            }
        }

        #endregion

        #region publicStaticMethods

        /// <summary>
        /// Method to load a file. Method will called in a new tread
        /// </summary>
        /// <param name="data">Reference to the currently active Window</param>
        public static void LoadFile(object data)
        {
            MainWindow reference = (MainWindow)data;
            Cursor c = Cursors.Arrow;
            reference.ChangeCursor(Cursors.Wait);
            reference.CurrentExtractor = new Extractor(reference.CurrentModel.FileName, reference.CurrentModel);
            reference.CurrentExtractor.Extract(); // All includes images, XML and text
            if (reference.CurrentExtractor.HasErrors)
            {
                string message;
                string[] ext = new[] { ".docx", ".dotx", ".docm", ".dotm", ".xlsx", ".xlsm", ".xlsb", ".xltx", ".xltm", ".pptx", ".pptm", ".potx", ".potm", ".ppsx", ".ppsm", ".docx", ".dotx", ".docm", ".dotm", ".xlsx", ".xlsm", ".xlsb", ".xltx", ".xltm", ".pptx", ".pptm", ".potx", ".potm", ".ppsx", ".ppsm", ".zip", ".7z", ".rar", ".bzip2", ".gz", ".tar", ".cab", ".chm", ".lzh", ".iso" };
                try
                {
                    FileInfo fi = new FileInfo(reference.CurrentModel.FileName);
                    if (ext.Contains(fi.Extension.ToLower()))
                    {
                        message = I18n.T(I18n.Key.TextLockedFile);
                    }
                    else
                    {
                        message = I18n.T(I18n.Key.TextInvalidFormat);
                    }
                }
                catch
                {
                    message = I18n.T(I18n.Key.TextInvalidPath);
                }

                reference.CurrentModel.StatusText = I18n.T(I18n.Key.StatusLoadFailure);
                MessageBox.Show(I18n.R(I18n.Key.DialogLoadFailure, message, reference.CurrentExtractor.LastError), I18n.T(I18n.Key.DialogErrorTitle), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                reference.CurrentModel.WindowTitle = reference.ProductName;
                reference.CurrentExtractor.ResetErrors();
            }
            else
            {
                reference.CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoaded, reference.CurrentModel.FileName);
                reference.CurrentModel.WindowTitle = reference.ProductName + " - " + reference.CurrentModel.FileName;
                reference.AddRecentFile(reference.CurrentModel.FileName);
            }
            RecalculateListViwItems(reference);
            reference.CurrentModel.Progress = 0;
            reference.ChangeCursor(c);
        }

        #endregion

        #region privateStaticMethods

        /// <summary>
        /// Method to recalculate the listView items 
        /// </summary>
        /// <param name="reference">Reference to the currently active window</param>
        private static void RecalculateListViwItems(MainWindow reference)
        {
            if (reference.CurrentModel == null || reference.CurrentExtractor == null)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(delegate
            {
                try
                {
                    int i = 0;
                    reference.CurrentModel.ClearListView();
                    ListViewItem lItem;
                    foreach (ExtractorItem item in reference.CurrentExtractor.EmbeddedFiles)
                    {
                        if ((item.ItemType == ExtractorItem.Type.Image && reference.CurrentModel.ShowEmbeddedImages) || (item.ItemType != ExtractorItem.Type.Image && reference.CurrentModel.ShowEmbeddedOther))
                        {
                            lItem = new ListViewItem()
                            {
                                FileName = item.FileName,
                                FileExtension = item.FileExtension,
                                Path = item.Path,
                                FileReference = item,
                                FileSize = new ListViewItem.Size(item.FileSize)
                            };
                            lItem.Type = item.ItemType;
                            reference.CurrentModel.ListViewItems.Add(lItem);
                            i++;
                        }
                    }
                    if (i > 0)
                    {
                        reference.CurrentModel.SaveAllStatus = true;
                    }
                    else
                    {
                        reference.CurrentModel.SaveAllStatus = false;
                    }
                }
                catch
                {
                    // Ignore
                }
            });
        }

        #endregion

        #region uiEvent

        /// <summary>
        /// Event Method to handle a changed ListView item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowPreview();
        }

        /// <summary>
        /// Method to set a text as preview
        /// </summary>
        /// <param name="item">Listview item to preview</param>
        private void SetTextPreview(ListViewItem item)
        {
            string text = item.FileReference.GenericText;
            if (text.Length == 0 && item.FileSize.Value > 0)
            {
                switch (item.Type)
                {
                    case ExtractorItem.Type.Text:
                    case ExtractorItem.Type.Xml:
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoadEmbeddedTextFailure, item.FileName);
                        break;
                    default:
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoadEmbeddedOtherFailure, item.FileName);
                        break;
                }
                SetTextPreviewVisible(true);
            }
            else
            {
                switch (item.Type)
                {
                    case ExtractorItem.Type.Image:
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoadEmbeddedImageFallback, item.FileName);
                        break;
                    case ExtractorItem.Type.Text:
                    case ExtractorItem.Type.Xml:
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusEmbeddedLoaded, item.FileName);
                        break;
                    default:
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusLoadEmbeddedOtherFallback, item.FileName);
                        break;
                }
                TextBox.Text = text;
                SetTextPreviewVisible();
            }
            ImageBox.Source = null;
            CurrentExtractor.ResetErrors();
        }

        /// <summary>
        /// Menu Event for the image filter item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void ImageFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for other files filter
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void OtherFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for the exit item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void QuitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Menu Event for the file open item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        /// <summary>
        /// Menu Event for the save all files item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void SaveAllFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            saveFileHandler.SaveAllFiles();
        }

        /// <summary>
        /// Menu Event for the save selected files item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void SaveSelectedFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveSelectedFile();
        }

        /// <summary>
        /// Menu Event for the about item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        /// <summary>
        /// Menu Event for the open file item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        /// <summary>
        /// Menu event to open a recent file
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void OpenRecentFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = (sender as MenuItem).Header as string;
                if (File.Exists(path))
                {
                    LoadFile(path);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(I18n.R(I18n.Key.DialogMissingRecentFile, path), I18n.T(I18n.Key.DialogErrorTitle), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        RemoveRecentFiles(path);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Menu event to clear the list of recent files
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void ClearRecentFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveRecentFiles();
        }

        /// <summary>
        /// Opens the project website
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WebsiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Properties.Settings.Default.Website);
            }
            catch
            {
                MessageBox.Show(I18n.T(I18n.Key.DialogMissingWebsite), I18n.T(I18n.Key.DialogMissingWebsiteTitle), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Opens the license file
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LicenseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Utils.ShowInExplorer("license.txt"))
            {
                MessageBox.Show(I18n.R(I18n.Key.DialogMissingLicense, "license.txt"), I18n.T(I18n.Key.DialogMissingLicenseTitle), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Opens the change log
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ChangeLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Utils.ShowInExplorer("changelog.txt"))
            {
                MessageBox.Show(I18n.R(I18n.Key.DialogMissingChangelog, "changelog.txt"), I18n.T(I18n.Key.DialogMissingChangelogTitle),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Enables the Dark Mode theme
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DarkModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            HandleDarkMode();
        }

        /// <summary>
        /// Enables English locale as application language
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void EnglishMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLocale(I18n.ENGLISH);
        }

        /// <summary>
        /// Enables German as application language
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GermanMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLocale(I18n.GERMAN);
        }

        /// <summary>
        /// Enables French as application language
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FrenchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLocale(I18n.FRENCH);
        }

        /// <summary>
        /// Enables the system locale as application language
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SystemLocaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLocale(I18n.GetSystemLocale());
        }

        /// <summary>
        /// Invalidates the extractor entries (toggles show unknown formats as text)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GenericTextPreviewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentExtractor != null)
            {
                CurrentExtractor.InvalidateEntries();
                ShowPreview();
            }

        }

        /// <summary>
        /// Handles the window closing event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StoreSettings();
            if (!string.IsNullOrEmpty(this.CurrentLocale) && HandleLocaleChange)
            {
                string locale = this.CurrentLocale;
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
                MainWindow window = new MainWindow();
                window.CurrentLocale = locale;
                window.Show();
                window.Left = this.Left;
                window.Top = this.Top;
                window.WindowState = this.WindowState;
                I18n.MatchLocale(window.CurrentModel, locale);
                window.RestoreSettings();
            }
            else
            {
                App.Current.Shutdown();
            }
        }

        /// <summary>
        /// Handles the window loaded event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreWindowState();
        }

        /// <summary>
        /// Handles the drag & drop of files to open them
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void DragField_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadFile(files[0]);
            }
        }
        #endregion

    }
}
