using Lekco.Promissum.Sync;
using Lekco.Promissum.ViewModel;
using System;
using System.Windows.Controls;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// ProjectContentPage.xaml 的交互逻辑
    /// </summary>
    public partial class SyncProjectContentPage : Page
    {
        private SyncProjectContentPageVM _vm;
        public SyncProjectContentPage(SyncProject project)
        {
            InitializeComponent();

            _vm = new SyncProjectContentPageVM(project);
            DataContext = _vm;
        }

        private void ContentRendering(object sender, EventArgs e)
        {
            Scrollviewer.ScrollToTop();
        }
    }
}
