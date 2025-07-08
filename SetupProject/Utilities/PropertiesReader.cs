using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WixSharp.Utilities
{
    public class PropertiesReader
    {
        private string[] lines;

        public string Version
        {
            get
            {
                return GetProperty("AssemblyVersion");
            }
        }

        public string FileNameVersion
        {
            get
            {
                string version = GetProperty("AssemblyVersion");
                if (string.IsNullOrEmpty(version))
                    return string.Empty;

                // Parse the version string into segments
                string[] parts = version.Split('.');
                List<int> numbers = parts.Select(p =>
                {
                    int.TryParse(p, out int n);
                    return n;
                }).ToList();

                // Trim trailing zero segments (from the right)
                for (int i = numbers.Count - 1; i > 1; i--)
                {
                    if (numbers[i] == 0)
                        numbers.RemoveAt(i);
                    else
                        break;
                }

                // Convert to _-separated string
                return string.Join(".", numbers);
            }
        }


        public string ProductName
        {
            get
            {
                return GetProperty("AssemblyTitle");
            }
        }

        public string ProductDirectoryName
        {
            get
            {
                string name = GetProperty("AssemblyTitle");
                return SanitizeFilename(name);
            }
        }

        private string GetProperty(string propertyName)
        {
            string line = lines.FirstOrDefault(l => l.StartsWith(string.Format("[assembly: {0}(", propertyName)));
            return line?.Split('"')[1].Trim(new char[] { ' ', '\t', '\r', '\n' });
        }

        public PropertiesReader()
        {
            lines = System.IO.File.ReadAllLines(Constants.PROPERTIES_PATH);
        }

        private static string SanitizeFilename(string filename)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            return string.Join("_", filename.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }

    }
}
