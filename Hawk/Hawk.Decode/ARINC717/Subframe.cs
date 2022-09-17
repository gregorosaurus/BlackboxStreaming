using System;
namespace Hawk.ARINC717
{
    public class Subframe
    {
        public int SubFrameNumber
        {
            get
            {
                if (Data == null && Data?.Length > 0)
                    return -1;

                UInt32 firstWord = Data![0];
                for (int i = 0; i < 4; i++)
                {
                    if (firstWord == FrameSyncWords[i])
                        return i + 1;
                }
                return -1;
            }
        }

        public UInt16[] Data { get; private set; }

        private readonly int[] FrameSyncWords = { 0x247, 0x5b8, 0xa47, 0xdb8 };

        public Subframe(UInt16[] data)
        {
            Data = data;
        }

        /// <summary>
        /// The word number is the 1-based index word number
        /// </summary>
        /// <param name="wordNumber">1-based word index</param>
        /// <returns></returns>
        public UInt16 WordRawData(int wordNumber)
        {
            int zeroBasedWordNumber = wordNumber - 1;

            if (zeroBasedWordNumber >= 0 && zeroBasedWordNumber < Data.Length)
                return Data[zeroBasedWordNumber];
            else return 0;
        }
    }
}

