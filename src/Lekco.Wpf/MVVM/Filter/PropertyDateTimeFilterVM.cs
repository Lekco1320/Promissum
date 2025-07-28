using Lekco.Wpf.Utility.Filter;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Lekco.Wpf.MVVM.Filter
{
    public class PropertyDateTimeFilterVM<T> : IPropertyFilterVM<T>, IFilterNode<DateTime>
    {
        public string DisplayName { get; set; }

        public DateTime Value
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

        public DateTimeFilterType FilterType
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

        public IFilterGroupNode? Parent { get; set; }

        public PropertyDateTimeFilter<T> Filter { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyDateTimeFilterVM(PropertyDateTimeFilter<T> filter, string displayName)
        {
            Filter = filter;
            DisplayName = displayName;
        }

        public PropertyDateTimeFilterVM(PropertyDateTimeFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyDateTimeFilterVM()
            : this(new PropertyDateTimeFilter<T>(), "")
        {
        }

        public Expression<Func<T, bool>> GetExpression()
        {
            return Filter.GetExpression();
        }

        public PropertyFilter<T> GetFilter()
        {
            return Filter;
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
