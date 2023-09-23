using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused
{
    public class ActivitySummary
    {
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        
        public TimeSpan TotalDuration { get; set; }
    }
}
