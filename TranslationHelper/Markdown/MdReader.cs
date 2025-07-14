/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * TranslationHelper is a library to help with the translation of Media Extractor. It is part of the Media Extractor project.
 * Copyright Raphael Stoeckli © 2025
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TranslationHelper.Markdown
{
    public class MdReader
    {
        public static Dictionary<string, TranslationItem> ReadMarkdownFile(string filePath)
        {
            Dictionary<string, TranslationItem> entries = new Dictionary<string, TranslationItem>();
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                bool inTable = false;
                foreach (string rawLine in lines)
                {
                    string line = rawLine.Trim();

                    // Start detecting table when header is found
                    if (!inTable)
                    {
                        if (line.StartsWith("|") && line.ToLower().Contains("key") && line.ToLower().Contains("text"))
                        {
                            inTable = true;
                        }
                        continue;
                    }

                    // Skip separator line
                    if (line.StartsWith("|") && line.Replace(" ", "").StartsWith("|---"))
                        continue;

                    // Exit if the line doesn't start with a pipe (assume table ends)
                    if (!line.StartsWith("|"))
                        break;

                    // Parse table row
                    string[] cells = line.Split('|')
                                         .Select(cell => cell.Trim())
                                         .ToArray();

                    if (cells.Length < 3)
                        continue; // Malformed line

                    string key = UnescapeMarkdown(cells[1]);
                    string text = UnescapeMarkdown(cells[2]);
                    // Optional: string context = UnescapeMarkdown(cells[3]); // Currently ignored

                    if (!string.IsNullOrWhiteSpace(key) && !entries.ContainsKey(key))
                    {
                        entries[key] = new TranslationItem
                        {
                            Key = key,
                            TranslatedValue = text
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading markdown file {filePath}: {ex.Message}");
            }
            return entries;
        }

        private static string UnescapeMarkdown(string input)
        {
            return input.Replace("\\n", "\n").Replace("`|", "|");
        }
    }
}
