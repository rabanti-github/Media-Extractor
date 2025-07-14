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
    public class CsvWriter
    {

        public static void WriteCsvFile(string filePath, Dictionary<string, TranslationItem> entries, string delimiter)
        {
            try
            {
                if (string.IsNullOrEmpty(delimiter))
                {
                    delimiter = ","; // Default delimiter
                }
                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = delimiter,
                        Encoding = System.Text.Encoding.UTF8,
                        NewLine = Environment.NewLine
                    }))
                    {
                        csv.WriteField("Key");
                        csv.WriteField("Default Value");
                        csv.WriteField("Translated Value");
                        csv.WriteField("Comment");
                        csv.NextRecord();

                        foreach (var entry in entries.Values)
                        {
                            csv.WriteField(entry.Key);
                            csv.WriteField(entry.DefaultValue);
                            csv.WriteField(entry.TranslatedValue);
                            csv.WriteField(entry.Comment);
                            csv.NextRecord();
                        }
                    }

                    Console.WriteLine($"Successfully wrote {entries.Count} entries to {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing CSV file {filePath}: {ex.Message}");
            }
        }

    }
}
