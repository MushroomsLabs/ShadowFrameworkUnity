using UnityEngine;

namespace MLab.ShadowFramework.Data
{
    public enum CPVertexCompressionMode
    {
        MILLI,
        DECIMILLI,
        CENTI,
        DECI,
    }

    public class CPVertexArrayData {

        public static float GetPrecision(CPVertexCompressionMode mode) {
            switch (mode) {
                case CPVertexCompressionMode.DECI: return 0.1f;
                case CPVertexCompressionMode.CENTI: return 0.01f;
                case CPVertexCompressionMode.MILLI: return 0.001f;
                case CPVertexCompressionMode.DECIMILLI: return 0.0001f;
            }
            return 1;
        }

        public static byte[] CompressVertexArray(Vector3[] data, CPVertexCompressionMode compressionMode) {

            float precision = GetPrecision(compressionMode);
            float recPrecision = 1.0f / precision;
            //Debug.Log("Compressing Vertex Data precision:" + precision + " compressionMode:" + compressionMode);

            int size = data.Length;
            int maxIndexX = size > 0 ? CPFloatArrayData.floatToInt(data[0].x, recPrecision, precision) : 0;
            int minIndexX = maxIndexX;
            int maxIndexY = size > 0 ? CPFloatArrayData.floatToInt(data[0].y, recPrecision, precision) : 0;
            int minIndexY = maxIndexY;
            int maxIndexZ = size > 0 ? CPFloatArrayData.floatToInt(data[0].z, recPrecision, precision) : 0;
            int minIndexZ = maxIndexZ;
            for (int i = 0; i < size; i++)
            {
                int valueX = CPFloatArrayData.floatToInt(data[i].x, recPrecision, precision);
                maxIndexX = valueX < maxIndexX ? maxIndexX : valueX;
                minIndexX = valueX > minIndexX ? minIndexX : valueX;
                int valueY = CPFloatArrayData.floatToInt(data[i].y, recPrecision, precision);
                maxIndexY = valueY < maxIndexY ? maxIndexY : valueY;
                minIndexY = valueY > minIndexY ? minIndexY : valueY;
                int valueZ = CPFloatArrayData.floatToInt(data[i].z, recPrecision, precision);
                maxIndexZ = valueZ < maxIndexZ ? maxIndexZ : valueZ;
                minIndexZ = valueZ > minIndexZ ? minIndexZ : valueZ;
            }

            int bitSizeX = CPShortArrayData.getBitSize(maxIndexX - minIndexX);
            int bitSizeY = CPShortArrayData.getBitSize(maxIndexY - minIndexY);
            int bitSizeZ = CPShortArrayData.getBitSize(maxIndexZ - minIndexZ);

            int bitSize = bitSizeX > bitSizeY ? bitSizeX : bitSizeY;
            bitSize = bitSizeZ > bitSize ? bitSizeZ : bitSize;

            /* 7 bytes
             * 3 for each minIndex
             * 1 for bitSize
             * 3 for size 
             * 1 for rounding ((size*3 * bitSize) >> 3)
             */
            int bytesSize = ((size*3 * bitSize) >> 3) + 14;
             

            BitOutputStream bitOutputStream = new BitOutputStream(bytesSize);
            bitOutputStream.WriteBits(24, minIndexX);
            bitOutputStream.WriteBits(24, minIndexY);
            bitOutputStream.WriteBits(24, minIndexZ);
            bitOutputStream.WriteBits(8, bitSize);
            bitOutputStream.WriteBits(24, size);
            for (int i = 0; i < size; i++)
            {
                int value = CPFloatArrayData.floatToInt(data[i].x, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value - minIndexX);
                value = CPFloatArrayData.floatToInt(data[i].y, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value - minIndexY);
                value = CPFloatArrayData.floatToInt(data[i].z, recPrecision, precision);
                bitOutputStream.WriteBits(bitSize, value - minIndexZ);
            }

            return bitOutputStream.GetData();
            
        }

        public static Vector3[] GetCompressedVertexArray(byte[] data, CPVertexCompressionMode compressionMode) {

            float precision = GetPrecision(compressionMode);
            //Debug.Log("Decompress Vertex Data precision:" + precision + " compressionMode:" + compressionMode);
            BitInputStream bitInputStream = new BitInputStream(data);
            int minX = bitInputStream.ReadBits(24) - CPFloatArrayData.MIDDLE_VALUE;
            int minY = bitInputStream.ReadBits(24) - CPFloatArrayData.MIDDLE_VALUE;
            int minZ = bitInputStream.ReadBits(24) - CPFloatArrayData.MIDDLE_VALUE;

            int bitsSize = bitInputStream.ReadBits(8);
            int size = bitInputStream.ReadBits(24);

            Vector3[] values = new Vector3[size];
            for (int i = 0; i < values.Length; i++){
                int read = bitInputStream.ReadBits(bitsSize);
                values[i].x = (minX + read) * precision;
                read = bitInputStream.ReadBits(bitsSize);
                values[i].y = (minY + read) * precision;
                read = bitInputStream.ReadBits(bitsSize);
                values[i].z = (minZ + read) * precision;
            }

            return values;
        }
    }
}
