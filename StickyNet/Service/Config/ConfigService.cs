using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace StickyNet.Service
{
    public class ConfigService
    {
        private readonly ILogger<ConfigService> Logger;

        private System.Timers.Timer RefreshTimer { get; set; }
        private List<StickyServerConfig> Configs { get; set; }

        public IReadOnlyList<StickyServerConfig> ServerConfigs => Configs.AsReadOnly();
        public StickyGlobalConfig StickyConfig { get; private set; }

        public string ConfigFolderPath
            => RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "/etc/stickynet.d"
                : "stickynet.d";

        public string GlobalConfigFilePath
            => RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "/etc/stickynet.cfg"
                : "stickynet.cfg";

        public string HasChangesFile
            => Path.Combine(ConfigFolderPath, "changes.lock");


        public event Func<StickyServerConfig, Task> ServerAdded;
        public event Func<StickyServerConfig, Task> ServerRemoved;

        public ConfigService(ILogger<ConfigService> logger)
        {
            Logger = logger;

            Configs = new List<StickyServerConfig>();
            RefreshTimer = new System.Timers.Timer(1000)
            {
                AutoReset = false,
            };
            RefreshTimer.Elapsed += RefreshAsync;

            RefreshConfigFileAsync(true).GetAwaiter().GetResult();
            RefreshTimer.Start();
        }

        public async Task MarkChangesAsync()
        {
            CreateConfigDirectory();
            Logger.LogInformation("Creating reload file...");
            await File.WriteAllTextAsync(HasChangesFile, "");
        }

        public async Task RefreshConfigFileAsync(bool force = false)
        {
            CreateConfigDirectory();

            bool requireRefresh = force || File.Exists(HasChangesFile);

            if (!requireRefresh)
            {
                return;
            }

            Logger.LogInformation("Reloading config...");

            var newGlobalConfig = await LoadGlobalConfigAsync();

            if (newGlobalConfig != StickyConfig)
            {
                StickyConfig = newGlobalConfig;
            }

            var newServerConfigs = await LoadServerConfigsAsync();
            var addedConfigs = newServerConfigs.Except(Configs);
            var removedConfigs = Configs.Except(newServerConfigs);

            var duplicates = newServerConfigs.GroupBy(x => x.Port)
                    .Where(x => x.Count() > 1)
                    .ToList();

            foreach (var duplicate in duplicates)
            {
                int port = duplicate.Key;
                Logger.LogError($"There are multiple StickyNets registered for port {port}! Please remove the duplicates!");
                Logger.LogWarning("Ignoring duplicates!");

                newServerConfigs.RemoveAll(x => x.Port == port);
                newServerConfigs.Add(duplicate.First());
            }

            foreach (var config in removedConfigs)
            {
                if (ServerRemoved != null)
                {
                    await ServerRemoved.Invoke(config);
                }
            }
            foreach (var config in addedConfigs)
            {
                if (ServerAdded != null)
                {
                    await ServerAdded.Invoke(config);
                }
            }


            Configs = newServerConfigs.ToList();

            File.Delete(HasChangesFile);
        }

        private async Task<StickyGlobalConfig> LoadGlobalConfigAsync()
        {
            if (!File.Exists(GlobalConfigFilePath))
            {
                return new StickyGlobalConfig();
            }

            string json = await File.ReadAllTextAsync(GlobalConfigFilePath);
            var cfg = JsonConvert.DeserializeObject<StickyGlobalConfig>(json);

            return cfg.IsValid
                ? cfg
                : new StickyGlobalConfig();
        }
        private async Task<List<StickyServerConfig>> LoadServerConfigsAsync()
        {
            if (!Directory.Exists(ConfigFolderPath))
            {
                return new List<StickyServerConfig>();
            }

            var configs = new List<StickyServerConfig>();

            foreach (string configPath in Directory.EnumerateFiles(ConfigFolderPath).Where(x => x.EndsWith(".cfg")))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(configPath);
                    var config = JsonConvert.DeserializeObject<StickyServerConfig>(json);

                    if (!config.IsValid())
                    {
                        Logger.LogWarning($"Found a invalid config file! [{configPath}]");
                        continue;
                    }

                    config.FilePath = configPath;
                    configs.Add(config);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"The config files contain broken json! [{configPath}]");
                }
            }

            return configs;
        }
        private void CreateConfigDirectory()
        {
            if (!Directory.Exists(ConfigFolderPath))
            {
                Logger.LogDebug("Creating Config Directory...");
                Directory.CreateDirectory(ConfigFolderPath);
            }
        }
        public async Task AddReportServerAsync(Uri reportServer, string reportToken)
        {
            Logger.LogInformation("Adding reportserver...");
            StickyConfig.AddTripLink(reportServer, reportToken);
            await SaveGlobalConfigAsync();
        }
        public async Task RemoveReportServerAsync(Uri reportServer)
        {
            Logger.LogInformation("Removing reportserver...");
            StickyConfig.RemoveTripLink(reportServer);
            await SaveGlobalConfigAsync();
        }

        public Task DeleteServerConfigAsync(StickyServerConfig config)
        {
            File.Delete(config.FilePath);
            Configs.Remove(config);
            Logger.LogInformation($"Removed the following StickyNet:\n{config}");
            return Task.CompletedTask;
        }
        public async Task AddServerConfigAsync(StickyServerConfig config)
        {
            CreateConfigDirectory();
            config.FilePath = GenerateFilePath(config);
            string json = JsonConvert.SerializeObject(config);
            await File.WriteAllTextAsync(config.FilePath, json);
            Logger.LogInformation("Successfully saved config file!");
            Configs.Add(config);
        }

        private async Task SaveGlobalConfigAsync()
        {
            string json = JsonConvert.SerializeObject(StickyConfig);
            await File.WriteAllTextAsync(GlobalConfigFilePath, json);
            Logger.LogInformation("Successfully saved config file!");
        }

        private string GenerateFilePath(StickyServerConfig config)
        {
            string path = Path.Combine(ConfigFolderPath, $"{config.Port}-{config.Protocol}.cfg");
            int i = 0;
            while (File.Exists(path))
            {
                path = Path.Combine(ConfigFolderPath, $"{config.Port}-{config.Protocol}-{i}.cfg");
                i++;
            }

            return path;
        }

        private async void RefreshAsync(object sender, EventArgs e)
        {
            await RefreshConfigFileAsync();
            RefreshTimer.Start();
        }
    }
}
