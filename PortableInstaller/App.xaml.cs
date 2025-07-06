using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortableInstaller
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            var options = new InstallerOptions();

            var argMap = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "--product", val => options.Product = val.Trim('"') },
                { "-p",      val => options.Product = val.Trim('"') },
                { "--target", val => options.TargetPath = val.Trim('"') },
                { "-t",      val => options.TargetPath = val.Trim('"') },
                { "--desktop-icon", val => options.WriteDesktopIcon = bool.TryParse(val, out var v) && v },
                { "-d",          val => options.WriteDesktopIcon = bool.TryParse(val, out var v) && v },
                { "--start-icon", val => options.WriteStartIcon = bool.TryParse(val, out var v) && v },
                { "-s",          val => options.WriteStartIcon = bool.TryParse(val, out var v) && v },
                { "--icon-file",  val => options.IconFile = val.Trim('"') },
                { "-i",           val => options.IconFile = val.Trim('"') }
            };

            foreach (var arg in args)
            {
                int eqIdx = arg.IndexOf('=');
                if (eqIdx > 0)
                {
                    var key = arg.Substring(0, eqIdx);
                    var value = arg.Substring(eqIdx + 1);
                    if (argMap.TryGetValue(key, out var setter))
                    {
                        setter(value);
                    }
                }
            }

            var main = new MainWindow(options);
            main.Show();
            if (!string.IsNullOrEmpty(options.TargetPath))
                main.BeginInstall();
        }
    }

    public class InstallerOptions
    {
        public string Product { get; set; }
        public string TargetPath { get; set; }
        public bool WriteDesktopIcon { get; set; } = false;
        public bool WriteStartIcon { get; set; }
        public string IconFile { get; set; }
    }
}
