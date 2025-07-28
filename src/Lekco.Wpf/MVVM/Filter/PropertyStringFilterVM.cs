using Lekco.Wpf.Utility.Filter;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Lekco.Wpf.MVVM.Filter
{
    public class PropertyStringFilterVM<T> : IPropertyFilterVM<T>, IFilterNode<string>
    {
        public string DisplayName { get; set; }

        public PropertyStringFilter<T> Filter { get; }

        public string? PropertyName
        {
            get => Filter.PropertyName;
            set
            {
                if (Filter.PropertyName != value)
                {
                    Filter.PropertyName = value;
                    OnPropertyChanged(nameof(PropertyName));
                }
            }
        }

        public StringFilterType FilterType
        {
            get => Filter.FilterType;
            set
            {
                if (Filter.FilterType != value)
                {
                    Filter.FilterType = value;
                    OnPropertyChanged(nameof(FilterType));
                }
            }
        }

        public string Value
        {
            get => Filter.Value;
            set
            {
                if (Filter.Value != value)
                {
                    Filter.Value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public IFilterGroupNode? Parent { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyStringFilterVM(PropertyStringFilter<T> filter, string displayName)
        {
            Filter = filter;
            DisplayName = displayName;
        }

        public PropertyStringFilterVM(PropertyStringFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyStringFilterVM()
            : this(new PropertyStringFilter<T>(), "")
        {
        }

        public PropertyFilter<T> GetFilter()
        {
            return Filter;
        }

        public Expression<Func<T, bool>> GetExpression()
        {
            return Filter.GetExpression();
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
