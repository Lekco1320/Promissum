using Lekco.Wpf.Utility.Sorter;
using System.ComponentModel;

namespace Lekco.Wpf.MVVM.Sorter
{
    public interface IPropertySorterVM : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }

        public PropertySorter GetSorter();
    }

    public interface IPropertySorterVM<T> : IPropertySorterVM
    {
        public new PropertySorter<T> GetSorter();

        PropertySorter IPropertySorterVM.GetSorter()
            => GetSorter();
    }
}
