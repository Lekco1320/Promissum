using System;
using System.Windows;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public static string Version { get => App.Promissum.Version.ToString(); }
        public AboutWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        public static void ShowAbout()
        {
            new AboutWindow().Show();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch
            {
            }
        }
    }
}
