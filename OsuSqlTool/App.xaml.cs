using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OsuSqlTool
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var errorLog = Path.Combine(Path.GetDirectoryName(Settings.Instance.SettingsFile), "error.log");
                var writer = new StreamWriter(errorLog, true);
                writer.WriteLine("{0:u} :: {1}", DateTime.Now, e.ExceptionObject);
                writer.Close();
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("A critical error has occurred somewhere in this software. "
                        + "Please provide me with the error.log which is where the OsuSqlTool.exe lies.",
                        "osu!SQL Tool - Critical Error");
                });
            }
            catch { }
        }
    }
}
