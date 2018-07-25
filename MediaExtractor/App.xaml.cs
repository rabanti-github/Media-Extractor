using System;
using System.Windows;

namespace MediaExtractor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Length > 2)
            {
                I18N.Current.SetLanguage(e.Args[2]);
            }
            base.OnStartup(e);
        }

        public static void Restart(string cultureInfoCode)
        {
            string[] args = Environment.GetCommandLineArgs();
            string argString = "";
            if (args.Length == 1)
            {
                argString = "null " + cultureInfoCode;
            }
            else if (args.Length > 1)
            {
                argString = Utils.PrepareArgument(args[1], "null") + " " + cultureInfoCode;
            }
     
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, argString);
            Application.Current.Shutdown();
        }
    }
}
