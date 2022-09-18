using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hawk.Decode.Configuration.Model;

namespace Hawk.Services
{
    public class ConfigurationCache
    {
        private Dictionary<string, DataFrameConfiguration> _configCache = new Dictionary<string, DataFrameConfiguration>();

        private Services.IFDRConfigurationService _configService;
        public ConfigurationCache(Services.IFDRConfigurationService configService)
        {
            _configService = configService;
        }
        public async Task RefreshAsync()
        {
            _configCache = await _configService.RetrieveAllConfigurationsAsync();
        }

        public DataFrameConfiguration? FindDataFrameConfig(string acIdent)
        {
            if (_configCache.TryGetValue(acIdent, out DataFrameConfiguration? config))
            {
                return config;
            }
            else
            {
                return null;
            }
        }
    }
}

