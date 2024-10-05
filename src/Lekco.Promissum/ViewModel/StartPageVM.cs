using Lekco.Promissum.App;
using Lekco.Wpf.MVVM.Command;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lekco.Promissum.ViewModel
{
    public class StartPageVM
    {
        public ObservableCollection<AccessedFile> AccessedFiles { get; }

        public ICommand DoubleClickCommand => new RelayCommand<object>(DoubleClick);

        public ICommand OpenCommand => new RelayCommand<AccessedFile>(OpenFile);

        public StartPageVM()
        {
            AccessedFiles = AccessedFileManager.AccessedFiles;
        }

        private void DoubleClick(object obj)
        {
            if (obj is DataGrid datagrid)
            {
                if (datagrid.SelectedItem is AccessedFile file)
                {
                    App.Promissum.MainWindowVM.OpenProject(file.FullName);
                }
                datagrid.SelectedItem = null;
            }
        }

        private void OpenFile(AccessedFile file)
        {
            App.Promissum.MainWindowVM.OpenProject(file.FullName);
        }
    }
}
