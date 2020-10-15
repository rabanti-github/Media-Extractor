/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2020
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using NanoXLSX;
using NanoXLSX.Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TranslationHelper
{
    /// <summary>
    /// Class to write translation related files
    /// </summary>
    public static class TranslationWriter
    {
        /// <summary>
        /// Method to write a worksheet of translation items into a file
        /// </summary>
        /// <param name="fileName">File name of the Excel file</param>
        /// <param name="comments">Optional list of comments (can be empty)</param>
        public static void WriteWorksheet(string fileName, List<TranslationItem> comments)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    WriteWorksheet(stream, comments);
                    stream.Position = 0;
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        stream.CopyTo(fs);
                    }
                    Console.WriteLine("File written: '" + fileName + "'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write file '" + fileName + "'");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Method to write a worksheet of translation items into a stream
        /// </summary>
        /// <param name="stream">File or memory stream</param>
        /// <param name="comments">Optional list of comments (can be empty)</param>
        public static void WriteWorksheet(MemoryStream stream, List<TranslationItem> comments)
        {
            try
            {
                string[] keys = GetTranslationKeys();
                Workbook wb = new Workbook("Default");
                Style style = NanoXLSX.Styles.BasicStyles.Bold;
                wb.CurrentWorksheet.CurrentCellDirection = Worksheet.CellDirection.ColumnToColumn;
                wb.CurrentWorksheet.AddNextCell("Name", style);
                wb.CurrentWorksheet.AddNextCell("Value", style);
                wb.CurrentWorksheet.AddNextCell("Translation", style);
                wb.CurrentWorksheet.AddNextCell("Comment", style);
                wb.CurrentWorksheet.GoToNextRow();
                foreach (string key in keys)
                {
                    
                    string term = MediaExtractor.Properties.Resources.ResourceManager.GetString(key);
                    wb.CurrentWorksheet.AddNextCell(key);
                    wb.CurrentWorksheet.AddNextCell(term);
                    wb.CurrentWorksheet.AddNextCell("");
                    if (comments.Find(x => x.Key == key) != null)
                    {
                        wb.CurrentWorksheet.AddNextCell(comments.Find(x => x.Key == key).Comment);
                    }
                    wb.CurrentWorksheet.GoToNextRow();
                }
                wb.SaveAsStream(stream, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write stream");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Method to write a markdown file
        /// </summary>
        /// <param name="items">List of translation items</param>
        /// <param name="comments">Optional list of comments (can be empty)</param>
        /// <param name="fileName">File name of the markdown file</param>
        /// <param name="writeTranslation">If true, the translation column will be written, other wise left blank</param>
        public static void WriteMarkdownTable(List<TranslationItem>items, List<TranslationItem> comments, string fileName, bool writeTranslation)
        {
            List<string> keys = new List<string>(GetTranslationKeys()); // With system default
            for(int i = 0; i < items.Count; i++)
            {
                string defaultTerm = keys.SingleOrDefault(s => s == items[i].Key);
                if (defaultTerm == null)
                {
                    items[i].DefaultValue = "";
                }
                else
                {
                    items[i].DefaultValue = MediaExtractor.Properties.Resources.ResourceManager.GetString(defaultTerm);
                }
                string comment = items[i].Comment;
                if (comments != null && comments.Count > 0)
                {
                    TranslationItem item = comments.First(x => x.Key == items[i].Key);
                    if (item == null && string.IsNullOrEmpty(comment))
                    {
                        comment = "";
                    }
                    else if (item != null)
                    {
                        comment = item.Comment;
                    }
                }
                items[i].Comment = comment;
            }
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                sw.WriteLine("|  Name (mandatory)  |  Default Text  |  Translation  |  Comment  |");
                sw.WriteLine("| --- | --- | --- | --- |");
                foreach (TranslationItem item in items)
                {
                        if (writeTranslation)
                        {
                            sw.Write("| " + item.Key + " | " + EscapeMarkdown(item.DefaultValue) + " | " + EscapeMarkdown(item.TranslatedValue) + " | " + EscapeMarkdown(item.Comment) + " |\n");
                        }
                        else
                        {
                            sw.Write("| " + item.Key + " | " + EscapeMarkdown(item.DefaultValue) + " |  | " + EscapeMarkdown(item.Comment) + " |\n");
                        }
                }
            }
                Console.WriteLine("Markdown file written: '" + fileName + "'");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write markdown file '" + fileName + "'");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets an array of defined translation keys
        /// </summary>
        /// <returns>String array of keys</returns>
        private static string[] GetTranslationKeys()
        {
            return Enum.GetNames(typeof(MediaExtractor.I18n.Key));
        }

        /// <summary>
        /// Method to escape a string for markdown
        /// </summary>
        /// <param name="value">String to escape</param>
        /// <returns>Escaped string</returns>
        private static string EscapeMarkdown(string value)
        {
            return value.Replace("|", "\\|").Replace("*", "\\*");
        }

    }
}
