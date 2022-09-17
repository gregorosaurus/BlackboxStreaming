using System;
namespace Hawk.Decode.Configuration.Model
{
	public class ARINC717ParameterLocation
	{
		public int Word { get; set; } = 0;
		public int Lsb { get; set; } = 1;
		public int Msb { get; set; } = 12;
		public int? Subframe { get; set; }
		public int? Superframe { get; set; }
	}
}

