using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;

namespace Blackbird.Services
{
    public class AzureQARDataService : IQARDataManagementService
    {
        public class Options
        {
            /// <summary>
            /// The blob service client configured with the appropriate
            /// authentication.
            /// </summary>
            public DataLakeServiceClient? DataLakeClient { get; set; }

            public string DataUploadFileSystemName { get; set; } = "fdr-raw";
        }

        private DataLakeFileSystemClient _fdrFileSystemClient;

        private const string RecordingStagingDirectory = "_recording";
        private const string RecordingStartTimeMetadataKey = "recording_start_time";
        private static readonly TimeSpan RecordingCloseThreshold = TimeSpan.FromMinutes(15);

        private ILogger<AzureQARDataService> _logger;
        public AzureQARDataService(Options options, ILogger<AzureQARDataService> logger)
        {
            if (options.DataLakeClient == null)
                throw new ArgumentNullException("Options.BlobServiceClient");

            _fdrFileSystemClient = options.DataLakeClient.GetFileSystemClient(options.DataUploadFileSystemName);
            _logger = logger;
        }


        /// <summary>
        /// To upload the subframe, we do the following:
        /// 1) Check to see if there is a current "recording" in progress.
        /// 2) If there is not a recording, we create a new file
        /// 3) if there is already a recording file, check to see if the last
        /// modified time is *near* our recording time.
        /// 4) If this subframe is from a new recording, we *close* the current recording
        /// and then start a new recording. 
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public async Task UploadSubframeAsync(string registration, Stream subframeStream)
        {
            _logger.LogTrace($"uploading subframe data from {registration}");

            //check to see if there is a current recording
            //get recording folder
            DataLakeDirectoryClient recordingDirectory = _fdrFileSystemClient.GetDirectoryClient(RecordingStagingDirectory);
            DataLakeFileClient recordingFile = recordingDirectory.GetFileClient($"{registration}.fdr");

            //we assume the recording directory is created, we don't want to check every subframe. 

            if (!(await recordingFile.ExistsAsync()))
            {
                _logger.LogInformation($"Creating new recording for aircraft {registration}");
                await StartFDRRecordingAsync(recordingFile);
            }

            var props = await recordingFile.GetPropertiesAsync();
            Int64 currentLength = props.Value.ContentLength;

            await recordingFile.AppendAsync(subframeStream, currentLength);
            var response = await recordingFile.FlushAsync(currentLength + subframeStream.Length);
            if (response.GetRawResponse().Status / 100 != 2)
            {
                throw new Exception($"Invalid status code returned by blob storage: {response.GetRawResponse().Status}");
            }
        }

        public async Task UploadSubframeAsync(string registration, byte[] subframeData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(subframeData, 0, subframeData.Length);
                ms.Position = 0;
                await UploadSubframeAsync(registration, ms);
            }
        }

        private async Task StartFDRRecordingAsync(DataLakeFileClient recordingFile)
        {
            
            //create a new append blob
            await recordingFile.CreateAsync();

            //set new metadata
            Dictionary<string, string> metadata = new Dictionary<string, string>()
            {
                {RecordingStartTimeMetadataKey,DateTime.UtcNow.ToString("yyyy-MM-dd\\THH:mm:ss") },
            };

            await recordingFile.SetMetadataAsync(metadata);

        }
        

        /// <summary>
        /// returns the root folder of the FDR data currently being recorded. 
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        private string AircraftDataFolderPath(string registration)
        {
            return Path.Combine(registration);
        }


        /// <summary>
        /// "closes" the recordings.  
        /// </summary>
        /// <returns></returns>
        public async Task CloseStaleRecordingsAsync()
        {
            DataLakeDirectoryClient recordingDirectory = _fdrFileSystemClient.GetDirectoryClient(RecordingStagingDirectory);
            await foreach(PathItem aircraftRecording in recordingDirectory.GetPathsAsync(recursive:false))
            {
                if(DateTime.UtcNow - aircraftRecording.LastModified > RecordingCloseThreshold)
                {
                    //CLOSE
                    //by closing it, we mean moving it to the specific directory.
                    string acIdent = Path.GetFileNameWithoutExtension(aircraftRecording.Name);

                    _logger.LogInformation($"Creating recording for aircraft {acIdent}");

                    DataLakeFileClient acRecordingFile = _fdrFileSystemClient.GetFileClient(aircraftRecording.Name);
                    var propertiesResponse = await acRecordingFile.GetPropertiesAsync();
                    DateTime? recordingSaveTime = null;
                    if (propertiesResponse.Value.Metadata.TryGetValue(RecordingStartTimeMetadataKey, out string? startTimeStringValue))
                    {
                        if(DateTime.TryParseExact(startTimeStringValue, "yyyy-MM-dd\\THH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedStartTime))
                        {
                            recordingSaveTime = parsedStartTime;
                        }
                    }
                    if(recordingSaveTime == null)
                        recordingSaveTime = DateTime.UtcNow; //just in case we didn't have it in the metadata, let's still keep going

                    string fileName = String.Join("_",
                        acIdent,
                        recordingSaveTime.Value.ToString("yyyyMMddHHmmss"),
                        DateTime.UtcNow.ToString("yyyyMMddHHmmss")) + ".fdr";
                    string saveFolder = $"{acIdent}/{recordingSaveTime.Value.Year}/{recordingSaveTime.Value.Month.ToString("00")}/{recordingSaveTime.Value.Day.ToString("00")}";

                    DataLakeDirectoryClient saveFolderDirClient = _fdrFileSystemClient.GetDirectoryClient(saveFolder);
                    await saveFolderDirClient.CreateIfNotExistsAsync();

                    await acRecordingFile.RenameAsync($"{saveFolder}/{fileName}");
                }
                else
                {
                    //stil active. 
                }
            }
        }
    }
}

