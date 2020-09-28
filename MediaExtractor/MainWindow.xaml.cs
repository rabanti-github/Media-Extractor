/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using AdonisUI;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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

        

        public string CurrentLocale { get; private set; } = null;
        public bool HandleLocaleChange { get; set; }


        private SaveFileHandler saveFileHandler { get; set; }

        private ViewModel CurrentModel { get; set; }
        private Extractor CurrentExtractor{ get; set; }

        private string ProductName { get; set; }

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
           // Title = versionInfo.ProductName;
           CurrentModel.WindowTitle = versionInfo.ProductName;
            CurrentModel.UseEnglishLocale = true; // TODO: Get from settings
            HandleArguments();
        }

        /// <summary>
        /// Method to handle command line arguments (open file as...)
        /// </summary>
        private void HandleArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string fileName = args[1];
                if (File.Exists(fileName) == false) { return; }
                else
                {
                    CurrentModel.FileName = args[1];
                    CurrentModel.StatusText = I18n.T("StatusLoading");
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
            ofd.Title = I18n.T("DialogLoadTitle");
            ofd.DefaultExt = ".docx";
            ofd.Filter = I18n.T("DialogLoadFilter"); // "All Office Formats|*.docx;*.dotx;*.docm;*.dotm;*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|Word documents|*.docx;*.dotx;*.docm;*.dotm|Excel documents|*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm|PowerPoint documents|*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|Common Archive Formats|*.zip;*.7z;*.rar;*.bzip2,*.gz;*.tar;*.cab;*.chm;*.lzh;*.iso|All files|*.*
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
            CurrentModel.StatusText = I18n.T("StatusLoading");
            Thread t = new Thread(LoadFile);
            t.Start(this);
        }


        /// <summary>
        /// Method to change the currently displayed cursor
        /// </summary>
        /// <param name="cursor">Cursor to show</param>
        public void ChangeCursor(Cursor cursor)
        {
            Dispatcher.Invoke
                (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(()=> Cursor = cursor)
                );
        }

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
            if (reference.CurrentExtractor.HasErrors)
            {
                reference.CurrentModel.StatusText = I18n.T("StatusNotLoaded");
                reference.CurrentModel.WindowTitle = reference.ProductName;
                reference.CurrentExtractor.ResetErrors();
                return;
            }
            else
            {
                reference.CurrentExtractor.Extract(Extractor.EmbeddedFormat.All); // All includes images, xml and text
                if (reference.CurrentExtractor.HasErrors == true)
                {
                    string message;
                    string[] ext = new []{".docx", ".dotx", ".docm", ".dotm", ".xlsx", ".xlsm", ".xlsb", ".xltx", ".xltm", ".pptx", ".pptm", ".potx", ".potm", ".ppsx", ".ppsm",".docx", ".dotx", ".docm", ".dotm", ".xlsx", ".xlsm", ".xlsb", ".xltx", ".xltm", ".pptx", ".pptm", ".potx", ".potm", ".ppsx", ".ppsm", ".zip", ".7z", ".rar", ".bzip2",".gz", ".tar", ".cab", ".chm", ".lzh", ".iso"};
                    try
                    {
                        FileInfo fi = new FileInfo(reference.CurrentModel.FileName);
                        if (ext.Contains(fi.Extension.ToLower()) == true)
                        {
                            message = I18n.T("TextLockedFile");
                        }
                        else
                        {
                            message = I18n.T("TextInvalidFormat");
                        }
                    }
                    catch
                    {
                        message = I18n.T("TextInvalidPath");
                    }

                    reference.CurrentModel.StatusText = I18n.T("StatusLoadFailure");
                    MessageBox.Show(I18n.R("DialogLoadFailure", message, reference.CurrentExtractor.LastError), I18n.T("DialogErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    reference.CurrentModel.WindowTitle = reference.ProductName;
                    reference.CurrentModel.Progress = 0;
                    reference.ChangeCursor(c);
                    reference.CurrentExtractor.ResetErrors();
                    RecalculateListViwItems(reference);
                    return;
                }
                RecalculateListViwItems(reference);
            }
            reference.CurrentModel.StatusText = I18n.R("StatusLoaded", reference.CurrentModel.FileName);
            reference.CurrentModel.WindowTitle = reference.ProductName + " - " + reference.CurrentModel.FileName;
            reference.CurrentModel.Progress = 0;
            reference.ChangeCursor(c);
        }

        /// <summary>
        /// Method to recalculate the listView items 
        /// </summary>
        /// <param name="reference">Reference to the currently active window</param>
        private static void RecalculateListViwItems(MainWindow reference)
        {
            if (reference.CurrentModel == null)
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
                            if ((item.IsImage == true && reference.ImageFilterMenuItem.IsChecked == true) || (item.IsImage == false && reference.OtherFilterMenuItem.IsChecked == true))
                            {
                                lItem = new ListViewItem()
                                {
                                    FileName = item.FileName,
                                    FileExtension = item.FileExtension,
                                    Path = item.Path,
                                    FileReference = item
                                };
                                lItem.SetType();
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
                    catch(Exception e)
                    {
                        Console.WriteLine("error: " + e.Message);
                    }
                });
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

        /// <summary>
        /// Event Method to handle a changed ListView item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor c = Cursor;
            CurrentModel.StatusText = "Loading embedded file... Please wait";
            Cursor = Cursors.Wait;
            try
            {
                ListViewItem[] selected = new ListViewItem[ImagesListView.SelectedItems.Count];
                ImagesListView.SelectedItems.CopyTo(selected, 0);
                CurrentModel.SelectedItems = selected;

                ListViewItem item = selected.Last();

                if (item.Type == ListViewItem.FileType.Image)
                {
                    CurrentExtractor.GetImageSourceByName(item.FileName, out var img);
                    SetImagePreviewVisible();
                    if (CurrentExtractor.HasErrors == true)
                    {
                        CurrentModel.StatusText = "Embedded file could not be loaded: " + CurrentExtractor.LastError;
                        ImageBox.Source = null;
                        CurrentExtractor.ResetErrors();
                    }
                    else
                    {
                        ImageBox.Source = img;
                        CurrentModel.StatusText = item.FileName + " loaded";
                    }
                }
                else if (item.Type == ListViewItem.FileType.Xml || item.Type == ListViewItem.FileType.Text)
                {
                    CurrentExtractor.GetGenericTextByName(item.FileName, out var text);
                    SetTextPreviewVisible();
                    if (CurrentExtractor.HasErrors == true)
                    {
                        CurrentModel.StatusText = "Text / XML file could not be loaded: " + CurrentExtractor.LastError;
                        TextBox.Text = string.Empty;
                        CurrentExtractor.ResetErrors();
                    }
                    else
                    {
                        TextBox.Text = text;
                        CurrentModel.StatusText = item.FileName + " loaded";
                    }
                }
                else
                {
                    SetTextPreviewVisible(true);
                    CurrentModel.StatusText = "Preview not possible for " + item.FileName;
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

        /// <summary>
        /// Menu Event for the image filter item (checked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void ImageFilterMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for other files filter (checked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void OtherFilterMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for the image filter item (unchecked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void ImageFilterMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for other files filter (unchecked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void OtherFilterMenuItem_Unchecked(object sender, RoutedEventArgs e)
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
            //FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            //string name = versionInfo.ProductName;
            //string version = versionInfo.ProductVersion.ToString();
            //MessageBox.Show(name + " v" + version + "\n--------------------------\nAuthor: Raphael Stoeckli\nLicense: MIT", "About", MessageBoxButton.OK, MessageBoxImage.Information);
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
        /// Method to save the currently selected entry or entries as file(s)
        /// </summary>
        private void SaveSelectedFile()
        {
           this.saveFileHandler.SaveSelectedFiles();
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        /// <summary>
        /// Opens the license file
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LicenseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.ShowInExplorer("license.txt") == false)
            { 
                MessageBox.Show("The license file 'license.txt' was not found.", "License could not be found",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Opens the change log
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ChangeLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.ShowInExplorer("changelog.txt") == false)
            {
                MessageBox.Show("The change log 'changelog.txt' was not found.", "Change log could not be found",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Enables or disables the Dark Mode theme
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DarkModeMenuItem_Click(object sender, RoutedEventArgs e)
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

        private void EnglishMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLocale(I18n.ENGLISH);
        }

        private void GermanhMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLocale(I18n.GERMAN);
        }

        private void ChangeLocale(string locale)
        {
            HandleLocaleChange = true;
            CurrentLocale = locale;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CurrentLocale) && HandleLocaleChange)
            {
                string locale = this.CurrentLocale;
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
                MainWindow window = new MainWindow();
                window.Show();
                window.Left = this.Left;
                window.Top = this.Top;
                I18n.MatchLocale(window.CurrentModel, locale);
            }
            else
            {
                App.Current.Shutdown();
            }
        }


        //public static class

        private void DragField_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadFile(files[0]);
            }
        }

        public RoutedUICommand SaveDefaultCommand = new RoutedUICommand("Save", "SaveDefault", typeof(MainWindow));

        public void SaveDefaultCommand_Execute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        public void SaveDefaultCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

    }
}
