using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace StayFocused
{
    public class ConfigManager
    {
        public ConfigManager()
        {
        }

        public ConfigManager(bool fullReset)
        {
            if (fullReset)
            {
                File.Delete(ConfigFilePath);
            }
        }

        private const string ConfigFilePath = "config.json";
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        public event Func<string, string> SettingNotFound;

        public string GetConfigSetting(string key)
        {
            if (!File.Exists(ConfigFilePath))
            {
                CreateDefaultConfig();
                Console.WriteLine($"A new config.json file has been created at: {Path.GetFullPath(ConfigFilePath)}");
            }

            var configContent = File.ReadAllText(ConfigFilePath);
            var configDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(configContent, jsonOptions);

            if (configDictionary.TryGetValue(key, out var value))
            {
                Console.WriteLine($"Using existing config value: {key}|{value}");
                return value;
            }

            return OnSettingNotFound(key);
        }

        private string OnSettingNotFound(string key)
        {
            if (SettingNotFound == null)
            {
                SettingNotFound += HandleSettingNotFound;
            }
            Console.WriteLine("Setting not found in the config file. Invoking event handler to get the value.");
            var settingValue = SettingNotFound.Invoke(key);
            UpdateConfigFile(key, settingValue);
            Console.WriteLine("Updated config file with the provided value.");
            return settingValue;
        }


        private void CreateDefaultConfig()
        {
            Console.WriteLine("Config file not found. Creating default config.");

            var defaultConfig = new Dictionary<string, string> { };

            var configContent = JsonSerializer.Serialize(defaultConfig, jsonOptions);
            File.WriteAllText(ConfigFilePath, configContent);
        }

        private void UpdateConfigFile(string settingName, string settingValue)
        {
            Console.WriteLine($"Updating config file. Setting name: {settingName}, Setting value: {settingValue}");

            var configContent = File.ReadAllText(ConfigFilePath);
            var configDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(configContent, jsonOptions);

            if (configDictionary.ContainsKey(settingName))
            {
                configDictionary[settingName] = settingValue;
            }
            else
            {
                configDictionary.Add(settingName, settingValue);
            }

            var updatedContent = JsonSerializer.Serialize(configDictionary, jsonOptions);
            File.WriteAllText(ConfigFilePath, updatedContent);
        }

        private static string HandleSettingNotFound(string settingName)
        {
            Console.WriteLine($"Please input a value for the following config setting: {settingName}");
            return Console.ReadLine();
        }
    }
}
