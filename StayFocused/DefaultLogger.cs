using StayFocused.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused
{
    public class DefaultLogger : ILogManager
    {
        public void Log(string v)
        {
            Console.WriteLine(v);
        }
    }
}
