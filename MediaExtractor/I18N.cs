using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace MediaExtractor
{
    public class I18N
    {
        private static I18N instance;

        public static I18N Current
        {
            get
            {
                if (instance == null) { instance = new I18N();}

                return instance;
            }
            set { instance = value; }
        }

        public enum AvailableCultures
        {
            system,
            en_US,
            de_DE
        }

        public static AvailableCultures GetCultureByString(string culture)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return AvailableCultures.system;
            }
            string equalized = culture.ToLower().Replace("-","_");
            if (equalized == AvailableCultures.de_DE.ToString().ToLower())
            {
                return AvailableCultures.de_DE;
            }
            else if (equalized == AvailableCultures.en_US.ToString().ToLower())
            {
                return AvailableCultures.en_US;
            }
            else
            {
                return AvailableCultures.system;
            }
        }
        

        public I18N()
        {

        }

        public void SetLanguage(string code)
        {
            code = code.Replace("_", "-");
            CultureInfo ci = new CultureInfo(code);
            Thread.CurrentThread.CurrentUICulture = ci;
            if (ci.IsNeutralCulture)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            }
        }

        public void SetLanguage(AvailableCultures cultre, Window windowInstance)
        {
            string code = cultre.ToString();
            code = code.Replace("_", "-");
            CultureInfo ci = new CultureInfo(code);
            Thread.CurrentThread.CurrentUICulture = ci;
            if (ci.IsNeutralCulture)
            {
                 Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
             }


            //  Init.Restart(ci);

            //https://github.com/SeriousM/WPFLocalizationExtension/
           // WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
           // WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = ci;

            /*
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(code);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(code);
            List<Uri> i18nList = new List<Uri>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                i18nList.Add(dictionary.Source);
            }
            Application.Current.Resources.MergedDictionaries.Clear();
            foreach (Uri uri in i18nList)
            {
                ResourceDictionary i18nDictionary = new ResourceDictionary();
                i18nDictionary.Source = uri;
                Application.Current.Resources.MergedDictionaries.Add(i18nDictionary);
            }
            */
        }

    }
}
