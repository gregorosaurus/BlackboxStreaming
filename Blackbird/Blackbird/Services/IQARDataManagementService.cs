using System;
using System.IO;
using System.Threading.Tasks;

namespace Blackbird.Services
{
    public interface IQARDataManagementService
    {
        Task UploadSubframeAsync(string registration, byte[] subframeData);
        Task UploadSubframeAsync(string registration, Stream subframeStream);
        Task CloseStaleRecordingsAsync();
    }
}

