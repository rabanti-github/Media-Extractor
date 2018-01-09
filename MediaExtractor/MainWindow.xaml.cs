using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Reflection;

namespace MediaExtractor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ViewModel CurrentModel { get; set; }
        private Extractor CurrentExtractor{ get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.CurrentModel = new ViewModel();
            this.DataContext = this.CurrentModel;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            this.Title = versionInfo.ProductName;
            HandleArguments();
        }

        private void HandleArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                string fileName = args[1];
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

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Office File...";
            ofd.DefaultExt = ".docx";
            ofd.Filter = "All Office Formats|*.docx;*.dotx;*.docm;*.dotm;*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|Word documents|*.docx;*.dotx;*.docm;*.dotm|Excel documents|*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm|PowerPoint documents|*.pptx;*.pptm;*.potx;*.potm;*.ppsx;*.ppsm|All files|*.*";
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

        public void ChangeCursor(Cursor cursor)
        {
            this.Dispatcher.Invoke
                (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(()=> this.Cursor = cursor)
                );
        }

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
                reference.CurrentExtractor.Extract(Extractor.ImageFormat.all);
                if (reference.CurrentExtractor.HasErrors == true)
                {
                    reference.CurrentModel.StatusText = "The file could not be displayed: " + reference.CurrentExtractor.LastError;
                    reference.CurrentExtractor.ResetErrors();
                    return;
                }
                RecalculateListViwItems(reference);
            }
            reference.CurrentModel.StatusText = "File was loaded";
            reference.CurrentModel.Progress = 0;
            reference.ChangeCursor(c);
        }


        private static void RecalculateListViwItems(MainWindow reference)
        {
                Application.Current.Dispatcher.Invoke((Action)(delegate
                {
                    try
                    {
                        int i = 0;
                        reference.CurrentModel.ClearListView();
                        foreach (Extractor.ExtractorItem item in reference.CurrentExtractor.Images)
                        {
                            if ((item.IsImage == true && reference.imageFilterMenuItem.IsChecked == true) || (item.IsImage == false && reference.otherFilterMenuItem.IsChecked == true))
                            {
                                reference.CurrentModel.ListViewItems.Add(new ListViewItem() { FileName = item.FileName, FileExtension = item.FileExtension, FileReference = item });
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


        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor c = this.Cursor;
            this.CurrentModel.StatusText = "Loading image... Please wait";
            this.Cursor = Cursors.Wait;
            try
            {
                ListViewItem item = (ListViewItem)this.imagesListView.SelectedItem;
                BitmapImage img;
                CurrentExtractor.GetImageSourceByName(item.FileName, out img);

                if (CurrentExtractor.HasErrors == true)
                {
                    this.CurrentModel.StatusText = "Image could not be loaded: " + this.CurrentExtractor.LastError;
                    this.imageBox.Source = null;
                    this.CurrentExtractor.ResetErrors();
                }
                else
                {
                    this.imageBox.Source = img;
                    this.CurrentModel.StatusText = item.FileName + " loaded";
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

        private void imageFilterMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        private void otherFilterMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        private void imageFilterMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        private void otherFilterMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.RecalculateListViwItems(this);
        }

        private void quitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void openFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void saveAllFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveAllFiles();
        }

        private void saveFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void saveAllFilesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAllFiles();
        }

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
                    List<Extractor.ExtractorItem> items = new List<Extractor.ExtractorItem>();
                    bool duplicatesFound = false;
                    bool overwriteFiles = false;
                    string fileName;
                    foreach (MediaExtractor.ListViewItem item in this.CurrentModel.ListViewItems)
                    {
                        items.Add(item.FileReference);
                        if (CheckFileExists(ofd.FileName, item.FileReference, out fileName) == true && duplicatesFound == false)
                        {
                            MessageBoxResult res2 = MessageBox.Show("At least one existing file was found in the folder.\nShall a dialog for each file be displayed (yes) or all files be overwritten (no)?", "Existing files", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            if (res2 == MessageBoxResult.No)
                            {
                                overwriteFiles = true;
                            }
                            else if (res2 == MessageBoxResult.Yes)
                            {
                                overwriteFiles = false;
                            }
                            else
                            {
                                this.CurrentModel.StatusText = "The save process was canceled";
                                return;
                            }
                            duplicatesFound = false;

                          //  Save(item.FileReference, sfd.FileName, true);
                        }
                    }
                    bool errorsFound = false;
                    bool check;
                    MessageBoxResult res3;
                    foreach (Extractor.ExtractorItem item in items)
                    {
                        
                        if (CheckFileExists(ofd.FileName, item, out fileName) == true && overwriteFiles == false)
                        {
                           res3 = MessageBox.Show("The file " + item.FileName + " already exists.\nOverwrite (Yes), rename (no) or ?", "Existing file", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            if (res3 == MessageBoxResult.No)
                            {
                                continue;
                            }
                            else if (res3 == MessageBoxResult.Cancel)
                            {
                                this.CurrentModel.StatusText = "The save process was canceled";
                                return;
                            }
                        }
                        check = Save(item, fileName, false);
                        if (check == false) { errorsFound = true; }

                        if (errorsFound == true)
                        {
                            this.CurrentModel.StatusText = "Errors occurred during saving";
                            MessageBox.Show("Errors occurred during saving", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            this.CurrentModel.StatusText = "All files were saved successfully";
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }

        private bool CheckFileExists(string folder, Extractor.ExtractorItem item, out string fileName)
        {
            char[] chars = new char[] { '/', '\\' };
            folder = folder.TrimEnd(chars);
            fileName = folder + System.IO.Path.DirectorySeparatorChar.ToString() + item.FileName;
            return File.Exists(fileName);
        }

        private void SaveFile()
        {
            try
            {
                MediaExtractor.ListViewItem item = (MediaExtractor.ListViewItem)this.imagesListView.SelectedItem;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save current File as...";
                //ofd.DefaultExt = ".docx";
                sfd.Filter = "All files|*.*";
                sfd.FileName = item.FileName;
                Nullable<bool> result = sfd.ShowDialog();
                if (result == true)
                {
                    Save(item.FileReference, sfd.FileName, true);
                }
            }
            catch
            {
                return;
            }
        }

        private bool Save(Extractor.ExtractorItem item, string filename, bool writeStatus)
        {
            try
            {
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

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
           string name = versionInfo.ProductName;
           string version = versionInfo.ProductVersion.ToString();
            MessageBox.Show(name + " v" + version + "\n--------------------------\nAuthor: Raphael Stoeckli\nLicense: MIT", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
