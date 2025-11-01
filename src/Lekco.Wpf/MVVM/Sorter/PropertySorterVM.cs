using Lekco.Wpf.Utility.Sorter;
using System.ComponentModel;

namespace Lekco.Wpf.MVVM.Sorter
{
    public class PropertySorterVM<T> : ISorterItem<T>, IPropertySorterVM<T>
    {
        public string DisplayName { get; set; }

        public PropertySorter<T> PropertySorter { get; }

        public string? PropertyName
        {
            get => PropertySorter.PropertyName;
            set => PropertySorter.PropertyName = value;
        }

        public bool Descending
        {
            get => PropertySorter.Descending;
            set
            {
                if (PropertySorter.Descending != value)
                {
                    PropertySorter.Descending = value;
                    OnPropertyChanged(nameof(Descending));
                }
            }
        }

        public ISorterGroup? Parent { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertySorterVM(PropertySorter<T> sorter, string displayName)
        {
            PropertySorter = sorter;
            DisplayName = displayName;
        }

        public PropertySorterVM(PropertySorter<T> sorter)
            : this(sorter, "")
        {
        }

        public PropertySorterVM()
            : this(new PropertySorter<T>(), "")
        {
        }

        public PropertySorter<T> GetSorter()
        {
            return PropertySorter;
        }

        public void Remove()
        {
            Parent?.RemoveChild(this);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
