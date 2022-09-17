using System;
namespace Hawk.Models
{
    public class RawDataMessage
    {
        public string AircraftIdentifier { get; set; } = "";
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// base64 encoded subframe data
        /// </summary>
        public string? SubframeData { get; set; }

        public byte[] SubframeBinaryData
        {
            get
            {
                if (SubframeData == null)
                    return new byte[0];

                return Convert.FromBase64String(SubframeData!);
            }
        }
    }
}

