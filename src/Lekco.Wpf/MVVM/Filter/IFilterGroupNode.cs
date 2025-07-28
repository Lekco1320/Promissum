using Lekco.Wpf.Utility.Filter;
using System.Collections.ObjectModel;

namespace Lekco.Wpf.MVVM.Filter
{
    public interface IFilterGroupNode : IFilterNode
    {
        public ObservableCollection<IFilterNode> Children { get; }

        public LogicalOperator Mode { get; set; }

        public void AddChild(IFilterNode node);

        public void AddChildGroup();

        public void RemoveChild(IFilterNode node);
    }
}
