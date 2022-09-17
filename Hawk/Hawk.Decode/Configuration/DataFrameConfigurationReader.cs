using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hawk.Decode.Configuration.Model;

namespace Hawk.Decode.Configuration
{
	public class DataFrameConfigurationReader
	{
		public DataFrameConfiguration? Read(Stream stream)
        {
			string json = "";

			using (StreamReader sr = new StreamReader(stream))
			{
				json = sr.ReadToEnd();
			}

			return Read(json);
        }

		public DataFrameConfiguration? Read(string json)
        {
			JsonSerializerOptions options = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true,
			};
			options.Converters.Add(new JsonStringEnumConverter());
			return JsonSerializer.Deserialize<DataFrameConfiguration>(json, options);
        }
	}
}

