using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Lekco.Promissum.DemoHarness
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            SetAppStyle();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // PrintResourceDictionary(Application.Current.Resources.MergedDictionaries);
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private static void SetAppStyle()
        {
            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);

            // Make MenuItems show left.
            var ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                // change to false
                var t = typeof(SystemParameters);
                var field = t.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                field?.SetValue(null, false);
            }
        }
    }
}
