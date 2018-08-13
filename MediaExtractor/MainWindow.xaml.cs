/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MediaExtractor
{
    /// <summary>
    /// Logic of the MainWindow Class
    /// </summary>
    public partial class MainWindow : Window
    {

        private ViewModel CurrentModel { get; set; }
        private Extractor CurrentExtractor{ get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SetDataContext(new ViewModel());
            //this.CurrentModel = new ViewModel(this);
            //this.DataContext = this.CurrentModel;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            this.Title = versionInfo.ProductName;
            HandleArguments();
        }

        public void SetDataContext(ViewModel model)
        {
            this.CurrentModel = model;
           // model.WindowInstance = this;
            this.DataContext = this.CurrentModel;
        }

        /// <summary>
        /// Method to handle command line arguments (open file as...)
        /// </summary>
        private void HandleArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

             /*
            string x = "";
            for (int i = 0; i < args.Length; i++)
            {
                x = x + "[" + i + "]" + args[i] + "\n";
            }
            MessageBox.Show(x);
            */

            if (args != null && args.Length > 1)
            {
                string fileName = args[1];
                if (fileName.ToLower() == "null" && args.Length > 2) // Language set
                {
                    I18N.AvailableCultures culture = I18N.GetCultureByString(args[2]);
                    this.CurrentModel.SetLanguage(culture);
                    return;
                } 
                if (File.Exists(fileName) == false) { return; }
                else
                {
                    this.CurrentModel.FileName = args[1];
                    this.CurrentModel.StatusText = "Loading file... Please wait";
                    Thread t = new Thread(MainWindow.LoadFile);
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
                this.CurrentModel.FileName = ofd.FileName;
                this.CurrentModel.StatusText = "File loaded: " + this.CurrentModel.FileName;
                this.CurrentModel.StatusText = "Loading file... Please wait";
                Thread t = new Thread(MainWindow.LoadFile);
                t.Start(this);
            }
        }


        /// <summary>
        /// Method to change the currently displayed cursor
        /// </summary>
        /// <param name="cursor">Cursor to show</param>
        public void ChangeCursor(Cursor cursor)
        {
            this.Dispatcher.Invoke
                (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(()=> this.Cursor = cursor)
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
                    catch (Exception e)
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
                Application.Current.Dispatcher.Invoke((Action)(delegate
                {
                    try
                    {
                        int i = 0;
                        reference.CurrentModel.ClearListView();
                        ListViewItem lItem;
                        foreach (ExtractorItem item in reference.CurrentExtractor.EmbeddedFiles)
                        {
                            if ((item.IsImage == true && reference.imageFilterMenuItem.IsChecked == true) || (item.IsImage == false && reference.otherFilterMenuItem.IsChecked == true))
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
                }));
        }

        /// <summary>
        /// Sets the image preview visible
        /// </summary>
        public void SetImagePreviewVisible()
        {
            imageBox.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Sets the text preview visible
        /// </summary>
        public void SetTextPreviewVisible()
        {
            textBox.Visibility = Visibility.Visible;
            imageBox.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Event Method to handle a changed ListView item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor c = this.Cursor;
            this.CurrentModel.StatusText = "Loading embedded file... Please wait";
            this.Cursor = Cursors.Wait;
            try
            {
                ListViewItem item = (ListViewItem)this.imagesListView.SelectedItem;
                if (item.Type == ListViewItem.FileType.image)
                {
                    BitmapImage img;
                    CurrentExtractor.GetImageSourceByName(item.FileName, out img);
                    this.SetImagePreviewVisible();
                    if (CurrentExtractor.HasErrors == true)
                    {
                        this.CurrentModel.StatusText = "Embedded file could not be loaded: " + this.CurrentExtractor.LastError;
                        this.imageBox.Source = null;
                        this.CurrentExtractor.ResetErrors();
                    }
                    else
                    {
                        this.imageBox.Source = img;
                        this.CurrentModel.StatusText = item.FileName + " loaded";
                    }
                }
                else if (item.Type == ListViewItem.FileType.xml || item.Type == ListViewItem.FileType.text)
                {
                    string text;
                    CurrentExtractor.GetGenericTextByName(item.FileName, out text);
                    this.SetTextPreviewVisible();
                    if (CurrentExtractor.HasErrors == true)
                    {
                        this.CurrentModel.StatusText = "Text / XML file could not be loaded: " + this.CurrentExtractor.LastError;
                        this.textBox.Text = string.Empty;
                        this.CurrentExtractor.ResetErrors();
                    }
                    else
                    {
                        this.textBox.Text = text;
                        this.CurrentModel.StatusText = item.FileName + " loaded";
                    }
                }
                else
                {
                    this.SetTextPreviewVisible();
                    this.textBox.Text = string.Empty;
                    // Fallback
                }
            }
            catch(Exception ex)
            { }
            this.Cursor = c;
            if (this.imagesListView.Items.Count == 0)
            {
                this.CurrentModel.SaveStatus = false;
            }
            else
            {
                this.CurrentModel.SaveStatus = true;
            }
        }

        /// <summary>
        /// Menu Event for the image filter item (checked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void imageFilterMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for other files filter (checked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void otherFilterMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for the image filter item (unchecked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void imageFilterMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for other files filter (unchecked)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void otherFilterMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        /// <summary>
        /// Menu Event for the exit item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void quitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Menu Event for the file open item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void openFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        /// <summary>
        /// Menu Event for the save all files item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void saveAllFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveAllFiles();
        }

        /// <summary>
        /// Menu Event for the safe file item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void saveFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        /// <summary>
        /// Menu Event for the about item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            string name = versionInfo.ProductName;
            string version = versionInfo.ProductVersion.ToString();
            MessageBox.Show(name + " v" + version + "\n--------------------------\nAuthor: Raphael Stoeckli\nLicense: MIT", "About", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        /// <summary>
        /// Event for the save all files button
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void saveAllFilesButton_Click(object sender, RoutedEventArgs e)
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
                    
                    string fileName;
                    bool fileExists, check;
                    int errors = 0;
                    int skipped = 0;
                    int extracted = 0;
                    int renamed = 0;
                    int overwritten = 0;
                    FileInfo fi;
                    ExistingFileDialog.ResetDialog();
                    foreach (MediaExtractor.ListViewItem item in this.CurrentModel.ListViewItems)
                    {
                        fileExists = CheckFileExists(ofd.FileName, item.FileReference, this.CurrentModel.KeepFolderStructure, out fileName);
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
                                this.CurrentModel.StatusText = "The save process was canceled";
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
                        this.CurrentModel.StatusText = extracted + " files extracted (" + overwritten + " overwritten, " + renamed + " renamed), " + skipped + " skipped, " + errors + " not extracted (errors)";
                        MessageBox.Show(message, "Not all files were extracted", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        this.CurrentModel.StatusText = extracted + " files extracted (" + overwritten + " overwritten, " + renamed + " renamed), " + skipped + " skipped";
                    }
                    if (this.CurrentModel.ShowInExplorer)
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
                MediaExtractor.ListViewItem item = (MediaExtractor.ListViewItem)this.imagesListView.SelectedItem;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save current File as...";
                sfd.Filter = "All files|*.*";
                sfd.FileName = item.FileName;
                Nullable<bool> result = sfd.ShowDialog();
                if (result == true)
                {
                    Save(item.FileReference, sfd.FileName, true);
                }
                if (this.CurrentModel.ShowInExplorer)
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
                return;
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
                    this.CurrentModel.StatusText = "The file was saved as: " + filename;
                }
            }
            catch (Exception e)
            {
                if (writeStatus == true)
                {
                    this.CurrentModel.StatusText = "Could not save the file: " + e.Message;
                    MessageBox.Show("The file could not be saved", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            return true;
        }

        private void systemLanguageMenuItem_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void lnguage_en_us_MenuItem_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void lnguage_de_de_MenuItem_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
