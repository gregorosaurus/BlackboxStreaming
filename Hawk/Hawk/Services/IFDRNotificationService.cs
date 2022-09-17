using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hawk.Services
{
    public interface IFDRNotificationService
    {
        Task SendNotificationOfDecodedDataAsync(string acIdent, Dictionary<string, List<object>> decodedData);
    }
}

