using Lekco.Wpf.Utility.Sorter;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Lekco.Wpf.MVVM.Sorter
{
    public class PropertySorterGroupVM<T> : IPropertySorterGroupVM<T>, ISorterGroup
    {
        public PropertySorterGroup<T> SorterGroup { get; }

        public ObservableCollection<ISorterItem> Children { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertySorterGroupVM()
        {
            SorterGroup = new PropertySorterGroup<T>();
            Children = new ObservableCollection<ISorterItem>();
            Children.CollectionChanged += (s, e) => OnChildrenChanged(e);
        }

        public PropertySorterGroupVM(PropertySorterGroup<T> sorters)
        {
            SorterGroup = sorters;
            Children = new ObservableCollection<ISorterItem>();

            foreach (var child in SorterGroup.Sorters)
            {
                Children.Add(new PropertySorterVM<T>(child));
            }
            Children.CollectionChanged += (s, e) => OnChildrenChanged(e);
        }

        protected void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IPropertySorterVM<T> vm in e.NewItems)
                {
                    SorterGroup.Sorters.Add(vm.GetSorter());
                }
            }
            if (e.OldItems != null)
            {
                foreach (IPropertySorterVM<T> vm in e.OldItems)
                {
                    SorterGroup.Sorters.Remove(vm.GetSorter());
                }
            }
        }

        public void AddChild(ISorterItem sorter)
        {
            Children.Add(sorter);
        }

        public void RemoveChild(ISorterItem sorter)
        {
            Children.Remove(sorter);
        }

        public void InsertChild(int index, ISorterItem sorter)
        {
            Children.Insert(index, sorter);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PropertySorterGroup<T> GetSorters()
        {
            return SorterGroup;
        }
    }
}
