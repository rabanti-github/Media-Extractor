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
using System.Diagnostics;
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

        private ViewModel CurrentModel { get; set; }
        private Extractor CurrentExtractor{ get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            CurrentModel = new ViewModel();
            DataContext = CurrentModel;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            Title = versionInfo.ProductName;
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
                    CurrentModel.StatusText = "Loading file... Please wait";
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
            ofd.Title = "Select Office File...";
            ofd.DefaultExt = ".docx";
            ofd.Filter = "All Office Formats|*.docx;*.dotx;*.docm;*.dotm;*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|Word documents|*.docx;*.dotx;*.docm;*.dotm|Excel documents|*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm|PowerPoint documents|*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|Common Archive Formats|*.zip;*.7z;*.rar;*.bzip2,*.gz;*.tar;*.cab;*.chm;*.lzh;*.iso|All files|*.*";
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            { 
                TextBox.Text = string.Empty;
                ImageBox.Source = null;
                CurrentModel.FileName = ofd.FileName;
                CurrentModel.StatusText = "File loaded: " + CurrentModel.FileName;
                CurrentModel.StatusText = "Loading file... Please wait";
                Thread t = new Thread(LoadFile);
                t.Start(this);
            }
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
                reference.CurrentModel.StatusText = "The file could not be loaded (no suitable format)";
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
                            message = "Please make sure that the file is not open in another application.";
                        }
                        else
                        {
                            message = "The file may be not a valid Office file or archive.";
                        }
                    }
                    catch
                    {
                        message = "It looks like the filename is not valid. Please check the file name and path.";
                    }

                    reference.CurrentModel.StatusText = "The file could not be loaded";
                    MessageBox.Show("The file could not be loaded.\n" + message + "\nError Message: " + reference.CurrentExtractor.LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    reference.CurrentModel.Progress = 0;
                    reference.ChangeCursor(c);
                    reference.CurrentExtractor.ResetErrors();
                    return;
                }
                RecalculateListViwItems(reference);
            }
            reference.CurrentModel.StatusText = "File was loaded";
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
        }

        /// <summary>
        /// Sets the text preview visible
        /// </summary>
        public void SetTextPreviewVisible()
        {
            TextBox.Visibility = Visibility.Visible;
            ImageBox.Visibility = Visibility.Hidden;
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
                ListViewItem item = (ListViewItem)ImagesListView.SelectedItem;
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
                    SetTextPreviewVisible();
                    TextBox.Text = string.Empty;
                    // Fall-back
                }
            }
            catch
            {
                // ignore
            }

            Cursor = c;
            if (ImagesListView.Items.Count == 0)
            {
                CurrentModel.SaveStatus = false;
            }
            else
            {
                CurrentModel.SaveStatus = true;
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
            SaveAllFiles();
        }

        /// <summary>
        /// Menu Event for the safe file item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void SaveFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
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
        /// Event for the save file button
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        /// <summary>
        /// Event for the save all files button
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void SaveAllFilesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAllFiles();
        }

        /// <summary>
        /// Method to ave all files
        /// </summary>
        private void SaveAllFiles()
        {
            try
            {
                CommonOpenFileDialog ofd = new CommonOpenFileDialog();
                ofd.IsFolderPicker = true;
                ofd.Title = "Select a Folder to save all Files...";
                CommonFileDialogResult res = ofd.ShowDialog();            
                if (res == CommonFileDialogResult.Ok)
                {
                    bool fileExists, check;
                    int errors = 0;
                    int skipped = 0;
                    int extracted = 0;
                    int renamed = 0;
                    int overwritten = 0;
                    FileInfo fi;
                    ExistingFileDialog.ResetDialog();
                    foreach (ListViewItem item in CurrentModel.ListViewItems)
                    {
                        fileExists = CheckFileExists(ofd.FileName, item.FileReference, CurrentModel.KeepFolderStructure, out var fileName);
                        if (fileExists == true)
                        {
                            if (ExistingFileDialog.RemeberDecision == null || ExistingFileDialog.RemeberDecision.Value != true)
                            {
                                fi = new FileInfo(fileName);
                                uint crc = Utils.GetCrc(fileName);
                                ExistingFileDialog efd = new ExistingFileDialog(fi.Name, fi.LastWriteTime, fi.Length, crc, item.FileName, item.FileReference.LastChange, item.FileReference.FileSize, item.FileReference.Crc32);
                                efd.ShowDialog();
                            }
                        }

                        if (fileExists)
                        {
                            if (ExistingFileDialog.DialogResult == ExistingFileDialog.Result.Cancel) // Cancel extractor
                            {
                                CurrentModel.StatusText = "The save process was canceled";
                                MessageBox.Show("The save process was canceled", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            else if (ExistingFileDialog.DialogResult == ExistingFileDialog.Result.Overwrite) // Overwrite existing
                            {
                                check = Save(item.FileReference, fileName, false);
                                if (check == false) { errors++; }
                                else
                                {
                                    extracted++;
                                    overwritten++;
                                }
                            }
                            else if (ExistingFileDialog.DialogResult == ExistingFileDialog.Result.Rename) // Rename new
                            {
                                fileName = Utils.GetNextFileName(fileName);
                                check = Save(item.FileReference, fileName, false);
                                if (check == false) { errors++; }
                                else
                                {
                                    extracted++;
                                    renamed++;
                                }
                            }
                            else if (ExistingFileDialog.DialogResult == ExistingFileDialog.Result.Skip) // Skipp file
                            {
                                skipped++;
                            }
                            else
                            {
                                errors++;
                            }
                        }
                        else
                        {
                            check = Save(item.FileReference, fileName, false);
                            if (check == false) { errors++; }
                            else { extracted++; }                            
                        }
                    }

                    if (errors > 0 || skipped > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        if (errors == 1) { sb.Append("One file could not be extracted.");  }
                        else if (errors > 1) { sb.Append(errors + " files could not be extracted."); }
                        sb.Append("\n");
                        if (skipped == 1) { sb.Append("One file was skipped."); }
                        else if (skipped > 1) { sb.Append(skipped + " files were skipped."); }
                        string message;
                        if (sb[0] == '\n')
                        {
                            message = sb.ToString(1,sb.Length - 1);
                        }
                        else
                        {
                            message = sb.ToString();
                        }
                        CurrentModel.StatusText = extracted + " files extracted (" + overwritten + " overwritten, " + renamed + " renamed), " + skipped + " skipped, " + errors + " not extracted (errors)";
                        MessageBox.Show(message, "Not all files were extracted", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        CurrentModel.StatusText = extracted + " files extracted (" + overwritten + " overwritten, " + renamed + " renamed), " + skipped + " skipped";
                    }
                    if (CurrentModel.ShowInExplorer)
                    {
                        bool open = Utils.ShowInExplorer(ofd.FileName);
                        if (open == false)
                        {
                            MessageBox.Show("The path '" + ofd.FileName + "' could not be opened", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }                                     
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an unexpected error during the extraction:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        /// <summary>
        /// Method to check whether the specified input file exists
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="item">Extractor Item</param>
        /// <param name="fileName">Determined name and path of the existing file as output parameter</param>
        /// <param name="keepFolderStructure">If true, the relative folder of the item will be added to the root folder</param>
        /// <returns>True if the file exists</returns>
        private bool CheckFileExists(string folder, ExtractorItem item, bool keepFolderStructure, out string fileName)
        {
            string separator = Path.DirectorySeparatorChar.ToString();
            char[] chars = new char[] { '/', '\\' };
            if (keepFolderStructure == true)
            {
                folder = folder.TrimEnd(chars) + separator + item.Path.Trim(chars);
            }
            else
            {
                folder = folder.TrimEnd(chars);
            }

            fileName = folder + separator + item.FileName;
            return File.Exists(fileName);
        }

        /// <summary>
        /// Method to save the currently selected entry as file
        /// </summary>
        private void SaveFile()
        {
            try
            {
                ListViewItem item = (ListViewItem)ImagesListView.SelectedItem;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save current File as...";
                sfd.Filter = "All files|*.*";
                sfd.FileName = item.FileName;
                Nullable<bool> result = sfd.ShowDialog();
                if (result == true)
                {
                    Save(item.FileReference, sfd.FileName, true);
                }
                if (CurrentModel.ShowInExplorer)
                {
                    FileInfo fi = new FileInfo(sfd.FileName);
                    bool open = Utils.ShowInExplorer(fi.DirectoryName);
                    if (open == false)
                    {
                        MessageBox.Show("The path '" + fi.DirectoryName + "' could not be opened", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Method to save a single entry as file
        /// </summary>
        /// <param name="item">ExtractorItem to save</param>
        /// <param name="filename">file name for the target file</param>
        /// <param name="writeStatus">If true, the status of the operation will be stated</param>
        /// <returns>True if the file could be saved</returns>
        private bool Save(ExtractorItem item, string filename, bool writeStatus)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (Directory.Exists(fi.DirectoryName) == false)
                {
                    Directory.CreateDirectory(fi.DirectoryName);
                }

                FileStream fs = new FileStream(filename, FileMode.Create);
                item.Stream.Position = 0;
                fs.Write(item.Stream.GetBuffer(), 0, (int)item.Stream.Length);
                fs.Flush();
                fs.Close();
                if (writeStatus == true)
                {
                    CurrentModel.StatusText = "The file was saved as: " + filename;
                }
            }
            catch (Exception e)
            {
                if (writeStatus == true)
                {
                    CurrentModel.StatusText = "Could not save the file: " + e.Message;
                    MessageBox.Show("The file could not be saved", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            return true;
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
    }
}
