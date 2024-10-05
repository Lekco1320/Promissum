using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Record;
using System.Collections.Generic;
using System.Linq;

namespace Lekco.Promissum.ViewModel.Sync
{
    public class ExceptionRecordsWindowVM
    {
        public IEnumerable<ExceptionRecord> ExceptionRecords { get; }

        public int RecordsCount { get; }

        public int ExecutionID { get; }

        public ExecutionTrigger ExecutionTrigger { get; }

        public ExceptionRecordsWindowVM(ExecutionRecord executionRecord)
        {
            ExceptionRecords = executionRecord.ExceptionRecords;
            RecordsCount = ExceptionRecords.Count();
            ExecutionID = executionRecord.ID;
            ExecutionTrigger = executionRecord.ExecutionTrigger;
        }
    }
}
