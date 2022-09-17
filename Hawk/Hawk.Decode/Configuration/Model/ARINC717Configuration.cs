using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hawk.Decode.Configuration.Model
{
	public class ARINC717Configuration
	{
		public int WordsPerSecond { get; set; } = 512;
		public ARINC717WordFormat WordFormat { get; set; } = ARINC717WordFormat.LittleEndianUnpacked16BitWords;

		public ARINC717Parameter? superframeParameter { get; set; }

		[JsonPropertyName("params")]
		public List<ARINC717Parameter> Parameters { get; set; } = new List<ARINC717Parameter>();
	}
}

