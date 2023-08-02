using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused.Plugins
{
    public sealed class PluginDefinition
    {
        public string Name { get; set; }

        public string FullTypeName { get; set; }

        public string ModuleName { get; set; }
    }
}
