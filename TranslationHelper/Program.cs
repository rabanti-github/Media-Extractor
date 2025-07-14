/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * TranslationHelper is a library to help with the translation of Media Extractor. It is part of the Media Extractor project.
 * Copyright Raphael Stoeckli © 2025
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using Mono.Options;
using System;
using System.Collections.Generic;
using TranslationHelper.Csv;
using TranslationHelper.Markdown;
using TranslationHelper.Resx;
using TranslationHelper.Xlsx;


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
            Console.ReadKey(); // Wait for a key press to ensure the console window stays open during debugging
            bool showHelp = false;
            bool modeResxToCsv = false;
            bool modeResxToXlsx = false;
            bool modeResxToMd = false;
            bool modeMdToCsv = false;
            bool modeMdToXlsx = false;
            bool modeMdToResx = false;
            bool modeXlsxToResx = false;
            bool modeCsvToResx = false;
            bool useSourceTexts = false;

            string inputResxDefault = null;
            string inputResxLang = null;
            string inputPath = null;
            string outputPath = null;
            string sourceLanguage = null;
            string targetLanguage = null;
            string csvDelimiter = null;

            OptionSet opt = new OptionSet()
            {
                { "h|help", "Shows this message", v => showHelp = v != null },

                // Modes (only one mode should be chosen; you can validate later)
                { "c|csv", "Read resx and save as CSV", v => { if (v != null) modeResxToCsv = true; } },
                { "x|xlsx", "Read resx and save as XLSX", v => { if (v != null) modeResxToXlsx = true; } },
                { "m|md", "Read resx and save as markdown for LLM translation", v => { if (v != null) modeResxToMd = true; } },
                { "M|mdin", "Read LLM generated markdown and save as CSV", v => { if (v != null) modeMdToCsv = true; } },
                { "X|mdxlsx", "Read LLM generated markdown and save as XLSX", v => { if (v != null) modeMdToXlsx = true; } },
                { "R|mdresx", "Read LLM generated markdown and save as resx", v => { if (v != null) modeMdToResx = true; } },
                { "Z|xlsxresx", "Read XLSX file and save as resx", v => { if (v != null) modeXlsxToResx = true; } },
                { "C|csvresx", "Read CSV file and save as resx", v => { if (v != null) modeCsvToResx = true; } },

                // Input paths
                { "n|neutral=", "Input path to the default (neutral) resx (required if -l specified)", v => inputResxDefault = v },
                { "l|lang=", "Optional input path to language specific resx", v => inputResxLang = v },
                { "i|input=", "Input path to LLM markdown file, CSV or XLSX (only valid in markdown input modes -M,-X,-R,-Z or -C)", v => inputPath = v },
                { "r|usesource", "If set, the source texts will be used when importing a translation from CSV or XLSX. If not set (default), the translated values will be used", v => { if (v != null) useSourceTexts = true; } },

                // Output
                { "o|output=", "Output path and file name", v => outputPath = v },

                // Language options
                { "t|target=", "Target language for translation (e.g. 'Italian', 'Mandarin, Simplified'; required if -l specified)", v => targetLanguage = v },
                { "s|source=", "Optional source language for translation (e.g. 'French'). Default is 'English'", v => sourceLanguage = v },
             
                // CSV options
                { "d|delimiter=", "Optional CSV delimiter (default is ',')", v => csvDelimiter = v }
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

            if (modeResxToCsv || modeResxToXlsx || modeResxToMd)
            {
                if (string.IsNullOrEmpty(inputResxDefault))
                {
                    Console.WriteLine("You must specify at least the default input resx file with -n and an optional language specific, with -l");
                    ShowHelp(opt);
                    return;
                }
                if (string.IsNullOrEmpty(outputPath))
                {
                    Console.WriteLine("You must specify an output path with -o");
                    ShowHelp(opt);
                    return;
                }
            }
            if (modeMdToCsv || modeMdToXlsx || modeMdToResx)
            {
                if (string.IsNullOrEmpty(inputPath))
                {
                    Console.WriteLine("You must specify an input markdown file with -i");
                    ShowHelp(opt);
                    return;
                }
                if (string.IsNullOrEmpty(outputPath))
                {
                    Console.WriteLine("You must specify an output path with -o");
                    ShowHelp(opt);
                    return;
                }
            }
            if (modeCsvToResx || modeXlsxToResx)
            {
                if (string.IsNullOrEmpty(inputPath))
                {
                    Console.WriteLine("You must specify an input file (XLSX or CSV) with -i");
                    ShowHelp(opt);
                    return;
                }
                if (string.IsNullOrEmpty(outputPath))
                {
                    Console.WriteLine("You must specify an output path with -o");
                    ShowHelp(opt);
                    return;
                }
            }
            if (modeResxToMd)
            {
                if (string.IsNullOrEmpty(targetLanguage))
                {
                    Console.WriteLine("You must specify a target language with -t if you have defined -m to generate a MD file for LLM translation");
                    ShowHelp(opt);
                    return;
                }
                if (!string.IsNullOrEmpty(inputResxLang) && string.IsNullOrEmpty(sourceLanguage))
                {
                    Console.WriteLine("You must specify a source language with -s if you have defined a language specific resx file with -l, if you want to generate a MD file for LLM translation");
                    ShowHelp(opt);
                    return;
                }
            }

            if (modeResxToCsv || modeResxToXlsx || modeResxToMd) 
            {
                Dictionary<string, TranslationItem> entries;
                if (string.IsNullOrEmpty(inputResxLang))
                {
                    entries = ResxParser.ReadResxFile(inputResxDefault, null);
                }
                else
                {
                    entries = ResxParser.ReadResxFile(inputResxDefault, inputResxLang);
                }
                if (modeResxToCsv)
                {
                    Console.WriteLine("Writing CSV file to " + outputPath);
                    CsvWriter.WriteCsvFile(outputPath, entries, csvDelimiter);
                }
                else if (modeResxToXlsx)
                {
                    Console.WriteLine("Writing XLSX file to " + outputPath);
                    XlsxWriter.WriteXlsxFile(outputPath, entries);
                }
                else if (modeResxToMd)
                {
                    Console.WriteLine("Writing markdown file for LLM translation to " + outputPath);
                    MdWriter.WriteMarkdownFile(outputPath, sourceLanguage, targetLanguage, entries);
                }
                     
            }
            else if (modeMdToCsv || modeMdToXlsx || modeMdToResx)
            {
                Dictionary<string, TranslationItem> entries = MdReader.ReadMarkdownFile(inputPath);
                if (modeMdToCsv)
                {
                    Console.WriteLine("Writing CSV file to " + outputPath);
                    CsvWriter.WriteCsvFile(outputPath, entries, csvDelimiter);
                }
                else if (modeMdToXlsx)
                {
                    Console.WriteLine("Writing XLSX file to " + outputPath);
                    XlsxWriter.WriteXlsxFile(outputPath, entries);
                }
                else if (modeMdToResx)
                {
                    Console.WriteLine("Writing resx file to " + outputPath);
                    ResxWriter.WriteResxFile(outputPath, entries, true);
                }
            }
            else if (modeXlsxToResx || modeCsvToResx)
            {
                Dictionary<string, TranslationItem> entries = null;
                if (modeXlsxToResx)
                {
                    Console.WriteLine("Reading XLSX file from " + inputPath);
                    entries = XlsxReader.ReadXlsxFile(inputPath);
                }
                else if (modeCsvToResx)
                {
                    Console.WriteLine("Reading CSV file from " + inputPath);
                    entries = CsvReader.ReadCsvFile(inputPath, csvDelimiter);
                }
                if (entries != null)
                {
                    Console.WriteLine("Writing resx file to " + outputPath);
                    ResxWriter.WriteResxFile(outputPath, entries, !useSourceTexts);
                }
            }
            else
            {
                ShowHelp(opt);
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
            Console.WriteLine("TranslationHelper is a tool to extract, convert and update translation data between .resx files, CSV/XLSX spreadsheets, and Markdown for LLM processing.\n");

            Console.WriteLine("Usage:");
            Console.WriteLine("  TranslationHelper [OPTIONS]\n");

            Console.WriteLine("Modes (choose one):");
            Console.WriteLine("  -c, --csv           Read .resx and save as CSV");
            Console.WriteLine("  -x, --xlsx          Read .resx and save as XLSX");
            Console.WriteLine("  -m, --md            Read .resx and save as Markdown for LLM translation");
            Console.WriteLine("  -M, --mdin          Read LLM-generated Markdown and save as CSV");
            Console.WriteLine("  -X, --mdxlsx        Read LLM-generated Markdown and save as XLSX");
            Console.WriteLine("  -R, --mdresx        Read LLM-generated Markdown and save as .resx");
            Console.WriteLine("  -Z, --xlsxresx      Read XLSX file and save as .resx");
            Console.WriteLine("  -C, --csvresx       Read CSV file and save as .resx\n");

            Console.WriteLine("Input options:");
            Console.WriteLine("  -n, --neutral=PATH  Path to default (neutral) .resx file (required if -l is used)");
            Console.WriteLine("  -l, --lang=PATH     Path to language-specific .resx file");
            Console.WriteLine("  -i, --input=PATH    Input file (Markdown, CSV, or XLSX depending on mode)\n");

            Console.WriteLine("Output options:");
            Console.WriteLine("  -o, --output=PATH   Output file path\n");

            Console.WriteLine("Language options:");
            Console.WriteLine("  -t, --target=NAME   Target language name (e.g. 'Italian', 'German')");
            Console.WriteLine("  -s, --source=NAME   Source language name (default is 'English')\n");

            Console.WriteLine("CSV options:");
            Console.WriteLine("  -d, --delimiter=CHAR    Optional CSV delimiter (default is ',')\n");

            Console.WriteLine("Help:");
            Console.WriteLine("  -h, --help          Show this help message\n");

            Console.WriteLine("Examples:");
            Console.WriteLine("  Extract neutral and German .resx into Excel:");
            Console.WriteLine("    TranslationHelper -d=Resources.resx -l=Resources.de-DE.resx -x=de.xlsx");
            Console.WriteLine("  Export to Markdown for LLM translation:");
            Console.WriteLine("    TranslationHelper -d=Resources.resx -l=Resources.es-ES.resx -m=translation.md -t=Spanish");
            Console.WriteLine("  Import Markdown and generate new .resx:");
            Console.WriteLine("    TranslationHelper -i=translation.md -R -o=Resources.fr-FR.resx\n");

            Console.WriteLine("All options:");
            opt.WriteOptionDescriptions(Console.Out);
        }


    }
}
