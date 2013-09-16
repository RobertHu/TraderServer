using System;
using System.Text;

namespace Trader.Server._4BitCompress
{
    public static class Quotation4BitEncoder
    {
        private static readonly byte[] _Char2High4Bits = { 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0 };
        private static readonly byte[] _Char2Low4Bits = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        private static readonly char[] _BitsToChar = { '-', '.', '/', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';' };

        public static byte[] Encode(string quotation)
        {
            byte[] buffer = new byte[(int)Math.Ceiling((double)quotation.Length / 2)];
            int indexInBuffer = 0;
            for (int index = 0; index < quotation.Length; index++)
            {
                int bitsIndex = quotation[index] - '-';
                byte bits = (index % 2 == 0) ? _Char2High4Bits[bitsIndex] : _Char2Low4Bits[bitsIndex];
                buffer[indexInBuffer] |= bits;
                indexInBuffer += index % 2;
            }
            return buffer;
        }

        public static string Decode(byte[] quotation)
        {
            var stringBuilder = new StringBuilder(quotation.Length * 2);
            foreach (byte data in quotation)
            {
                byte value = (byte)((data & 0XF0) / 16);
                stringBuilder.Append(_BitsToChar[value - 1]);
                value = (byte)(data & 0X0F);
                if (value != 0) stringBuilder.Append(_BitsToChar[value - 1]);
            }
            return stringBuilder.ToString();
        }
    }
}