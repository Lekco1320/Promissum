using Lekco.Wpf.Utility.Filter;
using System;

namespace Lekco.Wpf.MVVM.Filter
{
    public class PropertyFilterVMFactory<T> : IPropertyFilterVMFactory<T>
    {
        public IPropertyFilterVM<T> Create(PropertyFilter<T> filter)
        {
            return filter switch
            {
                PropertyFilterGroup<T> groupFilter => new PropertyFilterGroupVM<T>(groupFilter, this),
                PropertyStringFilter<T> stringFilter => new PropertyStringFilterVM<T>(stringFilter),
                PropertyDateTimeFilter<T> dateTimeFilter => new PropertyDateTimeFilterVM<T>(dateTimeFilter),
                PropertyIntFilter<T> numberFilter => new PropertyIntFilterVM<T>(numberFilter),
                PropertyLongFilter<T> longFilter => new PropertyLongFilterVM<T>(longFilter),
                PropertyDecimalFilter<T> decimalFilter => new PropertyDecimalFilterVM<T>(decimalFilter),
                PropertyFloatFilter<T> floatFilter => new PropertyFloatFilterVM<T>(floatFilter),
                PropertyDoubleFilter<T> doubleFilter => new PropertyDoubleFilterVM<T>(doubleFilter),
                PropertyFileSizeFilter<T> fileSizeFilter => new PropertyFileSizeFilterVM<T>(fileSizeFilter),
                _ => throw new NotSupportedException($"Unsupported filter type: {filter.GetType().Name}")
            };
        }
    }
}
