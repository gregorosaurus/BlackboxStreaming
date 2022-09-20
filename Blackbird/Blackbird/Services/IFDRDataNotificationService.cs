using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blackbird.Services
{
    public interface IFDRDataNotificationService
    {
        Task NotifyOfNewSubframeDataAsync(string acIdent, byte[] subframeData);
        Task NotifyOfNewDecodedDataAsync(string acIdent, Dictionary<string, List<object>> values);
    }
}

