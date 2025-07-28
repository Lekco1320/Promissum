namespace Lekco.Wpf.MVVM.Filter
{
    public interface IFilterNode
    {
        public IFilterGroupNode? Parent { get; set; }

        public void Remove();
    }

    public interface IFilterNode<T> : IFilterNode
    {
    }
}
