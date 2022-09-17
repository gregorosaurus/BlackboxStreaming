using System;
using Hawk.ARINC717;
using Hawk.Decode.Configuration.Model;

namespace Hawk.Decode
{
    public class FDRDecoder
    {
        private DataFrameConfiguration _configuration;
        public FDRDecoder(DataFrameConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<string, List<object>> DecodeSubframe(byte[] subframeData)
        {
            Dictionary<string, List<object>> values = new Dictionary<string, List<object>>();
            Subframe subframe;
            switch (_configuration.ARINC717?.WordFormat)
            {
                case ARINC717WordFormat.LittleEndianUnpacked16BitWords:
                    
                    UInt16[] words = ConvertWordDataFromLittleEndian16Bits(subframeData);
                    subframe = new Subframe(words);
                    break;
                default:
                    throw new NotImplementedException("Other word formats are currently not supported");
            }

            if (subframe.SubFrameNumber == -1)
                throw new DecodeException("Invalid subframe found, missing sync word.");

            ProcessSubframe(subframe, values);

            return values;
        }

        private void ProcessSubframe(Subframe subframe, Dictionary<string, List<object>> values)
        {
            foreach (var parameter in _configuration.ARINC717!.Parameters)
            {
                foreach(var location in parameter.Locations)
                {
                    UInt16 rawWord = subframe.WordRawData(location.Word);
                    object value = ARINC717Decode.Decode(parameter, location, rawWord);

                    if (!values.ContainsKey(parameter.Name))
                        values.Add(parameter.Name, new List<object>());

                    values[parameter.Name].Add(value);
                }
            }

            //not implmented if functions. Not needed for simple QAR decoding.

        }

        private bool IsParameterLocationInSubframeAndSuperframe(ARINC717ParameterLocation location, Subframe subframe, int superframe)
        {
            bool isInSubframe = location.Subframe == null || location.Subframe == subframe.SubFrameNumber;
            bool isInSuperframe = location.Superframe == null || location.Superframe == superframe;

            return isInSubframe && isInSuperframe;
        }

        private UInt16[] ConvertWordDataFromLittleEndian16Bits(byte[] subframeData)
        {
            if (subframeData.Length != _configuration.ARINC717!.WordsPerSecond * 2)
                throw new ArgumentException("Invalid subframe data, we were expecting a buffer twice the length of the wps");

            UInt16[] wordData = new UInt16[_configuration.ARINC717.WordsPerSecond];
            for (int i = 0; i < subframeData.Length; i += 2)
            {
                wordData[i / 2] = (UInt16)((subframeData[i + 1] << 8) | subframeData[i]);
            }

            return wordData;
        }
    }
}

