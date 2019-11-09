using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace StickyNet.Service
{
    public class ConfigService : StickyService
    {
#pragma warning disable CS0649
        [Inject] private readonly ILoggerFactory LoggerFactory;
#pragma warning restore

        private ILogger<ConfigService> Logger;

        private Timer RefreshTimer { get; set; }
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
                : "/stickynet.cfg";

        public string HasChangesFile
            => Path.Combine(ConfigFolderPath, "changes.lock");


        public event Func<StickyServerConfig, Task> ServerAdded;
        public event Func<StickyServerConfig, Task> ServerRemoved;

        public override async Task InitializeAsync()
        {
            Configs = new List<StickyServerConfig>();
            Logger = LoggerFactory.CreateLogger<ConfigService>();
            await RefreshConfigFileAsync(true);

            RefreshTimer = new Timer(1000)
            {
                AutoReset = false,
            };
            RefreshTimer.Elapsed += RefreshAsync;
            RefreshTimer.Start();
        }

        public async Task MarkChangesAsync()
        {
            CreateConfigDirectory();
            Logger.LogInformation("Creating changes file...");
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

            File.Delete(HasChangesFile);

            var newGlobalConfig = await LoadGlobalConfigAsync();

            if (newGlobalConfig != StickyConfig)
            {
                StickyConfig = newGlobalConfig;
            }

            var newServerConfigs = await LoadServerConfigsAsync();
            var addedConfigs = newServerConfigs.Except(ServerConfigs);
            var removedConfigs = ServerConfigs.Except(newServerConfigs);

            foreach (var config in addedConfigs)
            {
                await ServerAdded?.Invoke(config);
            }
            foreach (var config in removedConfigs)
            {
                await ServerRemoved?.Invoke(config);
            }

            Configs = newServerConfigs.ToList();
        }

        private async Task<StickyGlobalConfig> LoadGlobalConfigAsync()
        {
            if (!File.Exists(GlobalConfigFilePath))
            {
                return new StickyGlobalConfig();
            }

            string json = await File.ReadAllTextAsync(GlobalConfigFilePath);
            return JsonSerializer.Deserialize<StickyGlobalConfig>(json);
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
                    var config = JsonSerializer.Deserialize<StickyServerConfig>(json);

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
            config.FilePath = CreateFilePath(config);
            string json = JsonSerializer.Serialize(config);
            await File.WriteAllTextAsync(config.FilePath, json);
            Configs.Add(config);
            Logger.LogInformation("Successfully created config file!");
        }

        private string CreateFilePath(StickyServerConfig config)
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
