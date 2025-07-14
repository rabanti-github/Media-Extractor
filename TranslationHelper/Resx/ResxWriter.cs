/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * TranslationHelper is a library to help with the translation of Media Extractor. It is part of the Media Extractor project.
 * Copyright Raphael Stoeckli © 2025
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.Generic;
using System.Resources.NetStandard;


namespace TranslationHelper.Resx
{
    public class ResxWriter
    {
        public static void WriteResxFile(string outputPath, Dictionary<string, TranslationItem> entries, bool useTranslatedTexts)
        {
            try
            {
                using (var writer = new ResXResourceWriter(outputPath))
                {
                    foreach (var entry in entries.Values)
                    {
                        if (useTranslatedTexts)
                        {
                            writer.AddResource(entry.Key, entry.TranslatedValue);
                        }
                        else
                        {
                            writer.AddResource(entry.Key, entry.DefaultValue);
                        }

                        if (!string.IsNullOrEmpty(entry.Comment))
                        {
                            writer.AddMetadata(entry.Key, entry.Comment);
                        }
                    }
                    writer.Generate();
                }
                Console.WriteLine($"Successfully wrote resx file to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing resx file {outputPath}: {ex.Message}");
            }
        }
    }
}
