using Lekco.Promissum.Control;
using Lekco.Promissum.Sync;
using System.Collections.Generic;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// FailedSyncRecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FailedSyncRecordWindow : CustomWindow
    {
        public IEnumerable<FailedSyncRecord> Records { get; set; }

        public FailedSyncRecordWindow(IEnumerable<FailedSyncRecord> records)
        {
            InitializeComponent();

            Records = records;
            DataContext = this;
        }

        public static void ShowFailedRecords(IEnumerable<FailedSyncRecord> records)
        {
            new FailedSyncRecordWindow(records).ShowDialog();
        }
    }
}
