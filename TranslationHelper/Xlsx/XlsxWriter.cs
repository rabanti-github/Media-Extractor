/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * TranslationHelper is a library to help with the translation of Media Extractor. It is part of the Media Extractor project.
 * Copyright Raphael Stoeckli © 2025
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using NanoXLSX;
using NanoXLSX.Styles;
using System;
using System.Collections.Generic;

namespace TranslationHelper.Xlsx
{
    public class XlsxWriter
    {
        public static void WriteXlsxFile(string filePath, Dictionary<string, TranslationItem> entries)
        {
            try
            {
                Workbook workbook = new Workbook(filePath, "Translation");
                Style headerStyle = BasicStyles.Bold;
                workbook.CurrentWorksheet.CurrentCellDirection = Worksheet.CellDirection.ColumnToColumn;
                workbook.WS.Value("Key", headerStyle);
                workbook.WS.Value("Default Value", headerStyle);
                workbook.WS.Value("Translated Value", headerStyle);
                workbook.WS.Value("Comment", headerStyle);
                workbook.WS.Down();
                foreach (KeyValuePair<string, TranslationItem> entry in entries)
                {
                    workbook.WS.Value(entry.Value.Key);
                    workbook.WS.Value(entry.Value.DefaultValue);
                    workbook.WS.Value(entry.Value.TranslatedValue);
                    workbook.WS.Value(entry.Value.Comment);
                    workbook.WS.Down();
                }
                workbook.Save();
                Console.WriteLine($"Successfully wrote {entries.Count} entries to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file {filePath}: {ex.Message}");
            }
        }
    }
}
