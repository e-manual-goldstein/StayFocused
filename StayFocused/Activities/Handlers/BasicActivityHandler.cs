using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused.Activities.Handlers
{
    public class BasicActivityHandler : IActivityHandler
    {
        private static Dictionary<string, string> _processes = new Dictionary<string, string>();
        public BasicActivityHandler()
        {

        }

        public string GetActivityDescription(IntPtr hWnd)
        {
            throw new NotImplementedException();
        }

        public IActivity GetBasicActivity(string description)
        {
            return new Activity() { ProcessName = description };
        }

        
    }
}
