/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System.Collections.Generic;

namespace InstallerBootstrap
{
    using InstallerBootstrap.DTO;
    using System;
    using System.Reflection;

    public class Program
    {
        protected Program()
        {
            // Do not instantiate
        }

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: InstallerBootstrap <templateInputPath> <installerFileOutputPath> <assembyPath> <assemblyFile>");
                return;
            }

            string templateInputPath = args[0];
            string installerFileOutputPath = args[1];
            string assembyPath = args[2];
            string assembyFile = args[3];

            try
            {
                string assemblyFullPath = assembyPath + "\\" + assembyFile;
                Assembly mediaExtractorAssembly = Assembly.LoadFile(assemblyFullPath);
                List<Section> sections = SettingsParser.ParseConfiguration(assemblyFullPath + ".config");

                TemplateProcessor templateProcessor = new TemplateProcessor(templateInputPath);
                templateProcessor.SetAppName("Media Extractor");
                templateProcessor.SetAppVersion(GetFormattedVersion(mediaExtractorAssembly.GetName().Version, "."));
                templateProcessor.SetAppUrl(GetSettingsValue(sections, false, "Website"));
                templateProcessor.SetAppSupportUrl(GetSettingsValue(sections, true, "Support"));
                templateProcessor.SetAppExeName(assembyFile);
                //TODO ensure that these files are there or provide them on build
                templateProcessor.SetLicenseFile(".\\Resources\\MIT-License.rtf");
                templateProcessor.SetInstallIconFile(".\\Resources\\media_extractor_installer.ico");
                templateProcessor.SetChangeLogFile("changelog.txt");
                templateProcessor.SetOutputBaseFilename("Media-Extractor-Setup_" + GetFormattedVersion(mediaExtractorAssembly.GetName().Version, "_"));
                templateProcessor.SetMainExecutable(assembyPath, assembyFile);
                templateProcessor.SetAssemblyPath(assembyPath);
                templateProcessor.SaveFile(installerFileOutputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static string GetSettingsValue(List<Section> sections, bool userSettings, string key)
        {
            return sections.Find(x => x.IsUserSection == userSettings).Settings.Find(x => x.Name == key).Value;
        }

        private static string GetFormattedVersion(Version version, string delimiter)
        {
            string formattedVersion = $"{version.Major}{delimiter}{version.Minor}";

            if (version.Build > 0)
            {
                formattedVersion += $"{delimiter}{version.Build}";
            }

            if (version.Revision > 0)
            {
                formattedVersion += $"{delimiter}{version.Revision}";
            }
            return formattedVersion;
        }
    }

}
