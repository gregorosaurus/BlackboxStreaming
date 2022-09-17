using System;
using System.Threading.Tasks;
using Hawk.Decode.Configuration.Model;

namespace Hawk.Services
{
    public interface IFDRConfigurationService
    {
        Task<DataFrameConfiguration?> FindConfigurationAsync(string acIdent);
    }
}

