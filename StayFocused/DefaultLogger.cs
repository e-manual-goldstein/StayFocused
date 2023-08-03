using StayFocused.Api;
using System;

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
