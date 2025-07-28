using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.MVVM.Filter;
using Lekco.Wpf.Utility.Filter;
using System.ComponentModel;
using System.Windows.Controls;

namespace Lekco.Promissum.DemoHarness
{
    public partial class PropertyFilterViewPanel : StackPanel, INotifyPropertyChanged
    {
        public PropertyFilterGroupVM<Foo> RootFilterVM { get; }

        public RelayCommand RefreshExpressionCommand { get; }

        public string Expression { get; set; } = "";

        public RelayCommand<IFilterGroupNode> AddFilterCommand => new RelayCommand<IFilterGroupNode>(
            n =>
            {
                var filter = new PropertyStringFilter<Foo>
                {
                    PropertyName = nameof(Foo.Name),
                    FilterType = StringFilterType.Contains,
                    Value = "新添加的过滤器"
                };
                var vm = new PropertyStringFilterVM<Foo>(filter);
                n.AddChild(vm);
            });

        public PropertyFilterViewPanel()
        {
            var filter = new PropertyFilterGroup<Foo>
            {
                Mode = LogicalOperator.And,
                Children =
                [
                    new PropertyStringFilter<Foo>
                    {
                        PropertyName = nameof(Foo.Name),
                        FilterType = StringFilterType.Contains,
                        Value = "张三"
                    },
                    new PropertyIntFilter<Foo>
                    {
                        PropertyName = nameof(Foo.Age),
                        FilterType = NumericFilterType.GreaterThan,
                        Value = 18
                    },
                    new PropertyDateTimeFilter<Foo>
                    {
                        PropertyName = nameof(Foo.Birthday),
                        FilterType = DateTimeFilterType.Before,
                        Value = new DateTime(2000, 1, 1)
                    },
                    new PropertyFilterGroup<Foo>
                    {
                        Mode = LogicalOperator.Or,
                        Children =
                        [
                            new PropertyStringFilter<Foo>
                            {
                                PropertyName = nameof(Foo.ID),
                                FilterType = StringFilterType.StartsWith,
                                Value = "330"
                            },
                            new PropertyStringFilter<Foo>
                            {
                                PropertyName = nameof(Foo.ID),
                                FilterType = StringFilterType.EndsWith,
                                Value = "0035"
                            },
                        ]
                    },
                    new PropertyFileSizeFilter<Foo>
                    {
                        PropertyName = nameof(Foo.FileSize),
                        FilterType = FileSizeFilterType.LessThan,
                        Value = new Wpf.Utility.FileSize(5000)
                    }
                ]
            };
            RootFilterVM = new PropertyFilterGroupVM<Foo>(filter);
            ((IPropertyFilterVM)RootFilterVM.Children[0]).DisplayName = "姓名";
            ((IPropertyFilterVM)RootFilterVM.Children[1]).DisplayName = "年龄";
            ((IPropertyFilterVM)RootFilterVM.Children[2]).DisplayName = "出生时间";
            var group = (PropertyFilterGroupVM<Foo>)RootFilterVM.Children[3];
            ((IPropertyFilterVM)group.Children[0]).DisplayName = "身份证";
            ((IPropertyFilterVM)group.Children[1]).DisplayName = "身份证";
            ((IPropertyFilterVM)RootFilterVM.Children[4]).DisplayName = "文件大小";

            InitializeComponent();
            DataContext = this;

            RefreshExpressionCommand = new RelayCommand(
                () =>
                {
                    Expression = RootFilterVM.GetExpression().ToString();
                    OnPropertyChanged(nameof(Expression));
                });
        }
    }
}
