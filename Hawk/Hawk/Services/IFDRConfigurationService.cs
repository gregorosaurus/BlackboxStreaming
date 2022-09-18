using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hawk.Decode.Configuration.Model;

namespace Hawk.Services
{
    public interface IFDRConfigurationService
    {
        Task<DataFrameConfiguration?> FindConfigurationAsync(string acIdent);
        Task<Dictionary<string, DataFrameConfiguration>> RetrieveAllConfigurationsAsync();
    }
}

