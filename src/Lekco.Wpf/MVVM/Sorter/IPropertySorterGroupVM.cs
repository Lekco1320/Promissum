using Lekco.Wpf.Utility.Sorter;
using System.ComponentModel;

namespace Lekco.Wpf.MVVM.Sorter
{
    public interface IPropertySorterGroupVM : INotifyPropertyChanged
    {
    }

    public interface IPropertySorterGroupVM<T> : IPropertySorterGroupVM
    {
        public PropertySorterGroup<T> GetSorters();
    }
}
