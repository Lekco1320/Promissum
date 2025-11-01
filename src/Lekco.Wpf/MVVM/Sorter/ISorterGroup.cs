using System.Collections.ObjectModel;

namespace Lekco.Wpf.MVVM.Sorter
{
    public interface ISorterGroup
    {
        public ObservableCollection<ISorterItem> Children { get; }

        public void AddChild(ISorterItem sorter);

        public void RemoveChild(ISorterItem sorter);

        public void InsertChild(int index, ISorterItem sorter);
    }
}
