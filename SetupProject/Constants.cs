using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WixSharp.CommonTasks;
using WixToolset.Dtf.WindowsInstaller;

namespace WixSharp
{
    public static class Constants
    {
        public enum SecureProperties
        {
            RESUME_INSTALLATION,
            UI_LANGUAGE,
            INSTALLATION_TYPE,
            ADD_STARTMENU_ICON,
            ADD_DESKTOP_ICON,
            REGISTER_EXPLORER,
            TARGET_DIR,
            SETUP_PID
        }

        public enum InstallationFeatures
        {
            desktop,
            start,
            explorer,
        }

        #region external
        public const string PROPERTIES_PATH = @"..\MediaExtractor\Properties\AssemblyInfo.cs";
#if Debug
        public const string PROGRAM_FILES_PATH = @"..\MediaExtractor\bin\release\";
#else
        public const string PROGRAM_FILES_PATH = @"..\MediaExtractor\bin\debug\";
#endif
        public const string MAIN_APP_NAME = "MediaExtractor.exe";
        public const string ICON_NAME = "MediaExtractor.ico";
        public const string INSTALLER_OUTPUT_PATH = @"..\Install\";
        #endregion

        #region internal
        private const string PROPERTY_UNDEFINED_VALUE = "PROPERTY_UNDEFINED";
        public const string PRODUCT_NAME_KEY = "ProductName";
        public const string PRODUCT_VERSION_KEY = "PRODUCT_VERSION";
        public const string OPEN_AS_ADMIN_KEY = "OPEN_AS_ADMIN";
        public const string INSTALL_SCOPE_KEY = "INSTALLATION_TYPE";
        #endregion

        public const string LANGUAGE_ENGLISH = "EN";
        public const string LANGUAGE_FRENCH = "FR";
        public const string LANGUAGE_GERMAN = "DE";
        public const string LANGUAGE_SPANISH = "ES";
        public const string LANGUAGE_JAPANESE = "JP";

        public const string INSTALLATION_TYPE_USER = "user";
        public const string INSTALLATION_TYPE_SYSTEM = "system";


        public const string FEATURE_ROOT_NAME = "[InstallationFeatureProgramFiles]";
        public const string FEATURE_DESKTOP_NAME = "[InstallationFeatureDesktopIcon]";
        public const string FEATURE_STARTMENU_NAME = "[InstallationFeatureStartIcon]";
        public const string FEATURE_EXPLORER_NAME = "[InstallationFeatureRegisterExplorer]";

        public const string INSTALLATION_FEATURE_DESKTOP = "desktopIcon";
        public const string INSTALLATION_FEATURE_STARTMENU = "startMenuIcon";
        public const string INSTALLATION_FEATURE_EXPLORER = "explorerRegistration";



        public static void AddSecureProperty(Session session, SecureProperties property, bool value)
        {
            session[property.ToString()] = value ? "true" : "false";
        }

        public static void AddSecureProperty(Session session, SecureProperties property, string value)
        {
            session[property.ToString()] = value;
        }

        public static void AddSecureProperty(ManagedProject project, SecureProperties property)
        {
            AddSecureProperty(project, property, "", true);
        }

        public static void AddSecureProperty(ManagedProject project, SecureProperties property, string value, bool undefined = false)
        {
            if (undefined && string.IsNullOrEmpty(value))
            {
                value = PROPERTY_UNDEFINED_VALUE; // Use a placeholder for undefined values
            }
            Property secureProperty = new Property(property.ToString(), value) { Secure = true };
            int index = project.Properties.FindIndex(p => p.Name == property.ToString() && p.Secure == true);
            if (index == -1)
            {
                project.AddProperty(secureProperty);
            }
            else
            {
                project.Properties[index] = secureProperty; // Update existing property
            }
        }

        public static bool GetSecureProperty(Session session, SecureProperties property, out string value)
        {
            value = session.Property(property.ToString());
            if (value == PROPERTY_UNDEFINED_VALUE)
            {
                value = string.Empty; // Convert placeholder back to empty string
            }
            return !string.IsNullOrEmpty(value);
        }


        public static string CollectSecureProperties(Session session)
        {
            StringBuilder sb = new StringBuilder();
            var properties = Enum.GetValues(typeof(SecureProperties)).Cast<SecureProperties>().ToList();
            foreach (var property in properties)
            {
                if (GetSecureProperty(session, property, out string value))
                {
                    sb.Append($"{property}=\"{value}\" ");
                }
            }
            return sb.ToString().TrimEnd(); // Remove trailing newline
        }

    }
}
