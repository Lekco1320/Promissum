using Lekco.Promissum.Apps;
using Lekco.Promissum.Control;
using Lekco.Promissum.Sync;
using Lekco.Promissum.View;
using Lekco.Promissum.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lekco.Promissum
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string AppDir;
        public static readonly string TempDir;
        public static readonly string AgensPath;
        public static readonly string Name;
        public static readonly string[] Args;
        public static readonly Version Version;

        private static readonly MainWindowVM _mainWindowVM;
        private static readonly CancellationTokenSource _taskCancellation;

        public readonly NotifyIcon NotifyIcon;

        public App()
        {
            InitializeComponent();
            SetupServer();
            CheckAppTempDir();
            SyncEngine.LoadAutoRunProjects();
            NotifyIcon = new NotifyIcon(this);
        }

        static App()
        {
            Name = "Lekco Promissum";
            Args = Environment.GetCommandLineArgs();
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                SendArgs();
                Environment.Exit(0);
            }

            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
            _taskCancellation = new CancellationTokenSource();
            AppDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Lekco\Promissum";
            TempDir = AppDir + @"\.temp";
            AgensPath = Args[0][..Args[0].LastIndexOf('\\')] + @"\Lekco.Promissum.Agens.exe";
            Version = ResourceAssembly.GetName().Version ?? new Version(1, 0, 0, 0);

            _mainWindowVM = new MainWindowVM();
            MenuLeftDisplay();
        }

        private static void SendArgs()
        {
            var client = new PipeClient(".", "Lekco Promissum");
            foreach (string s in Args)
            {
                if (File.Exists(s))
                {
                    client.DoRequest(s);
                }
            }
            client.CloseServer();
        }

        private void SetupServer()
        {
            var serverTask = new Task(() =>
            {
                var server = new PipeServer("Lekco Promissum");
                server.Received += Receiving;
                server.BeginReceive();
                server.Close();
                SetupServer();
            }, _taskCancellation.Token, TaskCreationOptions.LongRunning);
            serverTask.Start();
        }

        private void Receiving(string message)
        {
            _mainWindowVM.OpenProject(message);
            Current.Dispatcher.BeginInvoke(() =>
            {
                MainWindow ??= new MainWindow(_mainWindowVM);
                MainWindow.Show();
            });
        }

        private void StartUp(object sender, StartupEventArgs e)
        {
            bool runBackground = false;
            foreach (string s in Args)
            {
                if (s == "-background")
                {
                    runBackground = true;
                }
            }

            if (!runBackground)
            {
                MainWindow = new MainWindow(_mainWindowVM);
                MainWindow.Show();
            }
        }

        public static void Quit()
        {
            SyncEngine.Dispose();
            Config.SaveAsFile();
            _taskCancellation.Cancel();
            Current.Shutdown();
        }

        public void ShowMainWindow()
        {
            if (MainWindow is not View.MainWindow)
            {
                MainWindow = new MainWindow(_mainWindowVM);
            }
            MainWindow.Show();
        }

        private static void CheckAppTempDir()
        {
            var dirInfo = new DirectoryInfo(TempDir);
            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(TempDir);
                return;
            }
            var subfiles = dirInfo.GetFiles();
            foreach (var file in subfiles)
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                }
            }
            var subDirectories = dirInfo.GetDirectories();
            foreach (var directory in subDirectories)
            {
                try
                {
                    directory.Delete(true);
                }
                catch
                {
                }
            }
        }

        private void Exiting(object sender, ExitEventArgs e)
        {
            SyncEngine.Dispose();
            Config.SaveAsFile();
            _taskCancellation.Cancel();
        }

        private static void MenuLeftDisplay()
        {
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