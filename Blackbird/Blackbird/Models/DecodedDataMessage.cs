using System;
using System.Collections.Generic;

namespace Blackbird.Models
{
    public class DecodedDataMessage
    {
        public string AircraftIdentifier { get; set; } = "";
        public DateTime ProcessedTime { get; set; } = DateTime.UtcNow;
        public Dictionary<string, List<object>> DecodedValues { get; set; } = new Dictionary<string, List<object>>();
    }
}

