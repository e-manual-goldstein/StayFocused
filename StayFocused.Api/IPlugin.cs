using Microsoft.Extensions.DependencyInjection;
using System;

namespace StayFocused
{
    public interface IPlugin
    {
        string PluginName { get; }

        void OnPluginLoaded(IServiceCollection services);
        void OnServicesBuilt(IServiceProvider serviceCollection);
    }
}
