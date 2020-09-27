using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaExtractor
{
    public class I18n
    {

        public const string ENGLISH = "en";
        public const string GERMAN = "de-DE";

        public static void MatchLocale(ViewModel viewModel, string currentLocale)
        {
            switch (currentLocale)
            {
                case ENGLISH:
                    viewModel.UseEnglishLocale = true;
                    viewModel.UseGermanLocale = false;
                    break;
                case GERMAN:
                    viewModel.UseEnglishLocale = false;
                    viewModel.UseGermanLocale = true;
                    break;
                default:
                    viewModel.UseEnglishLocale = true;
                    viewModel.UseGermanLocale = false;
                    break;
            }
        }

        public static string T(string key)
        {
            return Properties.Resources.ResourceManager.GetString(key);
        }

        public static string R(string key, params string[] parameters)
        {
            string localized = T(key);
            return string.Format(localized, parameters);
        }

        public static string R(string key, params int[] parameters)
        {
            string localized = T(key);
            return string.Format(localized, parameters);
        }

    }
}
