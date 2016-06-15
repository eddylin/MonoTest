using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ItemPreview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void UpdateInfo(string item_id)
        {
            string pattern = @"id *= *(" + item_id + @"), *name *= *""(\w*)""";
            Match match = Regex.Match(File.ReadAllText(@"F:\x1\trunk\src\mobile\scripts\config\configlogic\config_items.lua"), pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                this.textBlock.Text = match.Groups[2].ToString();
            }
            else
            {
                this.textBlock.Text = "";
            }
        }
        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            this.Topmost = true;
        }
    }
}
