using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hawk.Decode.Configuration.Model
{
	public class DataFrameConfiguration
	{
		public string Name { get; set; } = "";
		public string Version { get; set; } = "";

        [JsonPropertyName("arinc717")]
		public ARINC717Configuration? ARINC717 { get; set; }

		[JsonPropertyName("functions")]
		public List<Function> Functions { get; set; } = new List<Function>();
	}
}

