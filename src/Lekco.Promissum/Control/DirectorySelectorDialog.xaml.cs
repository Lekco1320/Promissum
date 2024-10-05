using Lekco.Promissum.Model.Sync.Base;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// DirectorySelector.xaml 的交互逻辑
    /// </summary>
    public partial class DirectorySelectorDialog : CustomWindow, IInteractive, INotifyPropertyChanged
    {
        public ObservableCollection<DirectoryTreeItemViewModel> RootItemViewModel { get; set; }

        public DirectoryBase RootDirectory { get; set; }

        public ICommand OKCommand => new RelayCommand(OK);

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public bool CanOK { get; protected set; }

        public bool IsOK { get; protected set; }

        public DirectoryBase SelectedDirectory
        {
            get => (DirectoryBase)GetValue(SelectedDirectoryProperty);
            set
            {
                SetValue(SelectedDirectoryProperty, value);
                CanOK = true;
            }
        }

        public static readonly DependencyProperty SelectedDirectoryProperty = DependencyProperty.Register(nameof(SelectedDirectory), typeof(DirectoryBase), typeof(DirectorySelectorDialog));

        protected DirectorySelectorDialog(DirectoryBase rootDirectory)
        {
            RootDirectory = rootDirectory;
            RootItemViewModel = new ObservableCollection<DirectoryTreeItemViewModel>
            {
                new DirectoryTreeItemViewModel(rootDirectory)
            };
            DataContext = this;
            InitializeComponent();
        }

        private void SelectedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedDirectory = ((DirectoryTreeItemViewModel)TreeView.SelectedItem).Directory;
        }

        private void OK()
        {
            IsOK = true;
            Close();
        }

        private void Cancel()
            => Close();

        public static bool ShowDialog(DirectoryBase rootDirectory, [MaybeNullWhen(false)] out DirectoryBase targetDirectory)
        {
            targetDirectory = null;
            var dialog = new DirectorySelectorDialog(rootDirectory);
            dialog.ShowDialog();
            if (dialog.IsOK)
            {
                targetDirectory = dialog.SelectedDirectory;
                return true;
            }
            return false;
        }

        public class DirectoryTreeItemViewModel : INotifyPropertyChanged
        {
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    LoadChildren();
                    IsExpanded = true;
                }
            }
            private bool _isSelected;

            public bool IsExpanded { get; set; }

            private bool _loaded;

            public string Name => Directory.Name != "" ? Directory.Name : "{...}";

            public ObservableCollection<DirectoryTreeItemViewModel> Children { get; }

            public ICommand ClickCommand { get; }

            public DirectoryBase Directory { get; }

            public event PropertyChangedEventHandler? PropertyChanged;

            public DirectoryTreeItemViewModel(DirectoryBase directory)
            {
                Directory = directory;
                Children = new ObservableCollection<DirectoryTreeItemViewModel>();
                ClickCommand = new RelayCommand(LoadChildren);
            }

            public void LoadChildren()
            {
                if (_loaded)
                {
                    return;
                }

                _loaded = true;
                Children.Clear();
                foreach (var subDir in Directory.EnumerateDirectories())
                {
                    Children.Add(new DirectoryTreeItemViewModel(subDir));
                }
            }
        }
    }
}
