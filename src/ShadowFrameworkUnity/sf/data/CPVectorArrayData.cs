using UnityEngine;

namespace MLab.ShadowFramework.Data
{
    public enum CPVectorCompressionMode {
        LOW_UNIT_PRECISION,
        MEDIUM_UNIT_PRECISION,
        HIGH_UNIT_PRECISION
    }

    public class CPVectorArrayData {

        public const int LOW_UNIT_TICKS = 60;
        public const int MEDIUM_UNIT_TICKS = 240;
        public const int HIGH_UNIT_TICKS = 960;

        public const int LOW_UNIT_BITSIZE = 6;
        public const int MEDIUM_UNIT_BITSIZE = 8;
        public const int HIGH_UNIT_BITSIZE = 10;

        public static float getModeResize(CPVectorCompressionMode mode)
        {
            switch (mode) {
                case CPVectorCompressionMode.LOW_UNIT_PRECISION: return LOW_UNIT_TICKS * 0.5f;
                case CPVectorCompressionMode.MEDIUM_UNIT_PRECISION: return MEDIUM_UNIT_TICKS * 0.5f;
                case CPVectorCompressionMode.HIGH_UNIT_PRECISION: return HIGH_UNIT_TICKS * 0.5f; 
            }
            return 0;
        }

        public static int getModeBitsize(CPVectorCompressionMode mode)
        {
            switch (mode)
            {
                case CPVectorCompressionMode.LOW_UNIT_PRECISION: return LOW_UNIT_BITSIZE;
                case CPVectorCompressionMode.MEDIUM_UNIT_PRECISION: return MEDIUM_UNIT_BITSIZE;
                case CPVectorCompressionMode.HIGH_UNIT_PRECISION: return HIGH_UNIT_BITSIZE;
            }
            return 0;
        }

        public static int vectorFloatToInt(float value, float recPrecion, float precision) { 
            return (int)((value + 1) * recPrecion);
        }


        public static byte[] compressVectorArray(Vector3[] data, CPVectorCompressionMode compressionMode)
        {

            float recPrecision = getModeResize(compressionMode);
            float precision = 1.0f/recPrecision; 

            int bitSize = getModeBitsize(compressionMode);

            /* 7 bytes  
             * 3 for size 
             * 1 for rounding ((size*3 * bitSize) >> 3) */
            int size = data.Length;
            int bytesSize = ((size * 3 * bitSize) >> 3) + 4;

            BitOutputStream bitOutputStream = new BitOutputStream(bytesSize);  
            bitOutputStream.WriteBits(24, size);
            for (int i = 0; i < size; i++) {
                int value = vectorFloatToInt(data[i].x, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value);
                value = vectorFloatToInt(data[i].y, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value);
                value = vectorFloatToInt(data[i].z, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value);
            }

            return bitOutputStream.GetData();

        }

        public static Vector3[] getCompressedVectorArray(byte[] data, CPVectorCompressionMode compressionMode)
        {
            float recPrecision = getModeResize(compressionMode);
            float precision = 1.0f / recPrecision;
            BitInputStream bitInputStream = new BitInputStream(data);
            int bitsSize = getModeBitsize(compressionMode);
            
            int size = bitInputStream.ReadBits(24); 
            Vector3[] values = new Vector3[size];
            for (int i = 0; i < values.Length; i++)
            {
                int read = bitInputStream.ReadBits(bitsSize);
                values[i].x = (read) * precision - 1;
                read = bitInputStream.ReadBits(bitsSize);
                values[i].y = (read) * precision - 1;
                read = bitInputStream.ReadBits(bitsSize);
                values[i].z = (read) * precision - 1;
                values[i].Normalize();
            }

            return values;
        }
    }
}
