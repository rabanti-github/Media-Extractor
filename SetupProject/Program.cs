using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.dialogs;
using WixSharp.Forms;
using WixSharp.logic;
using WixSharp.UI.Forms;
using WixToolset.Dtf.WindowsInstaller;

namespace SetupProject
{
    public static class Program
    {
        public enum SupportedLanguages
        {
            English,
            German,
            French,
            Spanish,
            Japanese
        }

        static public void Main()
        {

            // Force system UI culture explicitly
            
            PropertiesReader propertiesReader = new PropertiesReader();
            var project = new ManagedProject(propertiesReader.ProductName,
                             new Dir(@"%ProgramFiles%\" + propertiesReader.ProductDirectoryName,
                                 new Files(Constants.PROGRAM_FILES_PATH + "*.*"), 
                                 new ExeFileShortcut(propertiesReader.ProductName, "[INSTALLDIR]" + Constants.MAIN_APP_NAME, ""))
                             );

            project.AddProperty(new Property(Constants.PRODUCT_VERSION_KEY, propertiesReader.Version));

            Constants.AddSecureProperty(project, Constants.SecureProperties.RESUME_INSTALLATION);
            Constants.AddSecureProperty(project, Constants.SecureProperties.UI_LANGUAGE);
            Constants.AddSecureProperty(project, Constants.SecureProperties.INSTALLATION_TYPE);
            Constants.AddSecureProperty(project, Constants.SecureProperties.INSTALLATION_FEATURES);
            //Constants.AddSecureProperty(project, Constants.SecureProperties.ADD_STARTMENU_ICON);
            //Constants.AddSecureProperty(project, Constants.SecureProperties.ADD_DESKTOP_ICON);
            // Constants.AddSecureProperty(project, Constants.SecureProperties.ADD_QUICKLAUNCH_ICON);
            //Constants.AddSecureProperty(project, Constants.SecureProperties.REGISTER_EXPLORER);
            Constants.AddSecureProperty(project, Constants.SecureProperties.INSTALLATION_PATH);
            Constants.AddSecureProperty(project, Constants.SecureProperties.SETUP_PID);

            project.GUID = new Guid("6446F2F5-6F37-4DD0-B0DA-6248A5F01EDA");
            project.Version = new Version(propertiesReader.Version);
            project.ControlPanelInfo.ProductIcon = "resources\\media_extractor.ico"; // path to the icon file
            project.LicenceFile = "resources\\MIT-License.rtf"; // path to the license file

            Feature root = new Feature(Constants.INSTALLATION_FEATURE_ROOT, "[InstallationFeatureProgramFilesDescription]", true, false);
            Feature desktop = new Feature(Constants.INSTALLATION_FEATURE_DESKTOP, "[InstallationFeatureDesktopIconDescription]", false, true);
            Feature startMenu = new Feature(Constants.INSTALLATION_FEATURE_STARTMENU, "[InstallationFeatureStartIconDescription]", false, true);
            Feature quickLaunch = new Feature(Constants.INSTALLATION_FEATURE_QUICKLAUNCH, "[InstallationFeatureQuickLaunchIconDescription]", false, true);
            Feature registration = new Feature(Constants.INSTALLATION_FEATURE_EXPLORER, "[InstallationFeatureRegisterExplorerDescription]", false, true);

            root.Add(desktop);
            root.Add(startMenu);
            root.Add(quickLaunch);
            root.Add(registration);

            project.DefaultFeature = root;
            project.DefaultFeature.Display = FeatureDisplay.expand;

            

            project.Scope = InstallScope.perUserOrMachine;
            project.EnableResilientPackage();


            //custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI();


            project.ManagedUI.InstallDialogs.Add<LanguageSelectionDialog>()
                                            .Add<AdaptedWelcomeDialog>()
                                            .Add<AdaptedLicenseDialog>()
                                            .Add<LegacyCheckDialog>()
                                            .Add<CustomSetupTypeDialog>()
                                            //.Add(Dialogs.SetupType)
                                            .Add<AdaptedFeaturesDialog>()
                                            //.Add(Dialogs.Features)
                                            .Add<AdaptedInstallDirDialog>()
                                            //.Add(Dialogs.InstallDir)
                                            //.Add(Dialogs.Progress)
                                            .Add(Dialogs.Exit);

            project.ManagedUI.ModifyDialogs.Add<LanguageSelectionDialog>()
                                           .Add(Dialogs.MaintenanceType)
                                           .Add(Dialogs.Features)
                                           .Add(Dialogs.Progress)
                                           .Add(Dialogs.Exit);


            project.Load += Msi_Load;
            project.Load += Project_Load;
            project.BeforeInstall += Msi_BeforeInstall;
            project.AfterInstall += Msi_AfterInstall;

            project.InitLocalization();
            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            project.BuildMsi();

            var sourceMsi = Path.Combine(Directory.GetCurrentDirectory(), project.OutFileName + ".msi");
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");

            Directory.CreateDirectory(outputDir);
            string outputMsi = Path.Combine(propertiesReader.ProductName + "-" + propertiesReader.FileNameVersion + ".msi");
            if ( System.IO.File.Exists(outputMsi))
            {
                System.IO.File.Delete(outputMsi);
            }
            System.IO.File.Move(sourceMsi, Path.Combine(outputDir, outputMsi));
        }

