using AdonisUI.Controls;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaExtractor
{
    public class SaveFileHandler
    {

        public enum DefaultSaveMethod
        {
            All,
            Selected
        }

        public DefaultSaveMethod DefaultMethod { get; set; } = DefaultSaveMethod.All;
        private ViewModel CurrentModel { get; set; }

        public SaveFileHandler(ViewModel currentModel)
        {
            this.CurrentModel = currentModel;
            this.CurrentModel.CurrentSaveFileHandler = this;
        }

        public void SaveDefault()
        {
            if (!CurrentModel.SaveAllIsDefault && !CurrentModel.SaveSelectedIsDefault)
            {
                if(CurrentModel.SelectedItems.Length > 0)
                {
                    SaveSelectedFiles();
                }
                else
                {
                    SaveAllFiles();
                }
            }
            else
            {
                if (DefaultMethod == DefaultSaveMethod.All)
                {
                    SaveAllFiles();
                }
                else
                {
                    SaveSelectedFiles();
                }
            }
        }


        /// <summary>
        /// Method to save all files
        /// </summary>
        public void SaveAllFiles()
        {
            ListViewItem[] items = CurrentModel.ListViewItems.ToArray();
            SaveFileRange(items, "Select a Folder to save all Files...");
        }

        /// <summary>
        /// Method to save the selected files
        /// </summary>
        public void SaveSelectedFiles()
        {
            if (CurrentModel.SelectedItems.Length == 1)
            {
                SaveSingleFile(CurrentModel.SelectedItems.First());
            }
            else
            {
                SaveFileRange(CurrentModel.SelectedItems, "Select a Folder to save the selected Files...");
            }
            
        }

        /// <summary>
        /// Method to save the currently selected entry as file
        /// </summary>
        private void SaveSingleFile(ListViewItem item)
        {
            try
            {
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
        /// Method to save all files
        /// </summary>
        private void SaveFileRange(ListViewItem[] items, string dialogMessage)
        {
            try
            {
                CommonOpenFileDialog ofd = new CommonOpenFileDialog();
                ofd.IsFolderPicker = true;
                ofd.Title = dialogMessage;
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
                    //foreach (ListViewItem item in CurrentModel.ListViewItems)
                    foreach(ListViewItem item in items)
                    {
                        fileExists = CheckFileExists(ofd.FileName, item.FileReference, CurrentModel.KeepFolderStructure, out var fileName);
                        if (fileExists == true)
                        {
                            if (ExistingFileDialog.RememberDecision == null || ExistingFileDialog.RememberDecision.Value != true)
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
                            else if (ExistingFileDialog.DialogResult == ExistingFileDialog.Result.Skip) // Skip file
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
                        if (errors == 1) { sb.Append("One file could not be extracted."); }
                        else if (errors > 1) { sb.Append(errors + " files could not be extracted."); }
                        sb.Append("\n");
                        if (skipped == 1) { sb.Append("One file was skipped."); }
                        else if (skipped > 1) { sb.Append(skipped + " files were skipped."); }
                        string message;
                        if (sb[0] == '\n')
                        {
                            message = sb.ToString(1, sb.Length - 1);
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
            catch (Exception ex)
            {
                MessageBox.Show("There was an unexpected error during the extraction:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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



    }
}
