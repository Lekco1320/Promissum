using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.ViewModel.Sync;
using System;
using System.Windows.Controls;

namespace Lekco.Promissum.View.Sync
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
