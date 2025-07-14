/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * TranslationHelper is a library to help with the translation of Media Extractor. It is part of the Media Extractor project.
 * Copyright Raphael Stoeckli © 2025
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using System.Resources.NetStandard;

namespace TranslationHelper.Resx
{
    public class ResxParser
    {
        public static Dictionary<string, TranslationItem> ReadResxFile(string neutralResxFilePath, string inputLanguageResxFilePath)
        {
            Dictionary<string, TranslationItem> entries = new Dictionary<string, TranslationItem>();
            ReadResxFile(ref entries, neutralResxFilePath);
            if (!string.IsNullOrEmpty(inputLanguageResxFilePath))
            {
                ReadResxFile(ref entries, inputLanguageResxFilePath);
            }

            return entries;
        }

        private static void ReadResxFile(ref Dictionary<string, TranslationItem> entries, string resxFilePath)
        {
            using (var reader = new ResXResourceReader(resxFilePath))
            {
                reader.UseResXDataNodes = true;
                foreach (DictionaryEntry entry in reader)
                {
                    var node = (ResXDataNode)entry.Value;
                    string key = (string)entry.Key;
                    string value = node.GetValue((ITypeResolutionService)null)?.ToString() ?? string.Empty;
                    string comment = node.Comment ?? string.Empty;
                    if (!entries.ContainsKey(key))
                    {
                        entries[key] = new TranslationItem
                        {
                            Key = key,
                            DefaultValue = value,
                            Comment = comment
                        };
                    }
                    else
                    {
                        if (value != null && value != string.Empty)
                        {
                            entries[key].TranslatedValue = value;
                        }
                        if (comment != null && comment != string.Empty)
                        {
                            entries[key].Comment = comment;
                        }
                    }
                }
            }
        }
    }
}
