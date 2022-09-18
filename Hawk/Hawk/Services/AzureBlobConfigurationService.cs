using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string? configFileName = null;
            DataLakeFileClient mappingFileClient = _configFileSystemClient.GetFileClient("_mapping.csv");
            using (Stream mappingFileStream = await mappingFileClient.OpenReadAsync())
            using (StreamReader sr = new StreamReader(mappingFileStream))
            {
                string mappingContent = await sr.ReadToEndAsync();
                IEnumerable<string> mappingLines = mappingContent.Split("\n").Where(x=>x != "").Select(x=>x.Trim());
                foreach(string mappingLine in mappingLines)
                {
                    string[] mappingLineComponents = mappingLine.Split(",");
                    if (mappingLineComponents[0] == acIdent)
                    {
                        configFileName = mappingLineComponents[1] + ".json";
                        break;
                    }    
                }
            }
            if (configFileName == null)
                return null;//none found

            DataLakeFileClient configClient = _configFileSystemClient.GetFileClient(configFileName);
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

        public async Task<Dictionary<string, DataFrameConfiguration>> RetrieveAllConfigurationsAsync()
        {
            Dictionary<string, DataFrameConfiguration> configurations = new Dictionary<string, DataFrameConfiguration>();
            DataLakeFileClient mappingFileClient = _configFileSystemClient.GetFileClient("_mapping.csv");
            using (Stream mappingFileStream = await mappingFileClient.OpenReadAsync())
            using (StreamReader sr = new StreamReader(mappingFileStream))
            {
                string mappingContent = await sr.ReadToEndAsync();
                IEnumerable<string> mappingLines = mappingContent.Split("\n").Where(x => x != "").Select(x => x.Trim());
                foreach (string mappingLine in mappingLines)
                {
                    string[] mappingLineComponents = mappingLine.Split(",");
                    string acIdent = mappingLineComponents[0];
                    string configFileName = mappingLineComponents[1];

                    DataLakeFileClient configClient = _configFileSystemClient.GetFileClient(configFileName+".json");
                    if (!(await configClient.ExistsAsync()))
                    {
                        continue;
                    }
                    DataFrameConfigurationReader reader = new DataFrameConfigurationReader();
                    using (Stream configStream = await configClient.OpenReadAsync())
                    {
                        var config = reader.Read(configStream);
                        if (config != null)
                            configurations.Add(acIdent, config);
                    }
                }
            }
            return configurations;
        }
    }
}

