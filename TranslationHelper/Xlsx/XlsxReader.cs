/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * TranslationHelper is a library to help with the translation of Media Extractor. It is part of the Media Extractor project.
 * Copyright Raphael Stoeckli © 2025
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using NanoXLSX;
using System;
using System.Collections.Generic;

namespace TranslationHelper.Xlsx
{
    public class XlsxReader
    {

        public static Dictionary<string, TranslationItem> ReadXlsxFile(string filePath)
        {
            Dictionary<string, TranslationItem> entries = new Dictionary<string, TranslationItem>();
            try
            {
                Workbook workbook = Workbook.Load(filePath);
                Worksheet worksheet = workbook.GetWorksheet("Translation");
                if (worksheet == null)
                {
                    Console.WriteLine($"Worksheet 'Translation' not found in {filePath}");
                    return entries;
                }
                for (int row = 1; row <= worksheet.GetLastDataRowNumber(); row++)
                {
                    string key = GetCellValue(worksheet, row, 0);
                    string defaultValue = GetCellValue(worksheet, row, 1);
                    string translatedValue = GetCellValue(worksheet, row, 2);
                    string comment = GetCellValue(worksheet, row, 3);
                    if (!string.IsNullOrEmpty(key) && !entries.ContainsKey(key))
                    {
                        entries[key] = new TranslationItem
                        {
                            Key = key,
                            DefaultValue = defaultValue,
                            TranslatedValue = translatedValue,
                            Comment = comment
                        };
                    }
                }
                Console.WriteLine($"Successfully read {entries.Count} entries from {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
            }
            return entries;
        }

        private static string GetCellValue(Worksheet worksheet, int row, int column)
        {
            Cell cell = worksheet.GetCell(column, row);
            if (cell != null && cell.DataType != Cell.CellType.EMPTY)
            {
                return cell.Value.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
