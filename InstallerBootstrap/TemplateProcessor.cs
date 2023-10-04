/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System.IO;

namespace InstallerBootstrap
{
    public class TemplateProcessor
    {
        //TODO add new template values here and and as Set functions below
        private const string APP_NAME = "TPL_MY_APP_NAME";
        private const string APP_VERSION = "TPL_MY_APP_VERSION";
        private const string APP_URL = "TPL_MY_APP_URL";
        private const string APP_SUPPORT_URL = "TPL_MY_APP_SUPPORT_URL";
        private const string APP_EXE_NAME = "TPL_MY_APP_EXE_NAME";
        private const string LICENSE_FILE = "TPL_LICENSE_FILE";
        private const string INSTALL_ICON_FILE = "TPL_INSTALL_ICON_FILE";
        private const string CHANGELOG_FILE = "TPL_CHANGELOG_FILE";
        private const string OUTPUT_BASE_FILENAME = "TPL_OUTPUT_BASE_FILENAME";
        private const string MAIN_EXECUTABLE = "TPL_MAIN_EXE_PATH";
        private const string ASSEMBLY_PATH = "TPL_ASSEMBLY_PATH";


        public string TemplateText { get; private set; }

        public TemplateProcessor(string templatePath)
        {
            TemplateText = File.ReadAllText(templatePath);
        }

        public void SaveFile(string path)
        {
            File.WriteAllText(path, TemplateText);
        }

        public void SetAppName(string appName)
        {
           replaceTemplateVariable(APP_NAME, appName);
        }

        public void SetAppVersion(string appVersion)
        {
            replaceTemplateVariable(APP_VERSION, appVersion);
        }
        public void SetAppUrl(string appUrl)
        {
            replaceTemplateVariable(APP_URL, appUrl);
        }
        public void SetAppSupportUrl(string appSupportUrl)
        {
            replaceTemplateVariable(APP_SUPPORT_URL, appSupportUrl);
        }
        public void SetAppExeName(string appExeName)
        {
            replaceTemplateVariable(APP_EXE_NAME, appExeName);
        }
        public void SetLicenseFile(string licenseFile)
        {
            replaceTemplateVariable(LICENSE_FILE, licenseFile);
        }
        public void SetInstallIconFile(string iconFile)
        {
            replaceTemplateVariable(INSTALL_ICON_FILE, iconFile);
        }

        public void SetChangeLogFile(string changeLogFile)
        {
            replaceTemplateVariable(CHANGELOG_FILE, changeLogFile);
        }

        public void SetOutputBaseFilename(string outputBaseFilename)
        {
            replaceTemplateVariable(OUTPUT_BASE_FILENAME, outputBaseFilename);
        }
        public void SetMainExecutable(string path, string executable)
        {
            if (!path.EndsWith("\\"))
            {
                path = path + "\\";
            }
            replaceTemplateVariable(MAIN_EXECUTABLE, path + executable);

        }
        public void SetAssemblyPath(string assemblyPath)
        {
            if (!assemblyPath.EndsWith("\\"))
            {
                assemblyPath = assemblyPath + "\\";
            }
            replaceTemplateVariable(ASSEMBLY_PATH, assemblyPath);

        }

        private void replaceTemplateVariable(string templateVariable, string replacementValue)
        {
            string variableToReplace = "${" + templateVariable + "}";
            TemplateText = TemplateText.Replace(variableToReplace, replacementValue);
        }

    }
}
