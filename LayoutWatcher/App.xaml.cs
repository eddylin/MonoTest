using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;

namespace LayoutWatcher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        Mutex myMutex;

        private static NotifyIcon icon = new NotifyIcon();

        private static List<string> fail_list = new List<string>();

        private static List<string> logs = new List<string>();

        private static List<FileSystemWatcher> file_watchers = new List<FileSystemWatcher>();


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool isNewInstance = false;
            myMutex = new Mutex(true, "LayoutWatcher", out isNewInstance);
            if (!isNewInstance)
            {
                App.Current.Shutdown();
                return;
            }

            string[] paths = ConfigurationManager.AppSettings["watchPaths"].Split(';');

            foreach (var path in paths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    AddWatcher(path);
                }
            }

            //InitIcon();

            //timer
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler((sender, e2) =>
            {
                if (fail_list.Count > 0)
                {
                    GzipFile(fail_list[0]);
                }
            });
            timer.Interval = 1000;
            timer.Enabled = true;


            System.Timers.Timer info_timer = new System.Timers.Timer();
            info_timer.Elapsed += new ElapsedEventHandler((sender, e2) =>
            {
                if (logs.Count > 0)
                {
                    App.Current.Dispatcher.Invoke(new MethodInvoker(() =>
                    {
                        MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
                        mainWindow.LogInfo(logs[0]);
                    }));
                    logs.Remove(logs[0]);
                }
            });
            info_timer.Interval = 800;
            info_timer.Enabled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            icon.Dispose();
            foreach (var watcher in file_watchers)
            {
                watcher.Dispose();
            }
        }

        private static void InitIcon()
        {
            icon.Icon = new Icon("icon.ico");
            icon.Visible = true;
            //icon.BalloonTipTitle = "Info";
            //icon.BalloonTipText = "Watch started";
            //icon.ShowBalloonTip(1);


            ContextMenu menu = new ContextMenu();
            MenuItem menuItem = new MenuItem("退出", (s, e) =>
            {
                App.Current.Shutdown();
            });
            menu.MenuItems.Add(menuItem);

            icon.ContextMenu = menu;

            icon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
                {
                    App.Current.Dispatcher.Invoke(new MethodInvoker(() =>
                   {
                       if (Current.MainWindow.WindowState != WindowState.Minimized)
                       {
                           App.Current.MainWindow.WindowState = WindowState.Minimized;
                       }
                       else
                       {
                           App.Current.MainWindow.WindowState = WindowState.Normal;
                           App.Current.MainWindow.Activate();
                       }
                   }));
                }
                else if (e.Button == MouseButtons.Right)
                {

                }
            };
        }

        private static void GzipFile(string fullPath)
        {
            if (fail_list.Contains(fullPath))
            {
                fail_list.Remove(fullPath);
            }

            try
            {
                FileInfo file = new FileInfo(fullPath);

                string newPath = new Regex(@"editor\\mobile_uieditor").Replace(fullPath, @"trunk\config\mobileclient");

                FileInfo new_file = new FileInfo(newPath);
                if (new_file.Exists) new_file.Delete();

                Process proc = new Process();
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                string cmd = string.Format("gzip.exe {0} {1}", fullPath, newPath);
                proc.StandardInput.WriteLine(cmd);
                cmd = string.Format("move {0} {1}", newPath + ".gz", newPath);
                proc.StandardInput.WriteLine(cmd);
                proc.Close();

                LogInfo(file.Name + " is zipped");
            }
            catch (Exception)
            {
                if (!fail_list.Contains(fullPath))
                {
                    fail_list.Add(fullPath);
                }
            }
        }

        static void AddWatcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;

            file_watchers.Add(watcher);
        }

        static void OnChanged(object sender, FileSystemEventArgs e)
        {
            GzipFile(e.FullPath);
        }

        static void OnCreated(object sender, FileSystemEventArgs e)
        {
            GzipFile(e.FullPath);
        }

        static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            // GzipFile(e.FullPath);
        }

        static void OnRenamed(object sender, FileSystemEventArgs e)
        {
            GzipFile(e.FullPath);
        }

        static void LogInfo(string info)
        {
            logs.Add(info);
        }

        static bool IsShowBalloo()
        {
            return false;
        }
    }
}
