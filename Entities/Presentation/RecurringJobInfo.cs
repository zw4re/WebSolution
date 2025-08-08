using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Presentation
{
    public class RecurringJobInfo
    {
        public string Id { get; set; }              // Job ID
        public string Cron { get; set; }            // Cron Schedule
        public string TimeZone { get; set; }        // Zaman dilimi
        public string Job { get; set; }             // Job method ismi
        public DateTime? LastExecution { get; set; }
        public DateTime? NextExecution { get; set; }
        public string Status { get; set; }          // Success, Failed vb.
    }

}
