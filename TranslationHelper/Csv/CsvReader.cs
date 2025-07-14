/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * TranslationHelper is a library to help with the translation of Media Extractor. It is part of the Media Extractor project.
 * Copyright Raphael Stoeckli © 2025
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TranslationHelper.Csv
{
    public class CsvReader
    {
        public static Dictionary<string, TranslationItem> ReadCsvFile(string filePath, string delimiter)
        {
            Dictionary<string, TranslationItem> entries = new Dictionary<string, TranslationItem>();

            try
            {
                if (string.IsNullOrEmpty(delimiter))
                {
                    delimiter = ","; // Default delimiter
                }

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = delimiter,
                    Encoding = System.Text.Encoding.UTF8,
                    NewLine = Environment.NewLine,
                    IgnoreBlankLines = true,
                    TrimOptions = TrimOptions.Trim
                }))
                {
                    // Read header
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        var key = csv.GetField("Key");
                        var defaultValue = csv.TryGetField("Default Value", out string def) ? def : "";
                        var translatedValue = csv.TryGetField("Translated Value", out string trans) ? trans : "";
                        var comment = csv.TryGetField("Comment", out string comm) ? comm : "";

                        if (!string.IsNullOrWhiteSpace(key) && !entries.ContainsKey(key))
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
                }

                Console.WriteLine($"Successfully read {entries.Count} entries from {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file {filePath}: {ex.Message}");
            }

            return entries;
        }
    }
}
