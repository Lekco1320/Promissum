using Lekco.Wpf.MVVM.Filter;
using Lekco.Wpf.Utility;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lekco.Wpf.Control.Filter
{
    public class PropertyFilterNodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? GroupTemplate { get; set; }

        public DataTemplate? StringTemplate { get; set; }

        public DataTemplate? NumericTemplate { get; set; }

        public DataTemplate? DateTimeTemplate { get; set; }

        public DataTemplate? FileSizeTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                IFilterGroupNode => GroupTemplate,
                IFilterNode<string> => StringTemplate,
                IFilterNode<DateTime> => DateTimeTemplate,
                IFilterNode<int> => NumericTemplate,
                IFilterNode<long> => NumericTemplate,
                IFilterNode<float> => NumericTemplate,
                IFilterNode<double> => NumericTemplate,
                IFilterNode<decimal> => NumericTemplate,
                IFilterNode<FileSize> => FileSizeTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
