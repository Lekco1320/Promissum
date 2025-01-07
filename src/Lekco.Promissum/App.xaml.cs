// Lekco Promissum
// Lukaß Zhang, 2023/12/25

using Lekco.Promissum.Control;
using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.View;
using Lekco.Promissum.ViewModel;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Lekco.Promissum.App
{
    /// <summary>
    /// The class of application Lekco Promissum.
    /// </summary>
    public sealed partial class Promissum : Application
    {
        /// <summary>
        /// The name of application.
        /// </summary>
        public static string Name => "Lekco Promissum";

        /// <summary>
        /// The list of command arguments of Lekco Promissum.
        /// </summary>
        public static string[] Args => Environment.GetCommandLineArgs();

        /// <summary>
        /// The directory of Lekco Promissum in AppData folder.
        /// </summary>
        public static string AppDir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Lekco\Promissum";

        /// <summary>
        /// The temporary work directory.
        /// </summary>
        public static string TempDir => AppDir + @"\.temp";

        /// <summary>
        /// The version of current application.
        /// </summary>
        public static Version Version => ResourceAssembly.GetName().Version!;

        /// <summary>
        /// The path of Lekco Promissum Agens.
        /// </summary>
        public static string AgensPath => Args[0][..Args[0].LastIndexOf('\\')] + @"\Lekco.Promissum.Agens.exe";

        public static MainWindowVM MainWindowVM { get; } = new MainWindowVM();

        public static ICommand NewProjectCommand => MainWindowVM.NewProjectCommand;

        public static ICommand OpenProjectCommand => MainWindowVM.OpenProjectCommand;

        public static ICommand OpenSpecifiedProjectCommand => new RelayCommand<string>(MainWindowVM.OpenProject);

        public static ICommand ShowSyncEngineWindowCommand => new RelayCommand(SyncEngineWindow.ShowSyncEngineWindow);

        public static ICommand SetAppConfigCommand => new RelayCommand(() => new AppConfigDialog().ShowDialog());

        public static ICommand ShowAboutWindowCommand => new RelayCommand(AboutWindow.ShowAbout);

        public static ICommand OpenReposCommand => new RelayCommand(() => Process.Start("explorer.exe", "https://github.com/Lekco1320/Promissum"));

        public static ICommand QuitCommand => new RelayCommand(Quit);

        /// <summary>
        /// The notify icon in task bar.
        /// </summary>
        private static NotifyIcon? NotifyIcon;

        private readonly NamedPipeServer _pipeServer;

        private readonly Mutex _mutex;

        /// <summary>
        /// Create an instance.
        /// </summary>
        public Promissum()
        {
            _mutex = new Mutex(true, "Lekco.Promissum.Mutex", out bool createdNew);
            if (!createdNew)
            {
                var client = new NamedPipeClient(Name);
                foreach (string arg in Args)
                {
                    client.Send(arg);
                }
                Environment.Exit(0);
            }

            _pipeServer = new NamedPipeServer(Name);
            _ = _pipeServer.StartUpAsync();
            _pipeServer.OnConnected += ConnectedServer;
            _pipeServer.OnReceivedArg += ReceivedArg;

            DispatcherUnhandledException += CopeWithUnhandledException;
            CheckAppTempDir();
            SetAppStyle();
            InitializeComponent();

            var vm = new StartPageVM();
            var page = new StartPage(vm);
            MainWindowVM.NavigationService.DefaultView = page;

            NotifyIcon = new NotifyIcon();
        }

        /// <summary>
        /// Occurs when a new instance of Lekco.Promissum launched.
        /// </summary>
        private void ConnectedServer(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(ShowMainWindow);
        }

        /// <summary>
        /// Receive arguments from new Promissum application.
        /// </summary>
        /// <param name="e">Argument from new Promissum application.</param>
        private void ReceivedArg(object? sender, string e)
        {
            var info = new FileInfo(e);
            if (info.Exists && info.Extension == ".prms")
            {
                MainWindowVM.OpenProject(e);
            }
        }

        /// <summary>
        /// Invokes when Lekco Promissum starts up.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            Task.Run(SyncEngine.AutoLoadProjects);
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
                    file.IsReadOnly = false;
                    file.Delete();
                }
                catch { }
            }
            var subDirectories = dirInfo.GetDirectories();
            foreach (var directory in subDirectories)
            {
                try
                {
                    directory.Delete(true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Cope with unhandled exception.
        /// </summary>
        private void CopeWithUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (!DialogHelper.ShowError(
                exception: e.Exception,
                buttonStyle: MessageDialogButtonStyle.OKCancel
            ))
            {
                Current.Shutdown();
            }
        }

        /// <summary>
        /// Show main window.
        /// </summary>
        public void ShowMainWindow()
        {
            if (MainWindow is not View.MainWindow)
            {
                MainWindow = new MainWindow(MainWindowVM);
            }
            MainWindow.Show();
            MainWindow.Activate();
        }

        /// <summary>
        /// A helper method to make MenuItem show left.
        /// </summary>
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
                    MainWindowVM.OpenProject(arg);
                }
            }

            if (!runBackground)
            {
                MainWindow = new MainWindow(MainWindowVM);
                MainWindow.Show();
            }
        }

        /// <summary>
        /// Quit the application.
        /// </summary>
        public static void Quit()
        {
            if (SyncEngine.LoadedProjectsCount == 0 || DialogHelper.ShowWarning(
                message: "是否要退出Lekco Promissum？这将或许影响计划任务的执行。关闭窗体可以将程序最小化至托盘图标以保证计划任务的正常执行。",
                buttonStyle: MessageDialogButtonStyle.OKCancel
            ))
            {
                Current.Shutdown();
            }
        }

        /// <summary>
        /// Occurs when quit the application.
        /// </summary>
        private void Exiting(object sender, ExitEventArgs e)
        {
            _pipeServer.Stop();
            SyncEngine.Dispose();
            _mutex.ReleaseMutex();
        }
    }
}