using System;
using Loon.Utility;
using System.Text.Json.Serialization;

namespace Loon.Models
{
    public class DecodedDataMessage
    {
        public string AircraftIdentifier { get; set; } = "";
        public DateTime ProcessedTime { get; set; } = DateTime.UtcNow;
        [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
        public Dictionary<string, List<object>> DecodedValues { get; set; } = new Dictionary<string, List<object>>();
    }
}

