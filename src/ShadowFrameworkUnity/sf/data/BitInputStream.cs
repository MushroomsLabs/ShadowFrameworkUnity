
namespace MLab.ShadowFramework.Data
{
    public class BitInputStream
    {

        private byte[] data;

        private int position;

        public BitInputStream(byte[] data)
        {
            this.data = data;
        }

        private int GetByte(int index)
        {
            return data[index] < 0 ? data[index] + 256 : data[index];
        }


        public int GetPosition() {
            return position;
        }

        public int ReadBits(int count)
        {

            if (count > 8)
            {

                int value = 0;
                int index = 0;
                while (index + 8 < count)
                {
                    value = value << 8;
                    value += ReadBits(8);
                    index += 8;
                }
                value = value << (count - index);
                value += ReadBits(count - index);
                return value;
            }

            int first = position;
            int last = first + count - 1;
            position = first + count;

            int bId1 = first >> 3;
            int bId2 = last >> 3;

            first -= bId1 << 3;
            last -= bId2 << 3;

            if (bId1 == bId2)
            {

                int data = GetByte(bId1);

                data = (data << first) & 0xff;
                data = data >> (8 - (count));

                return data;
            }
            else if (bId2 == bId1 + 1)
            {

                int data1 = GetByte(bId1);
                int data2 = GetByte(bId2);

                data1 = (data1 << first) & 0xff;
                data1 = (data1 >> first);
                data1 = data1 << (last + 1);

                data2 = (data2 >> (7 - last));

                return data1 + data2;
            }

            return 0;
        }

    }


}