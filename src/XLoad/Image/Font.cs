namespace XLoad.Image
{
    public static class Font
    {
        static uint[] Data = new uint[]
        {
            0b01101001100110010110,
            0b11110100010001010110,
            0b11110010010010010110,
            0b01101001010010010110,
            0b10001000111110011001,
            0b01111000011100011111,
            0b01101001011100011110,
            0b00010010010010001111,
            0b01101001011010010110,
            0b01111000111010010110
        };

        static uint[] DataI = new uint[]
        {
            0b01101001100110010110,
            0b01100101010001001111,
            0b01101001010000101111,
            0b01101001010010010110,
            0b10011001111110001000,
            0b11110001011110000111,
            0b11100001011110010110,
            0b11111000010000100001,
            0b01101001011010010110,
            0b01101001111010000111
        };


        public static void Get4x5PointArray(char number, ref byte[] values, bool inverted)
        {
            if (values.Length != 20)
            {
                return;
            }

            int index = number - '0';

            for (int i = 0; i < 20; i++)
            {
                values[i] = (byte)((inverted ? DataI[index] : Data[index]) >> i & 1);
            }
        }
    }
}
