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

        private static List<FileSystemWatcher> file_watchers = new List<FileSystemWatcher>();


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool isNewInstance = false;
            myMutex = new Mutex(true, "LayoutWatcher", out isNewInstance);
            if (!isNewInstance)
            {
                System.Windows.MessageBox.Show("Already an instance is running...");
                App.Current.Shutdown();
            }

            InitIcon();

            AddWatcher(@"E:\x1\editor\mobile_uieditor\assets\ui");
            AddWatcher(@"E:\x1\editor\mobile_uieditor\assets\ui\layout");

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

            var windows = this.Windows;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            icon.Dispose();
        }

        private static void InitIcon()
        {
            icon.Icon = new Icon("icon.ico");
            icon.Visible = true;
            icon.BalloonTipTitle = "Info";
            icon.BalloonTipText = "Watch started";
            icon.ShowBalloonTip(1);

            icon.MouseClick += (s, e) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (App.Current.MainWindow.WindowState != WindowState.Minimized)
                    {
                        App.Current.MainWindow.WindowState = WindowState.Minimized;
                    }
                    else
                    {
                        App.Current.MainWindow.WindowState = WindowState.Normal;
                        App.Current.MainWindow.Activate();
                    }
                });
            };
        }

        private static void GzipFile(string fullPath)
        {
            FileInfo file = new FileInfo(fullPath);
            byte[] b;
            try
            {
                using (FileStream f = file.OpenRead())
                {
                    b = new byte[f.Length];
                    f.Read(b, 0, (int)f.Length);
                }
            }
            catch (Exception ex)
            {
                if (!fail_list.Contains(fullPath))
                {
                    fail_list.Add(fullPath);
                }
                return;
            }

            string newPath = new Regex(@"editor\\mobile_uieditor").Replace(fullPath, @"trunk\config\mobileclient");

            FileInfo new_file = new FileInfo(newPath);
            if (new_file.Exists) new_file.Delete();

            using (FileStream f2 = new FileStream(newPath, FileMode.Create))
            {
                using (GZipStream gzip_stream = new GZipStream(f2, CompressionMode.Compress, false))
                {
                    gzip_stream.Write(b, 0, b.Length);
                }
            }

            if (fail_list.Contains(fullPath))
            {
                fail_list.Remove(fullPath);
            }

            icon.BalloonTipTitle = "Layout已同步到mobile client.";
            icon.BalloonTipText = fullPath;
            icon.ShowBalloonTip(1);

            LogInfo(fullPath + " is zip to " + newPath);
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
            icon.BalloonTipTitle = "文件创建";
            icon.BalloonTipText = e.Name;
            icon.ShowBalloonTip(1);
        }

        static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            icon.BalloonTipTitle = "文件删除";
            icon.BalloonTipText = e.Name;
            icon.ShowBalloonTip(1);
        }

        static void OnRenamed(object sender, FileSystemEventArgs e)
        {
            icon.BalloonTipTitle = "文件重命名";
            icon.BalloonTipText = e.Name;
            icon.ShowBalloonTip(1);
        }

        static void LogInfo(string info)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
                mainWindow.LogInfo(DateTime.Now.ToString() + " " + info);
            });
        }
    }
}
