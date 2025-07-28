using Lekco.Wpf.Utility.Filter;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Lekco.Wpf.MVVM.Filter
{
    public class PropertyFilterGroupVM<T> : IPropertyFilterVM<T>, IFilterGroupNode
    {
        public string DisplayName { get; set; }

        public PropertyFilterGroup<T> Filter { get; }

        public LogicalOperator Mode
        {
            get => Filter.Mode;
            set
            {
                if (Filter.Mode != value)
                {
                    Filter.Mode = value;
                    OnPropertyChanged(nameof(Mode));
                }
            }
        }

        public ObservableCollection<IFilterNode> Children { get; }

        public IFilterGroupNode? Parent { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyFilterGroupVM()
        {
            DisplayName = "";
            Filter = new PropertyFilterGroup<T>();
            Children = new ObservableCollection<IFilterNode>();
            Children.CollectionChanged += (s, e) => OnChildrenChanged(e);
        }

        public PropertyFilterGroupVM(PropertyFilterGroup<T> filter, IPropertyFilterVMFactory<T> factory)
        {
            DisplayName = "";
            Filter = filter;
            Children = new ObservableCollection<IFilterNode>();

            foreach (var child in Filter.Children)
            {
                var vm = factory.Create(child);
                var node = (IFilterNode)vm;
                node.Parent = this;
                Children.Add(node);
            }

            Children.CollectionChanged += (s, e) => OnChildrenChanged(e);
        }

        public PropertyFilterGroupVM(PropertyFilterGroup<T> filter)
            : this(filter, new PropertyFilterVMFactory<T>())
        {
        }

        protected void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IPropertyFilterVM<T> vm in e.NewItems)
                {
                    var node = (IFilterNode)vm;
                    node.Parent = this;
                    Filter.Children.Add(vm.GetFilter());
                }
            }
            if (e.OldItems != null)
            {
                foreach (IPropertyFilterVM<T> vm in e.OldItems)
                {
                    var node = (IFilterNode)vm;
                    node.Parent = null;
                    Filter.Children.Remove(vm.GetFilter());
                }
            }
        }

        public Expression<Func<T, bool>> GetExpression()
        {
            return Filter.GetExpression();
        }

        public PropertyFilter<T> GetFilter()
        {
            return Filter;
        }

        public void AddChild(IFilterNode node)
        {
            Children.Add(node);
        }

        public void AddChildGroup()
        {
            Children.Add(new PropertyFilterGroupVM<T>());
        }

        public void RemoveChild(IFilterNode node)
        {
            Children.Remove(node);
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
