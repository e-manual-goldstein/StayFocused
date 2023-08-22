using Newtonsoft.Json;
using StayFocused.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StayFocused.Plugins
{
    public class PluginService
    {
        ConfigManager _configManager;
        IDictionary<string, IPlugin> _plugins;

        public PluginService(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public IDictionary<string, IPlugin> Plugins => _plugins;

        internal void Initialise()
        {
            var pluginConfigPath = _configManager.GetConfigSetting("PluginConfig");
            _plugins = LoadPluginConfig(pluginConfigPath);
        }

        private IDictionary<string, IPlugin> LoadPluginConfig(string pluginConfigPath)
        {
            if (File.Exists(pluginConfigPath))
            {
                var text = File.ReadAllText(pluginConfigPath);
                try
                {
                    var plugins = JsonConvert.DeserializeObject<Dictionary<string, PluginDefinition>>(text);
                    return CreatePlugins(plugins).ToDictionary(t => t.PluginName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to load plugins:\n\t{ex.Message}");
                }

            }
            return new Dictionary<string, IPlugin>();
        }

        private IEnumerable<IPlugin> CreatePlugins(Dictionary<string, PluginDefinition> pluginDefinitions)
        {
            foreach (var (name, pluginDefinition) in pluginDefinitions)
            {
                if (TryLoadModule(pluginDefinition.ModuleLocation, out var module))
                {
                    if (TryLoadPlugin(module, pluginDefinition.FullTypeName, out var plugin))
                    {
                        yield return plugin;
                    }
                }
            }
        }

        private bool TryLoadPlugin(Assembly module, string fullTypeName, out IPlugin plugin)
        {
            plugin = null;
            try
            {
                var pluginType = module.GetType(fullTypeName);
                if (typeof(IPlugin).IsAssignableFrom(pluginType))
                {
                    plugin = Activator.CreateInstance(pluginType) as IPlugin;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load plugin: {fullTypeName}");
                Console.WriteLine(ex.Message);
            }
            return plugin != null;
        }

        private bool TryLoadModule(string moduleName, out Assembly module)
        {
            bool success = false;
            module = null;
            if (File.Exists(moduleName))
            {
                try
                {
                    module = Assembly.LoadFrom(moduleName);
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load module {moduleName}");
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
            return success;
        }
    }
}
