using System;
using System.Threading.Tasks;

namespace Blackbird.Services
{
    public interface IFDRDataNotificationService
    {
        Task NotifyOfNewSubframeDataAsync(string acIdent, byte[] subframeData);
    }
}

