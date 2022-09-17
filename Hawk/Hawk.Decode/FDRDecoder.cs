using System;
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

        public Dictionary<string,object[]> DecodeSubframe(byte[] subframeData)
        {
            throw new NotImplementedException();
        }
    }
}

