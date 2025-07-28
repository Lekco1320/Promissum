using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Lekco.Promissum.View.Sync
{
    public partial class SelectPropertyDialog : CustomWindow, INotifyPropertyChanged, IInteractive
    {
        public IEnumerable<string> Items { get; }

        public int SelectedIndex { get; set; }
        
        public bool IsOK { get; set; }

        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }

        public SelectPropertyDialog(IEnumerable<string> items)
        {
            Items = items;
            OKCommand = new RelayCommand(OK);
            CancelCommand = new RelayCommand(Cancel);

            InitializeComponent();

            DataContext = this;
        }

        private void OK()
        {
            IsOK = true;
            Close();
        }

        private void Cancel()
        {
            Close();
        }
    }
}
