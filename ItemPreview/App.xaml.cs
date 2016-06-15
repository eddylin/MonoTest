using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ItemPreview
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        List<Exception> errors = new List<Exception>();

        protected override void OnStartup(StartupEventArgs e)
        {
            Guid guid = new Guid("{5686F85D-83A3-4F0A-8157-F21ED486B754}");
            using (SingleInstance singleInstance = new SingleInstance(guid))
            {
                if (singleInstance.IsFirstInstance)
                {
                    singleInstance.ArgumentsReceived += (sender, args) =>
                    {
                        Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow mainWindow = (MainWindow)Current.MainWindow;
                            mainWindow.UpdateInfo(args.Args[1] == null ? "" : args.Args[1]);
                        });
                    };
                    singleInstance.ListenForArgumentsFromSuccessiveInstances();
                    base.OnStartup(e);
                }
                else
                {
                    singleInstance.PassArgumentsToFirstInstance(Environment.GetCommandLineArgs());
                    //Application.Current.Shutdown();
                    Current.MainWindow.Close();
                }
            }
        }
    }
}
