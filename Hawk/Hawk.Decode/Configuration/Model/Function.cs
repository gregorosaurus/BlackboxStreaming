using System;
namespace Hawk.Decode.Configuration.Model
{
	public class Function
	{
		public string Name { get; set; } = "";
		public string Expr { get; set; } = "";
		public string? Units { get; set; }

		public FunctionReturnType ReturnType { get; set; } = FunctionReturnType.Double;

	}
}

