using System;
using Hawk.Decode.Configuration.Model;

namespace Hawk.Decode
{
    public static class ARINC717Decode
    {
        public static double Decode(ARINC717Parameter parameter, ARINC717ParameterLocation location, uint rawWord)
        {
            if (location.Msb == location.Lsb && parameter.Resolution == null)
            {
                return DecodeDiscrete(parameter, location, rawWord);
            }
            else
            {
                return DecodeBNR(parameter, location, rawWord);
            }
        }

        public static double DecodeDiscrete(ARINC717Parameter parameter, ARINC717ParameterLocation location, uint rawWord)
        {
            int value;
            uint mask = GenerateSelectionMask(location.Msb - 1, location.Lsb - 1);
            rawWord &= mask; //we select only the bits we need.

            rawWord >>= location.Msb - 1;
            rawWord &= 0x1; //select only the last bit, just in case;

            value = Convert.ToInt32(rawWord);
            return value;
        }

        public static double DecodeBNR(ARINC717Parameter parameter, ARINC717ParameterLocation location, uint rawWord)
        {
            double value = 0;

            uint mask = GenerateSelectionMask(location.Lsb - 1, location.Msb - 1);
            rawWord &= mask; //we select only the bits we need.

            // bit shift to the right, the number of bits until LSB starts
            rawWord >>= location.Lsb - 1;

            //bit shift the mask as well, we'll need that if we do two's compliment.
            mask >>= location.Lsb - 1;

            //Find out if we're signed or not.  
            //the end bit is the sign bit, two's compliment
            bool signed = false;
            if (parameter.Signed)
            {
                signed = Convert.ToBoolean((rawWord >> (location.Msb - location.Lsb)) & 0x1);
                if (signed)
                {
                    //flip the bits, add one
                    rawWord ^= mask;
                    rawWord++;
                }
            }

            value = Convert.ToDouble(rawWord) * (signed ? -1 : 1);

            //multiply by the coefficient
            value *= parameter.Resolution ?? 1.0;
            //add the offset
            value += parameter.Offset ?? 0.0;
            //value 

            return value;
        }

        /// <summary>
        /// returns a mask, such as 0b000100000000 to be used to select one or more discrete bits.  
        /// It is important to note that the discrete bits indexes sent as a parameter must be indexed from 0-11, and not 1-12. 
        /// </summary>
        /// <param name="lsb"></param>
        /// <param name="msb"></param>
        /// <returns>A mask to select the bit(s)</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the discrete bit is less than 0 or greater than 11.</exception>
        public static uint GenerateSelectionMask(int lsb, int msb)
        {
            //if (msb < 0 || msb > 11 || lsb < 0 || msb > 11)
            //     throw new IndexOutOfRangeException("The position of the lsb and msb bits must be between 0 and 11 (inclusive)");

            uint mask = 0;
            for (int i = 0; i <= msb - lsb; i++)
            {
                //do this number of length times
                uint m = 1;
                for (int j = 0; j < msb - i; j++)
                    m <<= 1;
                mask |= m;
            }
            return mask;
        }

        /// <summary>
        /// Generates a mask used to select a certain number of bits of 12 or more bits.
        /// </summary>
        /// <param name="length">The number of bits to select, starting at bit 0</param>
        /// <returns>The mask used to bitwise and a group of bits</returns>
        public static uint GenerateSelectionMask(int length)
        {
            return GenerateSelectionMask(0, length - 1);
        }
    }
}

