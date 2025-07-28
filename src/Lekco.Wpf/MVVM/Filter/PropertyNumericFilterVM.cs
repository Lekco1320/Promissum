using Lekco.Wpf.Utility.Filter;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;

namespace Lekco.Wpf.MVVM.Filter
{
    public class PropertyNumericFilterVM<T, TNumber> : IPropertyFilterVM<T>, IFilterNode<TNumber>
        where TNumber : struct, INumber<TNumber>
    {
        public string DisplayName { get; set; }

        public PropertyNumericFilter<T, TNumber> Filter { get; }

        public Type NumberType { get; }

        public NumericFilterType FilterType
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

        public TNumber Value
        {
            get => Filter.Value;
            set
            {
                if (!Filter.Value.Equals(value))
                {
                    Filter.Value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public IFilterGroupNode? Parent { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyNumericFilterVM()
            : this(new PropertyNumericFilter<T, TNumber>(), "")
        {
        }

        public PropertyNumericFilterVM(PropertyNumericFilter<T, TNumber> filter)
            : this(filter, "")
        {
        }

        public PropertyNumericFilterVM(PropertyNumericFilter<T, TNumber> filter, string displayName)
        {
            Filter = filter;
            DisplayName = displayName;
            NumberType = typeof(TNumber);
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

    public class PropertyIntFilterVM<T> : PropertyNumericFilterVM<T, int>
    {
        public PropertyIntFilterVM()
            : this(new PropertyIntFilter<T>(), "")
        {
        }

        public PropertyIntFilterVM(PropertyIntFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyIntFilterVM(PropertyIntFilter<T> filter, string displayName)
            : base(filter, displayName)
        {
        }
    }

    public class PropertyLongFilterVM<T> : PropertyNumericFilterVM<T, long>
    {
        public PropertyLongFilterVM()
            : this(new PropertyLongFilter<T>(), "")
        {
        }

        public PropertyLongFilterVM(PropertyLongFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyLongFilterVM(PropertyLongFilter<T> filter, string displayName)
            : base(filter, displayName)
        {
        }
    }

    public class PropertyDecimalFilterVM<T> : PropertyNumericFilterVM<T, decimal>
    {
        public PropertyDecimalFilterVM()
            : this(new PropertyDecimalFilter<T>(), "")
        {
        }

        public PropertyDecimalFilterVM(PropertyDecimalFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyDecimalFilterVM(PropertyDecimalFilter<T> filter, string displayName)
            : base(filter, displayName)
        {
        }
    }

    public class PropertyFloatFilterVM<T> : PropertyNumericFilterVM<T, float>
    {
        public PropertyFloatFilterVM()
            : this(new PropertyFloatFilter<T>(), "")
        {
        }

        public PropertyFloatFilterVM(PropertyFloatFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyFloatFilterVM(PropertyFloatFilter<T> filter, string displayName)
            : base(filter, displayName)
        {
        }
    }

    public class PropertyDoubleFilterVM<T> : PropertyNumericFilterVM<T, double>
    {
        public PropertyDoubleFilterVM()
            : this(new PropertyDoubleFilter<T>(), "")
        {
        }

        public PropertyDoubleFilterVM(PropertyDoubleFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyDoubleFilterVM(PropertyDoubleFilter<T> filter, string displayName)
            : base(filter, displayName)
        {
        }
    }
}
