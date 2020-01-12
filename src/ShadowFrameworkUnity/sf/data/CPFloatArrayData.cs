
namespace MLab.ShadowFramework.Data
{
    public class CPFloatArrayData
    {
        public const int MIDDLE_VALUE = 2097152;

        public static int floatToInt(float value, float recPrecion,float precision) {
            if(value>=0)
                return (int)((value + precision * 0.5f) * recPrecion) + MIDDLE_VALUE;
            return (int)((value - precision * 0.5f) * recPrecion) + MIDDLE_VALUE;
        }

        public static byte[] compressFloatsArray(float[] data, float precision){

            float recPrecision = 1.0f / precision;

            int size = data.Length;
            int maxIndex = size > 0 ? floatToInt(data[0],recPrecision,precision) : 0;
            int minIndex = maxIndex;
            for (int i = 0; i < size; i++)
            {
                int value = floatToInt(data[i], recPrecision, precision);
                maxIndex = value < maxIndex ? maxIndex : value;
                minIndex = value > minIndex ? minIndex : value;
            }

            int bitSize = CPShortArrayData.getBitSize(maxIndex - minIndex);

            /* 7 bytes
             * 3 for minIndex
             * 1 for bitSize
             * 3 for size 
             * 1 for rounding ((size * bitSize) >> 3)
             */
            int bytesSize = ((size * bitSize) >> 3) + 8;

            BitOutputStream bitOutputStream = new BitOutputStream(bytesSize);
            bitOutputStream.WriteBits(24, minIndex);
            bitOutputStream.WriteBits(8, bitSize);
            bitOutputStream.WriteBits(24, size);
            for (int i = 0; i < size; i++)
            {
                int value = floatToInt(data[i], recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value - minIndex);
            }

            return bitOutputStream.GetData();
             
        }

        public static float[] getCompressedFloatsArray(byte[] data,float precision) {

            if (data == null || data.Length == 0)
                return new float[0];

            BitInputStream bitInputStream = new BitInputStream(data);
            int min = bitInputStream.ReadBits(24) - MIDDLE_VALUE;
            int bitsSize = bitInputStream.ReadBits(8);
            int size = bitInputStream.ReadBits(24);

            float[] values = new float[size];
            for (int i = 0; i < values.Length; i++)
            {
                int read = bitInputStream.ReadBits(bitsSize);
                values[i] = (min + read)*precision;
            }

            return values;
        }
    }
}
