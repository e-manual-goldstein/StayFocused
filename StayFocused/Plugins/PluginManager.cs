using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused.Plugins
{
    internal class PluginManager
    {
        ConfigManager _configManager;
        IDictionary<string, IPlugin> _plugins;
        ILogManager _logManager;

        public PluginManager(ConfigManager configManager, ILogManager logManager) 
        {
            _configManager = configManager;
            _logManager = logManager;
        }

        internal void Initialise()
        {
            var pluginConfigPath = _configManager.GetConfigSetting("PluginConfig");
            LoadPluginConfig(pluginConfigPath);
        }

        private IDictionary<string, IPlugin> LoadPluginConfig(string pluginConfigPath)
        {
            if (File.Exists(pluginConfigPath))
            {
                var text = File.ReadAllText(pluginConfigPath);
                try
                {
                    _logManager.Log("Loading plugin config from file...");
                    var plugins = JsonConvert.DeserializeObject<Dictionary<string, PluginDefinition>>(text);
                    _plugins = CreatePlugins(plugins).ToDictionary(t => t.PluginName);
                }
                catch (Exception ex)
                {
                    _logManager.Log($"Unable to load plugins:\n\t{ex.Message}");
                }
                
            }
            return new Dictionary<string, IPlugin>();
        }

        private IEnumerable<IPlugin> CreatePlugins(Dictionary<string, PluginDefinition> pluginDefinitions)
        {
            foreach (var (name, pluginDefinition) in pluginDefinitions)
            {
                if (TryLoadModule(pluginDefinition.ModuleName, out var module))
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
                _logManager.Log($"Could not load plugin: {fullTypeName}");
                _logManager.Log(ex.Message);
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
                    _logManager.Log($"Could not load module {moduleName}");
                    _logManager.Log(ex.Message);
                    throw;
                }
            }
            return success;
        }
    }
}
