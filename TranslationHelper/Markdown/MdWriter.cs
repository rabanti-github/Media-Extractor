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

namespace TranslationHelper.Markdown
{
    public class MdWriter
    {
        private static string PromptTemplate = @"You are a translation expert for computer programs. Translate the following terms from '<SOURCE>' into '<TARGET>' as target language. Use the values of the column ""text"" as source. Use the context from the column ""context"" to determine the appropriate words / terms, if available. Use the commonly used computer terms of the target languages (e.g. ""télécharger"" in French for ""download"", or ""Abbrechen"" in German for ""cancel""). Do not change the values in the column ""key"". Try to keep the length of the translated text roughly at the size as the original '<SOURCE>' texts (translations can be shorter). Create a table as output with the same columns in the same column and row order.
Ensure that all placeholders (e.g. {1}) are present in the translations. Use new lines (\n) in translations in the same way as in the '<SOURCE>' original texts. If you see ""`|"", then a pipe was escaped to keep the input markdown table consistent. Ensure that the output table is not becoming broken, if markdown is used as format. Define the header of the output table exactly as the input table below.

Here is the input table:
| key | text | context |
| --- | --- | --- |
";

        public static void WriteMarkdownFile(string outputPath, string sourceLanguage, string targetLanguage, Dictionary<string, TranslationItem> entries)
        {
            try
            {
                bool userTranslationAsSource = false;
                string source = "English";
                if (!string.IsNullOrEmpty(sourceLanguage)){
                    source = sourceLanguage;
                    userTranslationAsSource = true;
                }
                string prompt = PromptTemplate
                    .Replace("<SOURCE>", source)
                    .Replace("<TARGET>", targetLanguage);

                using (var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
                {
                    writer.Write(prompt);

                    foreach (var entry in entries.Values)
                    {
                        string text;
                        string key = EscapeForMarkdown(entry.Key);
                        string context = EscapeForMarkdown(entry.Comment);
                        if (userTranslationAsSource)
                        {
                            text = EscapeForMarkdown(entry.TranslatedValue);
                        }
                        else
                        {
                            text = EscapeForMarkdown(entry.DefaultValue);
                        }
                        writer.WriteLine($"| {key} | {text} | {context} |");
                    }
                }

                Console.WriteLine($"Successfully wrote markdown table to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing markdown file {outputPath}: {ex.Message}");
            }
        }

        private static string EscapeForMarkdown(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // Escape unescaped pipes to keep markdown table consistent
            string escaped = input.Replace("|", "`|");

            // Replace newlines with literal "\n"
            escaped = escaped.Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "\\n");

            return escaped;
        }
    }
}
