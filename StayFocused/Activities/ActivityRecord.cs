using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused
{
    public class ActivityRecord
    {
        public Guid ActivityRecordId { get; set; }
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        public DateTime TimeStamp { get; set; }

    }
}
