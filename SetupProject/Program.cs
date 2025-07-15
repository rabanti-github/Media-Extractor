using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using System.Windows.Forms;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Utilities;
using WixToolset.Dtf.WindowsInstaller;
using static WixSharp.CommonTasks.AppSearch;

namespace SetupProject
{
    public static class Program
    {
        static void Main()
        {
            PropertiesReader propertiesReader = new PropertiesReader();

            #region featureDefinitions
            Feature root = new Feature(Constants.FEATURE_ROOT_NAME, "[InstallationFeatureProgramFilesDescription]", true, false) { Display = FeatureDisplay.expand };
            Feature desktopIcon = new Feature(Constants.FEATURE_DESKTOP_NAME, "[InstallationFeatureDesktopIconDescription]", false, true);
            Feature startMenuIcon = new Feature(Constants.FEATURE_STARTMENU_NAME, "[InstallationFeatureStartIconDescription]", false, true);
            Feature registration = new Feature(Constants.FEATURE_EXPLORER_NAME, "[InstallationFeatureRegisterExplorerDescription]", false, true);
            #endregion

            #region projectInitialzation
            var project = new ManagedProject(propertiesReader.ProductName,
                new Dir(@"%ProgramFiles%\" + propertiesReader.ProductDirectoryName,
                    new Files(Constants.PROGRAM_FILES_PATH + "*.*")//,
                ),
                new Dir(@"%ProgramMenu%",
                    new ExeFileShortcut(
                        propertiesReader.ProductName,
                        $"[INSTALLDIR]{Constants.MAIN_APP_NAME}",
                        "")
                    {
                        IconFile = $"{Constants.PROGRAM_FILES_PATH}\\{Constants.ICON_NAME}",
                        Feature = startMenuIcon
                    }
                ),
                new Dir(@"%Desktop%",
                    new ExeFileShortcut(
                        propertiesReader.ProductName,
                        $"[INSTALLDIR]{Constants.MAIN_APP_NAME}",
                        "")
                    {
                        IconFile = $"{Constants.PROGRAM_FILES_PATH}\\{Constants.ICON_NAME}",
                        Feature = desktopIcon
                    }
                )
            );

            project.AddProperty(new Property(Constants.PRODUCT_VERSION_KEY, propertiesReader.Version));

            project.GUID = new Guid("6446F2F5-6F37-4DD0-B0DA-6248A5F01EDA");
            project.Version = new Version(propertiesReader.Version);
            project.ControlPanelInfo.ProductIcon = "resources\\media_extractor.ico"; // path to the icon file
            project.LicenceFile = "resources\\MIT-License.rtf"; // path to the license file
            

            project.Scope = InstallScope.perUserOrMachine;
            project.EnableUninstallFullUI();
            project.EnableResilientPackage();
            project.InitLocalization();
            #endregion

            #region securePropertiesInitialization
            Constants.AddSecureProperty(project, Constants.SecureProperties.RESUME_INSTALLATION);
            Constants.AddSecureProperty(project, Constants.SecureProperties.UI_LANGUAGE);
            Constants.AddSecureProperty(project, Constants.SecureProperties.INSTALLATION_TYPE);
            Constants.AddSecureProperty(project, Constants.SecureProperties.ADD_DESKTOP_ICON);
            Constants.AddSecureProperty(project, Constants.SecureProperties.ADD_STARTMENU_ICON);
            Constants.AddSecureProperty(project, Constants.SecureProperties.REGISTER_EXPLORER);
            Constants.AddSecureProperty(project, Constants.SecureProperties.TARGET_DIR);
            Constants.AddSecureProperty(project, Constants.SecureProperties.SETUP_PID);
            #endregion

            #region featureAssinment
            root.Add(desktopIcon);
            root.Add(startMenuIcon);
            root.Add(registration);


            project.DefaultFeature = root;
            #endregion

            #region dialogs
            //custom set of UI WPF dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add<LanguageSelectionDialog>()
                                            .Add<WelcomeDialog>()
                                            .Add<LicenceDialog>()
                                            .Add<LegacyCheckDialog>()
                                            .Add<SetupTypeDialog>()
                                            .Add<FeaturesDialog>()
                                            .Add<InstallDirDialog>()
                                            .Add<ProgressDialog>()
                                            .Add<ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<LanguageSelectionDialog>()
                                           .Add<MaintenanceTypeDialog>()
                                           .Add<FeaturesDialog>()
                                           .Add<ProgressDialog>()
                                           .Add<ExitDialog>();
            #endregion

            #region hooks
            project.Load += Msi_Load;
            project.Load += Project_Load;
            project.BeforeInstall += Msi_BeforeInstall;
            project.AfterInstall += Msi_AfterInstall;
            #endregion

            #region regValues
            project.RegValues = new[]
           {
                // Context‑menu command
                new RegValue(RegistryHive.ClassesRoot,
                             @"*\shell\Open with " + propertiesReader.ProductName + @"\command",
                             "",
                             "\"[INSTALLDIR]" + Constants.MAIN_APP_NAME + "\" \"%1\"")
                {
                    Id        = "Reg_OpenWith_Command",
                    Feature   = registration,
                },
                // Context‑menu icon
                new RegValue(RegistryHive.ClassesRoot,
                             @"*\shell\Open with " + propertiesReader.ProductName,
                             "Icon",
                             "[INSTALLDIR]" + Constants.MAIN_APP_NAME + ",0")
                {
                    Id        = "Reg_OpenWith_Icon",
                    Feature   = registration,
                },
            };
            #endregion

            project.BuildMsi();

            #region postBuild
            var sourceMsi = Path.Combine(Directory.GetCurrentDirectory(), project.OutFileName + ".msi");
            var outputDir = Constants.INSTALLER_OUTPUT_PATH;

            Directory.CreateDirectory(outputDir);
            string outputMsi = Path.Combine(propertiesReader.ProductName + "-" + propertiesReader.FileNameVersion + ".msi");
            if (System.IO.File.Exists(outputMsi))
            {
                System.IO.File.Delete(outputMsi);
            }
            System.IO.File.Move(sourceMsi, Path.Combine(outputDir, outputMsi));
            #endregion
        }

