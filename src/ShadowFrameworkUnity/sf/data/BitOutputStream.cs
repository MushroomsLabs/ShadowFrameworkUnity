 
namespace MLab.ShadowFramework.Data
{
    public class BitOutputStream
    {

        private int bytesSize = 0;
        private byte[] data;

        private int position;
        private int onWrite = 0;

        public BitOutputStream(int size)
        {
            data = new byte[size];
        }
        
        public void WriteBits(int count, int value)
        {

            int mask = (1 << count) - 1;
            value = value & mask;

            int first = position;
            position = first + count;

            onWrite = onWrite << count;
            onWrite += value;

            int bytesCount = position >> 3;

            int diff = position - (bytesCount << 3);

            while (bytesCount > bytesSize)
            {

                int delta = bytesCount - bytesSize - 1;

                int shift = diff + (delta << 3);
                int valueNew = onWrite >> (shift);
                onWrite -= (valueNew << (shift));

                data[bytesSize] = (byte)valueNew;
                bytesSize++;
            }

        }

        public byte[] GetData()
        {
            if (position < data.Length * 8)
            {
                WriteBits(data.Length * 8 - position, 0);
            }
            return data;
        }

        public int GetBytesSize()
        {
            return bytesSize;
        }
    }

}