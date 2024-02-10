using Lekco.Promissum.Control;
using Lekco.Promissum.Sync;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// FailedSyncRecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FailedSyncRecordWindow : AnimatedWindow
    {
        public IEnumerable<FailedSyncRecord> Records { get; set; }

        public FailedSyncRecordWindow(IEnumerable<FailedSyncRecord> records)
        {
            InitializeComponent();

            Records = records;
            DataContext = this;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowFailedRecords(IEnumerable<FailedSyncRecord> records)
        {
            new FailedSyncRecordWindow(records).ShowDialog();
        }
    }
}
