using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WixToolset.Dtf.WindowsInstaller;

namespace WixSharp.dialogs
{
    public class LanguageSelectionDialog : AbstractCustomMainDialog
    {

        // Language options
        private RadioButton rbnEnglish;
        private RadioButton rbnSpanish;
        private RadioButton rbnFrench;
        private RadioButton rbnGerman;
        private RadioButton rbnJapanese;

        public override string GetLocalizedTitle()
        {
            return "[LanguageSelection_Title]";
        }

        public override string GetDialogName()
        {
            return "LanguageSelectionDialog";
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            string language;
            bool unattendedInstallation = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.RESUME_INSTALLATION, out _);
            bool languageDefined = Constants.GetSecureProperty(this.Session(), Constants.SecureProperties.UI_LANGUAGE, out language);
            if (unattendedInstallation && languageDefined)
            {
                SelectLanguage(language);
                base.Shell.GoNext();
            }
            else if (!unattendedInstallation)
            {
                int pid = Process.GetCurrentProcess().Id;
                Constants.AddSecureProperty(this.Session(), Constants.SecureProperties.SETUP_PID, pid.ToString());
            }
        }

        public override void NextClick(object sender, EventArgs e)
        {
            string selected = Constants.LANGUAGE_ENGLISH;  // default
            if (rbnGerman.Checked)
                selected = Constants.LANGUAGE_GERMAN;
            else if (rbnFrench.Checked)
                selected = Constants.LANGUAGE_FRENCH;
            else if (rbnSpanish.Checked)
                selected = Constants.LANGUAGE_SPANISH;
            else if (rbnJapanese.Checked)
                selected = Constants.LANGUAGE_JAPANESE;

            // Call your language‐setting logic
            SelectLanguage(selected);
            base.Shell.GoNext();
        }

        public override void BackClick(object sender, EventArgs e)
        {
            base.Shell.GoPrev();
        }



        public override void CreateContent()
        {
            bool enChecked = false;
            bool deChecked = false;
            bool frCheked = false;
            bool esChecked = false;
            bool jaChecked = false;
            var codes = Native.GetPreferredIsoTwoLetterUILanguages();
            string culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (codes.Length > 0)
            {
                culture = codes[0].ToLower(); // Use the first preferred language
            }

            switch (culture)
            {
                case "de":
                    deChecked = true;
                    break;
                case "fr":
                    frCheked = true;
                    break;
                case "es":
                    esChecked = true;
                    break;
                case "ja":
                    jaChecked = true;
                    break;
                default:
                   enChecked = true;
                    break;
            }


        AddLabelHeader("[LanguageSelection_Title]:");

            rbnEnglish = new RadioButton { Name = "rbnEnglish" };
            rbnSpanish = new RadioButton { Name = "rbnSpanish" };
            rbnFrench = new RadioButton { Name = "rbnFrench" };
            rbnGerman = new RadioButton { Name = "rbnGerman" };
            rbnJapanese = new RadioButton { Name = "rbnJapanese" };

            // radioButtons
            CreateRadioButton(rbnEnglish, "English", 2, new System.Drawing.Point(20, 60), new System.Drawing.Size(150, 17), enChecked);
            CreateRadioButton(rbnSpanish, "Español (Spanish)", 3, new System.Drawing.Point(20, 90), new System.Drawing.Size(150, 17), esChecked);
            CreateRadioButton(rbnFrench, "Français (French)", 4, new System.Drawing.Point(20, 120), new System.Drawing.Size(150, 17), frCheked);
            CreateRadioButton(rbnGerman, "Deutsch (German)", 5, new System.Drawing.Point(20, 150), new System.Drawing.Size(150, 17), deChecked);
            CreateRadioButton(rbnJapanese, "日本語 (Japanese)", 6, new System.Drawing.Point(20, 180), new System.Drawing.Size(150, 17), jaChecked);

            AddControlToTextPanel(rbnEnglish);
            AddControlToTextPanel(rbnSpanish);
            AddControlToTextPanel(rbnFrench);
            AddControlToTextPanel(rbnGerman);
            AddControlToTextPanel(rbnJapanese);
        }

        private void CreateRadioButton(RadioButton rbn, string displayText, int tabIndex,
                               System.Drawing.Point location, System.Drawing.Size size, bool isChecked = false)
        {
            rbn = rbn ?? new RadioButton();   // (optional) create if null, but we’ll instantiate before calling
            rbn.AutoSize = true;
            rbn.Location = location;
            rbn.Name = rbn.Name;   // the field name is already set below
            rbn.Size = size;
            rbn.TabIndex = tabIndex;
            rbn.Text = displayText;
            rbn.UseVisualStyleBackColor = true;
            rbn.Checked = isChecked;
            rbn.TabStop = isChecked;
        }

        public void SelectLanguage(string selectedLanguage)
        {
            // 1) Set thread culture // TODO Auto-detect based on system settings or user selection
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);

            // 2) Reinitialize MSI UI text from correct .wxl
            MsiRuntime runtime = Shell.MsiRuntime();
            Session wixSession = this.Session();

            Constants.AddSecureProperty(wixSession, Constants.SecureProperties.UI_LANGUAGE, selectedLanguage);

            switch (selectedLanguage)
            {
                case Constants.LANGUAGE_GERMAN:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("de_wxl"));
                    break;

                case Constants.LANGUAGE_FRENCH:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("fr_wxl"));
                    break;

                case Constants.LANGUAGE_SPANISH:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("es_wxl"));
                    break;

                case Constants.LANGUAGE_JAPANESE:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("jp_wxl"));
                    break;

                default:
                    runtime.UIText.InitFromWxl(wixSession.ReadBinary("en_wxl"));
                    break;
            }
        }

    }
    
}
