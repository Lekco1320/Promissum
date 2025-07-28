using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Filter;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Lekco.Wpf.MVVM.Filter
{
    class PropertyFileSizeFilterVM<T> : IPropertyFilterVM<T>, IFilterNode<FileSize>
    {
        public string DisplayName { get; set; }

        public FileSize Value
        {
            get => Filter.Value;
            set => Filter.Value = value;
        }

        public double Fitted
        {
            get => _fitted;
            set
            {
                _fitted = value;
                Value = new FileSize(value, Unit);
                OnPropertyChanged(nameof(Fitted));
            }
        }
        private double _fitted;

        public FileSizeUnit Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                Value = new FileSize(Fitted, value);
                OnPropertyChanged(nameof(Unit));
            }
        }
        private FileSizeUnit _unit;

        public FileSizeFilterType FilterType
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

        public PropertyFileSizeFilter<T> Filter { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyFileSizeFilterVM(PropertyFileSizeFilter<T> filter, string displayName)
        {
            Filter = filter;
            DisplayName = displayName;
            _fitted = Filter.Value.Fit(out _unit);
        }

        public PropertyFileSizeFilterVM(PropertyFileSizeFilter<T> filter)
            : this(filter, "")
        {
        }

        public PropertyFileSizeFilterVM()
            : this(new PropertyFileSizeFilter<T>(), "")
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
