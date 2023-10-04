/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using Mono.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;


namespace TranslationHelper
{
    /// <summary>
    /// Main class
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Arguments in the format of Mono.Options</param>
        static void Main(string[] args)
        {
            bool showHelp = false;
            string excelOutputPath = null;
            string excelInputPath = null;
            string markdownPath = null;
            string defaultPath = null;
            string commentsPath = null;
            string locale = "en";

            OptionSet opt = new OptionSet()
            {
                { "h|help", "Shows this message", v => showHelp = v != null },
                { "l|locale=", "Defines the locale to be written (e.g. 'en', 'de-DE'). If not valid or defined, the application default (en) will be used", v => locale = v },
                { "x|xlsx=", "Writes an Excel file with all translation strings in the defined locale at the defined path" , v => excelOutputPath = v  },
                { "i|input=", "Reads an Excel file with all translation strings from the defined path. The structure must be the same as generated with 'x'" , v => excelInputPath = v  },
                { "m|markdown=", "Writes a markdown text based on a loaded Excel file at the defined path. The 'i' flag must be defined as well" , v => markdownPath = v },
                { "d|default=", "Writes a markdown text only with the default terms to the specified path", v => defaultPath = v},
                { "c|comments=", "Reads an Excel file with term comments, where column A is the translation key and B the comment (starting with row 2). These comments will overrode existing ones", v => commentsPath = v}
            };

            try
            {
              opt.Parse(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid arguments:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try 'TranslationHelper --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(opt);
                return;
            }

            if (string.IsNullOrEmpty(excelOutputPath) && (string.IsNullOrEmpty(excelInputPath) || string.IsNullOrEmpty(markdownPath)))
            {
                ShowHelp(opt);
                return;
            }

            if (string.IsNullOrEmpty(locale))
            {
                locale = "en";
                Console.WriteLine("Reset invalid locale to '" + locale + "'");
            }
            else
            {
                Console.WriteLine("Set locale as '" + locale + "'");
            }
            // Set default locale
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);

            List<TranslationItem> commentsItems = new List<TranslationItem>();
            if (!string.IsNullOrEmpty(commentsPath))
            {
                Console.WriteLine("Attempt to read comments Excel file: " + commentsPath);
                commentsItems = TranslationReader.ReadCommentsWorksheet(commentsPath);
            }

            if (!string.IsNullOrEmpty(excelOutputPath))
            {
                Console.WriteLine("Attempt to write Excel file: " + excelOutputPath);
                TranslationWriter.WriteWorksheet(excelOutputPath, commentsItems);
            }

            List<TranslationItem> translationItems = new List<TranslationItem>();
            if (!string.IsNullOrEmpty(excelInputPath))
            {
                Console.WriteLine("Attempt to read Excel file: " + excelInputPath);
                translationItems = TranslationReader.ReadTranslationWorksheet(excelInputPath);
            }

            if (!string.IsNullOrEmpty(markdownPath))
            {
                Console.WriteLine("Attempt to write markdown file: " + markdownPath);
                TranslationWriter.WriteMarkdownTable(translationItems, commentsItems, markdownPath, true);
            }

            if (!string.IsNullOrEmpty(defaultPath))
            {
                Console.WriteLine("Attempt to write default markdown file: " + defaultPath);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                using (MemoryStream stream = new MemoryStream())
                {
                    TranslationWriter.WriteWorksheet(stream, commentsItems);
                    stream.Position = 0;
                    translationItems = TranslationReader.ReadTransaltionWorksheet(stream);
                    TranslationWriter.WriteMarkdownTable(translationItems, commentsItems, defaultPath, false);
                }
            }
        }

        /// <summary>
        /// Method to display the help
        /// </summary>
        /// <param name="opt">Option object</param>
        private static void ShowHelp(OptionSet opt)
        {
            Console.WriteLine("Media-Extractor - TranslationHelper");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("TranslationHelper is mainly to export the existing translation strings to an Excel file. Furthermore, an Excel file can be read to generate a markdown file (for Wiki maintenance)\n");
            Console.WriteLine("Usage: TranslationHelper [OPTIONS]");
            Console.WriteLine("Example to write all translation strings to an Excel file:");
            Console.WriteLine("   TranslationHelper -l=de-De -x=C:\\temp\\germanStrings.xlsx");
            Console.WriteLine("Example to read a translation from an Excel file and to write it back to a markdown file:");
            Console.WriteLine("   TranslationHelper -i=C:\\temp\\germanStrings.xlsx -m=C:\\temp\\markdown.md");
            Console.WriteLine("Example to write a default markdown file:");
            Console.WriteLine("   TranslationHelper -d=C:\\temp\\markdown.md");
            Console.WriteLine("Example to write a default markdown file with externally loaded comments:");
            Console.WriteLine("   TranslationHelper -c=C:\\temp\\comments.xlsx -d=C:\\temp\\markdown.md");
            Console.WriteLine("\nOptions:");
            opt.WriteOptionDescriptions(Console.Out);
        }

    }
}
