/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using NanoXLSX;
using System;
using System.Collections.Generic;
using System.IO;

namespace TranslationHelper
{
    /// <summary>
    /// Class to read translation related files
    /// </summary>
    public static class TranslationReader
    {
        /// <summary>
        /// Enum to define which data source is loaded
        /// </summary>
        private enum ReadType
        {
            /// <summary>
            /// Data source is a translation table
            /// </summary>
            Translation,
            /// <summary>
            /// Data source is a comment table
            /// </summary>
            Comments
        }

        /// <summary>
        /// Method to read a translation table from an Excel file
        /// </summary>
        /// <param name="fileName">File name of the Excel workbook</param>
        /// <returns>List of translation items</returns>
        public static List<TranslationItem> ReadTranslationWorksheet(string fileName)
        {
            return ReadWorksheet(fileName, ReadType.Translation);
        }

        /// <summary>
        /// Method to read a translation table from a stream
        /// </summary>
        /// <param name="stream">File or memory stream</param>
        /// <returns>List of translation items</returns>
        public static List<TranslationItem> ReadTransaltionWorksheet(Stream stream)
        {
            return ReadWorksheet(stream, ReadType.Translation);
        }

        /// <summary>
        /// Method to read comments from an Excel file
        /// </summary>
        /// <param name="fileName">File name of the Excel workbook</param>
        /// <returns>List of translation items with comments</returns>
        public static List<TranslationItem> ReadCommentsWorksheet(string fileName)
        {
            return ReadWorksheet(fileName, ReadType.Comments);
        }

        /// <summary>
        /// Method to read a row from a translation table
        /// </summary>
        /// <param name="worksheet">Worksheet object as reference</param>
        /// <param name="items">Translation item list as reference</param>
        /// <param name="row">Current row number of the table</param>
        private static void ReadTransaltionRow(Worksheet worksheet, List<TranslationItem> items, int row)
        {
            TranslationItem item = new TranslationItem();
            if (worksheet.HasCell(0, row))
            {
                item.Key = worksheet.GetCell(0, row).Value.ToString();
            }
            if (worksheet.HasCell(1, row))
            {
                item.DefaultValue = worksheet.GetCell(1, row).Value.ToString();
            }
            if (worksheet.HasCell(2, row))
            {
                item.TranslatedValue = worksheet.GetCell(2, row).Value.ToString();
            }
            if (worksheet.HasCell(3, row))
            {
                item.Comment = worksheet.GetCell(3, row).Value.ToString();
            }
            items.Add(item);
        }

        /// <summary>
        /// Method to read a row from a comment table
        /// </summary>
        /// <param name="worksheet">Worksheet object as reference</param>
        /// <param name="items">Translation item list as reference</param>
        /// <param name="row">Current row number of the table</param>
        private static void ReadCommentRow(Worksheet worksheet, List<TranslationItem> items, int row)
        {
            TranslationItem item = new TranslationItem();
            if (worksheet.HasCell(0, row))
            {
                item.Key = worksheet.GetCell(0, row).Value.ToString();
            }
            if (worksheet.HasCell(1, row))
            {
                item.Comment = worksheet.GetCell(1, row).Value.ToString();
            }
            items.Add(item);
        }

        /// <summary>
        /// Method to read a worksheet from a stream
        /// </summary>
        /// <param name="stream">File or memory stream</param>
        /// <param name="type">data source type</param>
        /// <returns>List of translation items</returns>
        private static List<TranslationItem> ReadWorksheet(Stream stream, ReadType type)
        {
            try
            {
                List<TranslationItem> items = new List<TranslationItem>();
                Workbook wb = Workbook.Load(stream);
                int lastRow = wb.CurrentWorksheet.GetLastRowNumber();
                for (int i = 1; i <= lastRow; i++)
                {
                    if (type == ReadType.Comments)
                    {
                        ReadCommentRow(wb.CurrentWorksheet, items, i);
                    }
                    else
                    {
                        ReadTransaltionRow(wb.CurrentWorksheet, items, i);
                    }
                }
                if (type == ReadType.Comments)
                {
                    Console.WriteLine(items.Count + " comment items retrieved");
                }
                else
                {
                    Console.WriteLine(items.Count + " translation items retrieved");
                }
                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not read stream");
                Console.WriteLine(ex.Message);
                return new List<TranslationItem>();
            }
        }

        /// <summary>
        /// Method to read a worksheet from a file
        /// </summary>
        /// <param name="fileName">File name of the Excel file</param>
        /// <param name="type">data source type</param>
        /// <returns>List of translation items</returns>
        private static List<TranslationItem> ReadWorksheet(string fileName, ReadType type)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("The file '" + fileName + "' does not exist");
                return new List<TranslationItem>();
            }
            try
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    return ReadWorksheet(stream, type);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not read worksheet");
                Console.WriteLine(ex.Message);
                return new List<TranslationItem>();
            }
        }

    }
}
