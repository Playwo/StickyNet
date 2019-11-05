using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using StickyNet.Server;

namespace StickyNet.Service
{
    public class ConfigService : StickyService
    {
        private Timer RefreshTimer { get; set; }
        private List<StickyServerConfig> ServerConfigs { get; set; }

        public string ConfigPath { get; private set; }
        public IReadOnlyList<StickyServerConfig> Configs => ServerConfigs.AsReadOnly();

        public event Func<StickyServerConfig, Task> ServerAdded;
        public event Func<StickyServerConfig, Task> ServerRemoved;

        public override async Task InitializeAsync()
        {
            ConfigPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "/etc/stickynet.cfg"
                : "stickynet.cfg";

            ServerConfigs = new List<StickyServerConfig>();

            RefreshTimer = new Timer(1500)
            {
                AutoReset = true,
            };

            await RefreshConfigFileAsync();

            RefreshTimer.Elapsed += RefreshAsync;
            RefreshTimer.Start();
        }

        public async Task RefreshConfigFileAsync()
        {
            List<StickyServerConfig> newServerConfigs;

            if (File.Exists(ConfigPath))
            {
                string json = await File.ReadAllTextAsync(ConfigPath);
                newServerConfigs = JsonSerializer.Deserialize<List<StickyServerConfig>>(json);
            }
            else
            {
                newServerConfigs = new List<StickyServerConfig>();
            }

            var addedConfigs = newServerConfigs.Except(ServerConfigs);

            foreach (var config in addedConfigs)
            {
                if (ServerAdded != null)
                {
                    await ServerAdded.Invoke(config);
                }
            }

            var removedConfigs = ServerConfigs.Except(newServerConfigs);

            foreach (var config in removedConfigs)
            {
                if (ServerRemoved != null)
                {
                    await ServerRemoved.Invoke(config);
                }
            }

            ServerConfigs = newServerConfigs;
        }

        public async Task<(bool, string)> AddStickyNetAsync(StickyServerConfig config)
        {
            RefreshTimer.Stop();

            if (ServerConfigs.Any(x => x.Port == config.Port))
            {
                return (false, "This port is already used by a StickyNet!");
            }

            ServerConfigs.Add(config);

            await SaveConfigAsync();

            RefreshTimer.Start();

            return (true, "");
        }

        public async Task<(bool, string)> RemoveStickyNetAsync(int port)
        {
            RefreshTimer.Stop();

            if (ServerConfigs.RemoveAll(x => x.Port == port) == 0)
            {
                return (false, $"There is no StickyNet on port {port}!");
            }

            await SaveConfigAsync();

            RefreshTimer.Start();

            return (true, "");
        }

        public async Task SaveConfigAsync()
        {
            string json = JsonSerializer.Serialize(ServerConfigs);
            await File.WriteAllTextAsync(ConfigPath, json);
        }

        private async void RefreshAsync(object sender, EventArgs e)
            => await RefreshConfigFileAsync();
    }
}
