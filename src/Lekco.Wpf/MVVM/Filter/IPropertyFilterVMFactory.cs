using Lekco.Wpf.Utility.Filter;

namespace Lekco.Wpf.MVVM.Filter
{
    public interface IPropertyFilterVMFactory<T>
    {
        public IPropertyFilterVM<T> Create(PropertyFilter<T> filter);
    }
}
