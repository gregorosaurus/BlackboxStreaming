using System;
using System.Text.RegularExpressions;

namespace Hawk.Decode
{

    public static class CustomFunctions
    {

        /// <summary>
        /// Decodes a boeing Longitude / longitude, (12CC)
        /// </summary>
        /// <param name="msp"></param>
        /// <param name="lsp"></param>
        /// <param name="mspBitCount">All MSP bits, including sign bit</param>
        /// <param name="lspBitCount">All LSP bits, including sign bit</param>
        /// <returns></returns>
        public static double DecodeBoeingLatLon(double msp, double lsp, int mspBitCount, int lspBitCount, double resolution)
        {
            //convert to ints to do bit shifting
            uint mspBits = (uint)msp;
            uint lspBits = (uint)lsp;

            //move the mspbits the amount of lsp bits -1, -1 for the sign bit, which
            //we will remove soon
            mspBits <<= lspBitCount - 1;

            //remove the sign bit
            lspBits &= ARINC717Decode.GenerateSelectionMask(lspBitCount - 1);

            uint allBits = mspBits | lspBits;

            return ARINC717Decode.DecodeBNR(new Configuration.Model.ARINC717Parameter()
            {
                Resolution = resolution,
                Offset = 0,
                Signed = true,
            }, new Configuration.Model.ARINC717ParameterLocation()
            {
                Lsb = 1,
                Msb = mspBitCount + lspBitCount - 1,
            }, allBits);
        }

        public static string UTF8(double val)
        {
            return char.ConvertFromUtf32((int)val);
        }

        public static string Concat(params string[] strings)
        {
            return string.Join("", strings);
        }

        public static string DiscreteDecode(double discreteValue, int trueState, string trueValue, string falseValue)
        {
            if ((int)discreteValue == trueState)
            {
                return trueValue;
            }
            else
            {
                return falseValue;
            }
        }


        /// <summary>
        /// Returns a value that is mapped from another number.
        /// ie. 1.5123 -> 5 degrees
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tolerance">The tolerance it can be away from the key for it to be correct</param>
        /// <param name="mappings">In the format [key,value], [key, value], etc</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static double RetrieveMappedValue(double value, double tolerance, string mappings)
        {


            foreach (Match mapping in Regex.Matches(mappings, @"\[\s*?([\d\.]+)\s*?,\s*?([\d\.]+)\s*?\]"))
            {
                double mapKey = double.Parse(mapping.Groups[1].Value);
                double mapValue = double.Parse(mapping.Groups[2].Value);

                if (Math.Abs(mapKey - value) <= tolerance)
                {
                    return mapValue;
                }
            }
            throw new Exception("No mapping found for value.");
        }


        /// <summary>
        /// Returns a string value that is mapped from another number.
        /// ie. 1.5123 -> 5 degrees
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tolerance">The tolerance it can be away from the key for it to be correct. Typically this is 0.0d</param>
        /// <param name="mappings">In the format [key,value], [key, value], etc</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string RetrieveStringMappedValue(double value, double tolerance, string mappings)
        {


            foreach (Match mapping in Regex.Matches(mappings, @"\[\s*?([\d\.]+)\s*?,(.+?)\]"))
            {
                double mapKey = double.Parse(mapping.Groups[1].Value);
                string mapValue = mapping.Groups[2].Value.Trim();

                if (Math.Abs(mapKey - value) <= tolerance)
                {
                    return mapValue;
                }
            }
            throw new Exception("No mapping found for value.");
        }

        public static int Int(double value)
        {
            return (int)value;
        }

        public static int Floor(double value)
        {
            return (int)Math.Floor(value);
        }

        public static int Ceiling(double value)
        {
            return (int)Math.Ceiling(value);
        }

        public static int Round(double value)
        {
            return (int)Math.Round(value);
        }

        /// <summary>
        /// Because flee doesn't have this built in. 
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static int BitwiseOr(int value1, int value2)
        {
            return value1 | value2;
        }

        /// <summary>
        /// Because flee doesn't have this built in. 
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static int BitwiseAnd(int value1, int value2)
        {
            return value1 & value2;
        }
    }
}

