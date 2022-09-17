using System;
namespace Blackbird.Models
{
    public class RawDataMessage
    {
        public string? AircraftIdentifier { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// base64 encoded subframe data
        /// </summary>
        public string? SubframeData { get; set; }

        public RawDataMessage(string acIdent, byte[] data)
        {
            this.AircraftIdentifier = acIdent;
            this.SubframeData = Convert.ToBase64String(data);
        }

        public RawDataMessage()
        {

        }
    }
}

