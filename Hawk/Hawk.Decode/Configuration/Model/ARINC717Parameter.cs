using System;
using System.Collections.Generic;

namespace Hawk.Decode.Configuration.Model
{
	public class ARINC717Parameter
	{
		private string _escapedName = "";
		public string Name
        {
            get
            {
				return _escapedName;
            }
            set
            {
                _escapedName = value
                    .Replace("/", "_")
                    .Replace(" ", "_");
            }
        }
		public string? Units { get; set; }
        public List<ARINC717ParameterLocation> Locations { get; set; } = new List<ARINC717ParameterLocation>();

		public double? Resolution { get; set; }
		public double? Offset { get; set; }

		/// <summary>
        /// True if it's a signed value and the MSB indicates if the # is negative.
        /// </summary>
		public bool Signed { get; set; }

        public override string ToString()
        {
			return Name;
		}
    }
}

