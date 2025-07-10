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
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
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

        /// <summary>
        /// Viewmodel of the main window.
        /// </summary>
        public static MainWindowVM MainWindowVM { get; } = new MainWindowVM();

        /// <summary>
        /// Static command for creating a new project.
        /// </summary>
        public static ICommand NewProjectCommand => MainWindowVM.NewProjectCommand;

        /// <summary>
        /// Static command for opening a project.
        /// </summary>
        public static ICommand OpenProjectCommand => MainWindowVM.OpenProjectCommand;

        /// <summary>
        /// Static command for opening a specified project.
        /// </summary>
        public static ICommand OpenSpecifiedProjectCommand => new RelayCommand<string>(MainWindowVM.OpenProject);

        /// <summary>
        /// Static command for showing <see cref="SyncEngineWindow" />.
        /// </summary>
        public static ICommand ShowSyncEngineWindowCommand => new RelayCommand(SyncEngineWindow.Show);

        /// <summary>
        /// Static command for showing <see cref="AppConfigDialog" />.
        /// </summary>
        public static ICommand ShowAppConfigDialogCommand => new RelayCommand(() => new AppConfigDialog().ShowDialog());

        /// <summary>
        /// Static command for showing <see cref="AboutWindow" />.
        /// </summary>
        public static ICommand ShowAboutWindowCommand => new RelayCommand(AboutWindow.ShowAbout);

        /// <summary>
        /// Static command for checking update.
        /// </summary>
        public static ICommand CheckUpdateCommand => new RelayCommand(static async () => await CheckForUpdatesAsync());

        /// <summary>
        /// Static command for opening the repository in browser.
        /// </summary>
        public static ICommand OpenReposCommand => new RelayCommand(() => Process.Start("explorer.exe", "https://github.com/Lekco1320/Promissum"));

        /// <summary>
        /// Static command for quitting the app.
        /// </summary>
        public static ICommand QuitCommand => new RelayCommand(Quit);

        /// <summary>
        /// The notify icon in task bar.
        /// </summary>
        private static NotifyIcon? NotifyIcon;

        /// <summary>
        /// Pipe server for corresponding with other instances of Promissum.
        /// </summary>
        private static readonly NamedPipeServer _pipeServer;

        /// <summary>
        /// Mutex for ensuring singleton.
        /// </summary>
        private static readonly Mutex _mutex;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Promissum()
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
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public Promissum()
        {
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
                Dispatcher.Invoke(() => MainWindowVM.OpenProject(e));
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

        /// <summary>
        /// Checks whether a new version of the application is available by comparing the current version with the latest release on GitHub.
        /// If a new version is found, prompts the user to download it; otherwise, notifies that the application is up to date.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task CheckForUpdatesAsync()
        {
            var currentVersion = App.Promissum.Version;

            var latestTag = await FetchLatestReleaseTagAsync();
            if (string.IsNullOrEmpty(latestTag) ||
                !Version.TryParse(latestTag.TrimStart('v', 'V'), out var latestVersion))
            {
                DialogHelper.ShowError("版本更新检查失败。");
                return;
            }

            if (latestVersion > currentVersion)
            {
                if (DialogHelper.ShowInformation($"检测到新版本：{latestVersion}{Environment.NewLine}当前版本：{currentVersion}{Environment.NewLine}是否前往Github下载？"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/Lekco1320/Promissum/releases/latest",
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                DialogHelper.ShowInformation(
                    message: $"当前已是最新版本：{latestVersion}。",
                    buttonStyle: MessageDialogButtonStyle.OK
                );
            }
        }

        /// <summary>
        /// Retrieves the latest release tag of the Promissum project from GitHub using the GitHub API.
        /// </summary>
        /// <returns>The tag name of the latest release if successful; otherwise, <see langword="null"/>.</returns>
        public static async Task<string?> FetchLatestReleaseTagAsync()
        {
            using var client = new HttpClient
            {
                DefaultRequestHeaders = { { "User-Agent", "Promissum-Updater" } }
            };

            var url = $"https://api.github.com/repos/Lekco1320/Promissum/releases/latest";
            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc    = await JsonDocument.ParseAsync(stream);
            if (doc.RootElement.TryGetProperty("tag_name", out var tagElem))
            {
                return tagElem.GetString();
            }

            return null;
        }
    }
}