// Lekco Promissum. 2023/12/25.
using Lekco.Promissum.Apps;
using Lekco.Promissum.Control;
using Lekco.Promissum.View;
using Lekco.Promissum.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lekco.Promissum
{
    /// <summary>
    /// The class of application Lekco Promissum.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The directory of Lekco Promissum in AppData folder.
        /// </summary>
        public static readonly string AppDir;

        /// <summary>
        /// The temporary work directory.
        /// </summary>
        public static readonly string TempDir;

        /// <summary>
        /// The path of Lekco Promissum Agens.
        /// </summary>
        public static readonly string AgensPath;

        /// <summary>
        /// The name of application.
        /// </summary>
        public static readonly string Name;

        /// <summary>
        /// The list of command arguments of Lekco Promissum.
        /// </summary>
        public static readonly string[] Args;

        /// <summary>
        /// The version of current application.
        /// </summary>
        public static readonly Version Version;

        private static readonly MainWindowVM _mainWindowVM;
        private static readonly CancellationTokenSource _taskCancellation;

        /// <summary>
        /// The notify icon in task bar.
        /// </summary>
        public readonly NotifyIcon NotifyIcon;

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        public App()
        {
            InitializeComponent();
            SetupServer();
            CheckAppTempDir();
            SyncEngine.LoadAutoRunProjects();
            NotifyIcon = new NotifyIcon(this);
        }

        /// <summary>
        /// The static constructor of this type.
        /// </summary>
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

        /// <summary>
        /// Send arguments to open processes.
        /// </summary>
        private static void SendArgs()
        {
            var client = new PipeClient(".", Name);
            for (int i = 1; i < Args.Length; i++)
            {
                client.DoRequest(Args[i]);
            }
            client.CloseServer();
        }

        /// <summary>
        /// Set up a new server to receive messages sent from other processes.
        /// </summary>
        private void SetupServer()
        {
            var serverTask = new Task(() =>
            {
                var server = new PipeServer(Name);
                server.Received += Receiving;
                server.BeginReceive();
                server.Close();
                SetupServer();
            }, _taskCancellation.Token, TaskCreationOptions.LongRunning);
            serverTask.Start();
        }

        /// <summary>
        /// Receive message from other processes.
        /// </summary>
        /// <param name="message">Message sent from other processes.</param>
        private void Receiving(string message)
        {
            var files = new List<string>();
            if (File.Exists(message))
            {
                files.Add(message);
            }
            Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var file in files)
                {
                    _mainWindowVM.OpenProject(file);
                }
                MainWindow ??= new MainWindow(_mainWindowVM);
                MainWindow.Show();
            });
        }

        /// <summary>
        /// The method to do after application started up completely.
        /// </summary>
        private void StartUp(object sender, StartupEventArgs e)
        {
            bool runBackground = false;
            for (int i = 1; i < Args.Length; i++)
            {
                var arg = Args[i];
                if (arg == "-background")
                {
                    runBackground = true;
                }
                if (File.Exists(arg))
                {
                    _mainWindowVM.OpenProject(arg);
                }
            }

            if (!runBackground)
            {
                MainWindow = new MainWindow(_mainWindowVM);
                MainWindow.Show();
            }
        }

        /// <summary>
        /// Quit Lekco Promissum.
        /// </summary>
        public static void Quit()
        {
            SyncEngine.Dispose();
            Config.SaveAsFile();
            _taskCancellation.Cancel();
            Current.Shutdown();
        }

        /// <summary>
        /// Show main window.
        /// </summary>
        public void ShowMainWindow()
        {
            if (MainWindow is not View.MainWindow)
            {
                MainWindow = new MainWindow(_mainWindowVM);
            }
            MainWindow.Show();
        }

        /// <summary>
        /// Checks the temporary work directory's existence and delete all remain files.
        /// </summary>
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

        /// <summary>
        /// Occurs when quit Lekco Promissum.
        /// </summary>
        private void Exiting(object sender, ExitEventArgs e)
        {
            SyncEngine.Dispose();
            Config.SaveAsFile();
            _taskCancellation.Cancel();
        }

        /// <summary>
        /// A helper method to make MenuItem show left.
        /// </summary>
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