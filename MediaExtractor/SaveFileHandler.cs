/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2020
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using AdonisUI.Controls;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Text;

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
            SaveFileRange(items, I18n.T(I18n.Key.DialogSaveAllTitle));
            
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
                SaveFileRange(CurrentModel.SelectedItems, I18n.T(I18n.Key.DialogSaveSelectedTitle));
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
                sfd.Title = I18n.T(I18n.Key.DialogSaveCurrentTitle);
                sfd.Filter = I18n.T(I18n.Key.DialogSaveFilter); // All files|*.*
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
                    if (!open)
                    {
                        MessageBox.Show(I18n.R(I18n.Key.TextSaveError, fi.DirectoryName), I18n.T(I18n.Key.DialogErrorTitle), MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
                                CurrentModel.StatusText = I18n.T(I18n.Key.StatusSaveCanceled);
                                MessageBox.Show(I18n.T(I18n.Key.StatusSaveCanceled), I18n.T(I18n.Key.DialogCancelTitle), MessageBoxButton.OK, MessageBoxImage.Information);
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
                        if (errors == 1) { sb.Append(I18n.T(I18n.Key.TextErrorOneFile)); }
                        else if (errors > 1) { sb.Append(I18n.R(I18n.Key.TextErrorMultipleFiles, errors)); }
                        sb.Append("\n");
                        if (skipped == 1) { sb.Append(I18n.T(I18n.Key.TextSkippedOneFile)); }
                        else if (skipped > 1) { sb.Append(I18n.R(I18n.Key.TextSkippedMultipleFiles, skipped)); }
                        string message;
                        if (sb[0] == '\n')
                        {
                            message = sb.ToString(1, sb.Length - 1);
                        }
                        else
                        {
                            message = sb.ToString();
                        }
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusSaveErrorSummary, extracted, overwritten, renamed, skipped, errors);
                        MessageBox.Show(message, I18n.T(I18n.Key.DialogSaveErrors), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        CurrentModel.StatusText = I18n.R(I18n.Key.StatusSaveSummary, extracted, overwritten, renamed, skipped);
                    }
                    if (CurrentModel.ShowInExplorer)
                    {
                        bool open = Utils.ShowInExplorer(ofd.FileName);
                        if (open == false)
                        {
                            MessageBox.Show(I18n.R(I18n.Key.DialogExplorerError, ofd.FileName), I18n.T(I18n.Key.DialogErrorTitle), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(I18n.T(I18n.Key.DialogUnexpectedError) + "\n" + ex.Message, I18n.T(I18n.Key.DialogErrorTitle), MessageBoxButton.OK, MessageBoxImage.Error);
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
                    CurrentModel.StatusText = I18n.R(I18n.Key.StatusSaveSuccess, filename);
                }
            }
            catch (Exception e)
            {
                if (writeStatus == true)
                {
                    CurrentModel.StatusText = I18n.R(I18n.Key.StatusSaveFailure, e.Message);
                    MessageBox.Show(I18n.T(I18n.Key.DialogSaveFailure), I18n.T(I18n.Key.DialogErrorTitle), MessageBoxButton.OK, MessageBoxImage.Information);
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