        #region methods
        private static void InitLocalization(this ManagedProject project)
        {
            project.AddBinary(new Binary(new Id("de_wxl"), @"Resources\Localization\WixUI_de-de.wxl"))
                   .AddBinary(new Binary(new Id("en_wxl"), @"Resources\Localization\WixUI_en-us.wxl"))
                   .AddBinary(new Binary(new Id("fr_wxl"), @"Resources\Localization\WixUI_fr-fr.wxl"))
                   .AddBinary(new Binary(new Id("es_wxl"), @"Resources\Localization\WixUI_es-es.wxl"))
                   .AddBinary(new Binary(new Id("it_wxl"), @"Resources\Localization\WixUI_it-it.wxl"))
                   .AddBinary(new Binary(new Id("jp_wxl"), @"Resources\Localization\WixUI_ja-jp.wxl"));

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
                    case Constants.LANGUAGE_ITALIAN:
                        runtime.UIText.InitFromWxl(e.Session.ReadBinary("it_wxl"));
                        break;
                    default:
                        runtime.UIText.InitFromWxl(e.Session.ReadBinary("en_wxl"));
                        culture = Constants.LANGUAGE_ENGLISH; // Default to English if no match
                        break;
                }
                if (!Constants.GetSecureProperty(session, Constants.SecureProperties.RESUME_INSTALLATION, out _))
                {
                    Constants.AddSecureProperty(e.Session, Constants.SecureProperties.UI_LANGUAGE, culture);
                }
            };
        }

        static void Project_Load(SetupEventArgs e)
        {
            if (!e.IsInstalling && !e.IsRepairing && !e.IsUninstalling)
            {
                return;
            }

            string installForAll = e.Session["INSTALL_FOR_ALL_USERS"] ?? "";
            if (installForAll == "1")
            {
                // Per-machine install
                e.Session["ALLUSERS"] = "1";
            }
            else
            {
                // Per-user install
                e.Session["ALLUSERS"] = "";
            }

            // Check if this is an administrator installation started as a user and if we re-start as admin
            string isOpenAdmin = e.Session["OPEN_AS_ADMIN"];

                if (isOpenAdmin == "true" && !e.IsElevated)
                {
                    //MessageBox.Show(e.ToString(), "Restarting as Administrator");
                    string msiFilePath = e.Session["OriginalDatabase"];
                    Constants.AddSecureProperty(e.Session, Constants.SecureProperties.RESUME_INSTALLATION, true);
                    Constants.AddSecureProperty(e.Session, Constants.SecureProperties.TARGET_DIR, e.InstallDir);
                    string additionalParameters = Constants.CollectSecureProperties(e.Session);

                    "msiexec".StartElevated($"/i \"{msiFilePath}\" {additionalParameters}");

                    e.Result = ActionResult.SkipRemainingActions;
                    MessageBox.Show("The setup will now restart with elevated privileges. Please wait...", "Restarting Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        #endregion
    }
}