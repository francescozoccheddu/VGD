namespace Wheeled.Networking
{

    internal static class CRC16
    {

        private const ushort c_poly = 0xA001;
        private static readonly ushort[] s_table = new ushort[256];

        public static ushort Compute(byte[] _bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < _bytes.Length; ++i)
            {
                byte index = (byte) (crc ^ _bytes[i]);
                crc = (ushort) ((crc >> 8) ^ s_table[index]);
            }
            return crc;
        }

        static CRC16()
        {
            ushort value;
            ushort temp;
            for (ushort i = 0; i < s_table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort) ((value >> 1) ^ c_poly);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                s_table[i] = value;
            }
        }

    }

}
