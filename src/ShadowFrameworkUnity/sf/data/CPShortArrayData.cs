using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLab.ShadowFramework.Data
{
    public class CPShortArrayData
    {
        private const int MIDDLE_VALUE = 32768;

        public static int getBitSize(int size) {
            size++;
            int bitSize = 0;
            while (size > 0) {
                size = size >> 1;
                bitSize++;
            }
            return bitSize;
        }

        public static byte[] CompressShortsArray(short[] data)
        {
            int size = data.Length;
            int maxIndex = size > 0 ? (data[0]+ MIDDLE_VALUE) : 0;
            int minIndex = maxIndex;
            for (int i = 0; i < size; i++)
            {
                int value = data[i] + MIDDLE_VALUE;
                maxIndex = value < maxIndex ? maxIndex : value;
                minIndex = value > minIndex ? minIndex : value;
            }

            int bitSize = getBitSize(maxIndex - minIndex);

            /* 7 bytes
             * 2 for minIndex
             * 1 for bitSize
             * 3 for size 
             * 1 for rounding ((size * bitSize) >> 3)
             */
            int bytesSize = ((size * bitSize) >> 3) + 7;

            int delta = MIDDLE_VALUE - minIndex;
            BitOutputStream bitOutputStream = new BitOutputStream(bytesSize);
            bitOutputStream.WriteBits(16, minIndex);
            bitOutputStream.WriteBits(8, bitSize);
            bitOutputStream.WriteBits(24, size);
            for (int i = 0; i < size; i++) {
                bitOutputStream.WriteBits(bitSize, data[i] + delta);
            } 

            return bitOutputStream.GetData();
        }

        public static short[] GetCompressedShortsArray(byte[] data)
        {
            BitInputStream bitInputStream = new BitInputStream(data);
            int min = bitInputStream.ReadBits(16);
            int bitsSize = bitInputStream.ReadBits(8);
            int size = bitInputStream.ReadBits(24);

            short[] values = new short[size];
            int delta = min - MIDDLE_VALUE;
            for (int i = 0; i < values.Length; i++)
            {
                int read = bitInputStream.ReadBits(bitsSize);
                values[i] = (short)(delta + read);
            }

            return values;
        }
    }
}
