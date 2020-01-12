using UnityEngine;

namespace MLab.ShadowFramework.Data
{
    public enum CPUVCompressionMode {
        ONE_ON_10,
        ONE_ON_100,
        ONE_ON_1000,
        ONE_ON_10000,
        ONE_ON_128,
        ONE_ON_256,
        ONE_ON_512,
        ONE_ON_1024,
        ONE_ON_2048,
        ONE_ON_4096,
        ONE_ON_8192,
        ONE_ON_16384
    }

    public class CPUVArrayData {

        const float ONE_ON_128 = 1.0f / 128;
        const float ONE_ON_256 = 1.0f / 256;
        const float ONE_ON_512 = 1.0f / 512;
        const float ONE_ON_1024 = 1.0f / 1024;
        const float ONE_ON_2048 = 1.0f / 2048;
        const float ONE_ON_4096 = 1.0f / 4096;
        const float ONE_ON_8192 = 1.0f / 8192;
        const float ONE_ON_16384 = 1.0f / 16384;

        public static float getPrecision(CPUVCompressionMode mode)
        {
            switch (mode)
            {
                case CPUVCompressionMode.ONE_ON_10: return 0.1f;
                case CPUVCompressionMode.ONE_ON_100: return 0.01f;
                case CPUVCompressionMode.ONE_ON_1000: return 0.001f;
                case CPUVCompressionMode.ONE_ON_10000: return 0.0001f;
                case CPUVCompressionMode.ONE_ON_128: return ONE_ON_128;
                case CPUVCompressionMode.ONE_ON_256: return ONE_ON_256;
                case CPUVCompressionMode.ONE_ON_512: return ONE_ON_512;
                case CPUVCompressionMode.ONE_ON_1024: return ONE_ON_1024;
                case CPUVCompressionMode.ONE_ON_2048: return ONE_ON_2048;
                case CPUVCompressionMode.ONE_ON_4096: return ONE_ON_4096;
                case CPUVCompressionMode.ONE_ON_8192: return ONE_ON_8192;
                case CPUVCompressionMode.ONE_ON_16384: return ONE_ON_16384;
            }
            return 1;
        }

        public static byte[] compressUVArray(Vector3[] data, CPUVCompressionMode compressionMode)
        {

            float precision = getPrecision(compressionMode);
            float recPrecision = 1.0f / precision;

            //Debug.Log("Compressing UV Data precision:" + precision+ " compressionMode:"+ compressionMode);

            int size = data.Length;
            int maxIndexX = size > 0 ? CPFloatArrayData.floatToInt(data[0].x, recPrecision, precision) : 0;
            int minIndexX = maxIndexX;
            int maxIndexY = size > 0 ? CPFloatArrayData.floatToInt(data[0].y, recPrecision, precision) : 0;
            int minIndexY = maxIndexY; 
            for (int i = 0; i < size; i++)
            {
                int valueX = CPFloatArrayData.floatToInt(data[i].x, recPrecision, precision);
                maxIndexX = valueX < maxIndexX ? maxIndexX : valueX;
                minIndexX = valueX > minIndexX ? minIndexX : valueX;
                int valueY = CPFloatArrayData.floatToInt(data[i].y, recPrecision, precision);
                maxIndexY = valueY < maxIndexY ? maxIndexY : valueY;
                minIndexY = valueY > minIndexY ? minIndexY : valueY; 
            }

            int bitSizeX = CPShortArrayData.getBitSize(maxIndexX - minIndexX);
            int bitSizeY = CPShortArrayData.getBitSize(maxIndexY - minIndexY); 

            int bitSize = bitSizeX > bitSizeY ? bitSizeX : bitSizeY; 

            /* 7 bytes
             * 3 for each minIndex
             * 1 for bitSize
             * 3 for size 
             * 1 for rounding ((size*3 * bitSize) >> 3)
             */
            int bytesSize = ((size * 3 * bitSize) >> 3) + 11;

            BitOutputStream bitOutputStream = new BitOutputStream(bytesSize);
            bitOutputStream.WriteBits(24, minIndexX);
            bitOutputStream.WriteBits(24, minIndexY); 
            bitOutputStream.WriteBits(8, bitSize);
            bitOutputStream.WriteBits(24, size);
            for (int i = 0; i < size; i++)
            {
                int value = CPFloatArrayData.floatToInt(data[i].x, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value - minIndexX);
                value = CPFloatArrayData.floatToInt(data[i].y, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value - minIndexY);
            } 
            return bitOutputStream.GetData();

        }

        public static Vector3[] getCompressedUVArray(byte[] data, CPUVCompressionMode compressionMode)
        {

            float precision = getPrecision(compressionMode);

            //Debug.Log("Decompressing UV Data precision:" + precision + " compressionMode:" + compressionMode);

            BitInputStream bitInputStream = new BitInputStream(data);
            int minX = bitInputStream.ReadBits(24) - CPFloatArrayData.MIDDLE_VALUE;
            int minY = bitInputStream.ReadBits(24) - CPFloatArrayData.MIDDLE_VALUE; 

            int bitsSize = bitInputStream.ReadBits(8);
            int size = bitInputStream.ReadBits(24);

            Vector3[] values = new Vector3[size];
            for (int i = 0; i < values.Length; i++)
            {
                int read = bitInputStream.ReadBits(bitsSize);
                values[i].x = (minX + read) * precision;
                read = bitInputStream.ReadBits(bitsSize);
                values[i].y = (minY + read) * precision;
            } 
            return values;
        }
    }
}
