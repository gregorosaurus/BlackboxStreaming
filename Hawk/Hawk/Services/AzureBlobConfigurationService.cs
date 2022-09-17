using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake;
using Hawk.Decode.Configuration;
using Hawk.Decode.Configuration.Model;

namespace Hawk.Services
{
    public class AzureBlobConfigurationService :IFDRConfigurationService
    {
        public class Options
        {
            public DataLakeServiceClient? DataLakeServiceClient { get; set; }
            public string ConfigurationFileSystem { get; set; } = "configs";
        }


        private DataLakeFileSystemClient _configFileSystemClient;
        public AzureBlobConfigurationService(Options options)
        {
            if (options.DataLakeServiceClient == null)
                throw new ArgumentNullException("DataLakeServiceClient");

            _configFileSystemClient = options.DataLakeServiceClient!.GetFileSystemClient(options.ConfigurationFileSystem);

        }

        public async Task<DataFrameConfiguration?> FindConfigurationAsync(string acIdent)
        {
            DataLakeFileClient configClient = _configFileSystemClient.GetFileClient(acIdent + ".json");
            if (!(await configClient.ExistsAsync()))
            {
                return null;
            }
            DataFrameConfigurationReader reader = new DataFrameConfigurationReader();
            using (Stream configStream = await configClient.OpenReadAsync())
            {
                return reader.Read(configStream);
            }
        }
    }
}

