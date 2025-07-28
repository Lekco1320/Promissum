using Lekco.Wpf.Utility.Filter;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Lekco.Wpf.MVVM.Filter
{
    public interface IPropertyFilterVM : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }

        public PropertyFilter GetFilter();

        public Expression GetExpression();
    }

    public interface IPropertyFilterVM<T> : IPropertyFilterVM
    {
        public new PropertyFilter<T> GetFilter();

        public new Expression<Func<T, bool>> GetExpression();

        PropertyFilter IPropertyFilterVM.GetFilter()
            => GetFilter();

        Expression IPropertyFilterVM.GetExpression()
            => GetExpression();
    }
}
