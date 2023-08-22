using Microsoft.Extensions.DependencyInjection;
using StayFocused;
using StayFocused.Api;

namespace ExampleModule
{
    public class ExamplePlugin : IPlugin
    {
        public string PluginName => "Example Plugin";

        public void OnPluginLoaded(IServiceCollection services)
        {
            
        }

        public void OnServicesBuilt(IServiceProvider serviceCollection)
        {

        }
    }
}