        private static void InitLocalization(this ManagedProject project)
        {
            project.AddBinary(new Binary(new Id("de_wxl"), "resources\\localization\\WixUI_de-de.wxl"))
                   .AddBinary(new Binary(new Id("en_wxl"), "resources\\localization\\WixUI_en-us.wxl"))
                   .AddBinary(new Binary(new Id("fr_wxl"), "resources\\localization\\WixUI_fr-fr.wxl"))
                   .AddBinary(new Binary(new Id("es_wxl"), "resources\\localization\\WixUI_es-es.wxl"))
                   .AddBinary(new Binary(new Id("jp_wxl"), "resources\\localization\\WixUI_ja-jp.wxl"));

            project.UIInitialized += (SetupEventArgs e) =>
            {
                var runtime = e.ManagedUI.Shell.MsiRuntime();
                var session = e.Session;
                var culture = Thread.CurrentThread.CurrentUICulture.Name;

               var codes = Native.GetPreferredIsoTwoLetterUILanguages();
                if (codes.Length > 0)
                {                     
                    culture = codes[0].ToUpper(); // Use the first preferred language
                }

                switch (culture)
                {
                    case Constants.LANGUAGE_GERMAN:
                        runtime.UIText.InitFromWxl(e.Session.ReadBinary("de_wxl"));
                        break;
                    case Constants.LANGUAGE_FRENCH:
                        runtime.UIText.InitFromWxl(e.Session.ReadBinary("fr_wxl"));
                        break;
                    case Constants.LANGUAGE_SPANISH:
                        runtime.UIText.InitFromWxl(e.Session.ReadBinary("es_wxl"));
                        break;
                    case Constants.LANGUAGE_JAPANESE:
                        runtime.UIText.InitFromWxl(e.Session.ReadBinary("jp_wxl"));
                        break;
                    default:
                        runtime.UIText.InitFromWxl(e.Session.ReadBinary("en_wxl"));
                        culture = Constants.LANGUAGE_ENGLISH; // Default to English if no match
                        break;
                }
                MessageBox.Show($"Using UI language: {culture}", "Language Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Constants.AddSecureProperty(project, Constants.SecureProperties.UI_LANGUAGE, culture);
            };
        }

        static void Project_Load(SetupEventArgs e)
        {
            if (e.IsInstalling)
            {
                // Check if this is an administrator installation started as a user and if we re-start as admin
                string isOpenAdmin = e.Session["OPEN_AS_ADMIN"];

                if (isOpenAdmin == "true" && !e.IsElevated)
                {
                    string msiFilePath = e.Session["OriginalDatabase"];
                    Constants.AddSecureProperty(e.Session, Constants.SecureProperties.RESUME_INSTALLATION, true);
                    Constants.AddSecureProperty(e.Session, Constants.SecureProperties.INSTALLATION_PATH, e.InstallDir);
                    string additionalParameters =  Constants.CollectSecureProperties(e.Session);

                    "msiexec".StartElevated($"/i \"{msiFilePath}\" {additionalParameters}");
                    DumpCommandLine(msiFilePath, additionalParameters);

                    e.Result = ActionResult.SkipRemainingActions;
                    if (Constants.GetSecureProperty(e.Session, Constants.SecureProperties.SETUP_PID, out string pidStr) &&
                         int.TryParse(pidStr, out int oldPid))
                    {
                        try
                        {
                            var proc = Process.GetProcessById(oldPid);
                            if (!proc.HasExited)
                            {
                                proc.Kill(); // or Send a close message if you want a more graceful attempt
                            }
                        }
                        catch
                        {
                            // Optional: log or ignore errors
                        }
                    }
                }

            }
        }

        // Writes the exact command you launch to %TEMP%\installer_debug.txt
        static void DumpCommandLine(string msiPath, string extraParams)
        {
            var temp = Path.Combine(Path.GetTempPath(), "D:\\Dev\\NET\\Media-Extractor\\SetupProject\\output\\installer_debug.txt");
            using (var tw = new StreamWriter(temp, append: true))
            {
                var cmd = $@"msiexec /i ""{msiPath}"" {extraParams}";
                tw.WriteLine($"=== Relaunch command at {DateTime.Now:O} ===");
                tw.WriteLine(cmd);
                tw.WriteLine();  // blank line
            }
        }

        static void Msi_Load(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "Load");
        }

        static void Msi_BeforeInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "BeforeInstall");
        }

        static void Msi_AfterInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "AfterExecute");
        }


    }
}