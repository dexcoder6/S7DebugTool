using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace S7DebugTool.Services
{
    /// <summary>
    /// S7数据类型转换辅助类
    /// </summary>
    public static class S7DataHelper
    {
        /// <summary>
        /// 从字节数组中读取位
        /// </summary>
        public static bool GetBitAt(byte[] buffer, int byteIndex, int bitIndex)
        {
            if (byteIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(byteIndex));
            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index must be between 0 and 7");
            
            return (buffer[byteIndex] & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 设置字节中的位
        /// </summary>
        public static void SetBitAt(ref byte value, int bitIndex, bool bit)
        {
            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index must be between 0 and 7");
            
            if (bit)
                value |= (byte)(1 << bitIndex);
            else
                value &= (byte)~(1 << bitIndex);
        }

        /// <summary>
        /// 从字节数组读取字节
        /// </summary>
        public static byte GetByteAt(byte[] buffer, int index)
        {
            if (index >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return buffer[index];
        }

        /// <summary>
        /// 从字节数组读取字（2字节）- Big Endian
        /// </summary>
        public static ushort GetWordAt(byte[] buffer, int index)
        {
            if (index + 1 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            return (ushort)((buffer[index] << 8) | buffer[index + 1]);
        }

        /// <summary>
        /// 从字节数组读取双字（4字节）- Big Endian
        /// </summary>
        public static uint GetDWordAt(byte[] buffer, int index)
        {
            if (index + 3 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            return (uint)((buffer[index] << 24) | 
                         (buffer[index + 1] << 16) | 
                         (buffer[index + 2] << 8) | 
                         buffer[index + 3]);
        }

        /// <summary>
        /// 从字节数组读取有符号整数（2字节）- Big Endian
        /// </summary>
        public static short GetIntAt(byte[] buffer, int index)
        {
            return (short)GetWordAt(buffer, index);
        }

        /// <summary>
        /// 从字节数组读取有符号双整数（4字节）- Big Endian
        /// </summary>
        public static int GetDIntAt(byte[] buffer, int index)
        {
            return (int)GetDWordAt(buffer, index);
        }

        /// <summary>
        /// 从字节数组读取浮点数（4字节）- Big Endian
        /// </summary>
        public static float GetRealAt(byte[] buffer, int index)
        {
            if (index + 3 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            // S7 uses Big Endian, .NET uses Little Endian
            byte[] temp = new byte[4];
            temp[0] = buffer[index + 3];
            temp[1] = buffer[index + 2];
            temp[2] = buffer[index + 1];
            temp[3] = buffer[index];
            
            return BitConverter.ToSingle(temp, 0);
        }

        /// <summary>
        /// 从字节数组读取双精度浮点数（8字节）- Big Endian
        /// </summary>
        public static double GetLRealAt(byte[] buffer, int index)
        {
            if (index + 7 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            // S7 uses Big Endian, .NET uses Little Endian
            byte[] temp = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                temp[i] = buffer[index + 7 - i];
            }
            
            return BitConverter.ToDouble(temp, 0);
        }

        /// <summary>
        /// 从字节数组读取S7字符串
        /// S7字符串格式：[最大长度][实际长度][字符数据...]
        /// </summary>
        public static string GetStringAt(byte[] buffer, int index)
        {
            if (index + 1 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            int maxLength = buffer[index];
            int actualLength = buffer[index + 1];
            
            if (actualLength > maxLength)
                actualLength = maxLength;
            
            if (index + 2 + actualLength > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            return Encoding.ASCII.GetString(buffer, index + 2, actualLength);
        }

        /// <summary>
        /// 将字节写入缓冲区
        /// </summary>
        public static void SetByteAt(byte[] buffer, int index, byte value)
        {
            if (index >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            buffer[index] = value;
        }

        /// <summary>
        /// 将字写入缓冲区（2字节）- Big Endian
        /// </summary>
        public static void SetWordAt(byte[] buffer, int index, ushort value)
        {
            if (index + 1 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            buffer[index] = (byte)(value >> 8);
            buffer[index + 1] = (byte)(value & 0xFF);
        }

        /// <summary>
        /// 将双字写入缓冲区（4字节）- Big Endian
        /// </summary>
        public static void SetDWordAt(byte[] buffer, int index, uint value)
        {
            if (index + 3 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            buffer[index] = (byte)(value >> 24);
            buffer[index + 1] = (byte)((value >> 16) & 0xFF);
            buffer[index + 2] = (byte)((value >> 8) & 0xFF);
            buffer[index + 3] = (byte)(value & 0xFF);
        }

        /// <summary>
        /// 将有符号整数写入缓冲区（2字节）- Big Endian
        /// </summary>
        public static void SetIntAt(byte[] buffer, int index, short value)
        {
            SetWordAt(buffer, index, (ushort)value);
        }

        /// <summary>
        /// 将有符号双整数写入缓冲区（4字节）- Big Endian
        /// </summary>
        public static void SetDIntAt(byte[] buffer, int index, int value)
        {
            SetDWordAt(buffer, index, (uint)value);
        }

        /// <summary>
        /// 将浮点数写入缓冲区（4字节）- Big Endian
        /// </summary>
        public static void SetRealAt(byte[] buffer, int index, float value)
        {
            if (index + 3 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            byte[] temp = BitConverter.GetBytes(value);
            
            // Convert from Little Endian to Big Endian
            buffer[index] = temp[3];
            buffer[index + 1] = temp[2];
            buffer[index + 2] = temp[1];
            buffer[index + 3] = temp[0];
        }

        /// <summary>
        /// 将双精度浮点数写入缓冲区（8字节）- Big Endian
        /// </summary>
        public static void SetLRealAt(byte[] buffer, int index, double value)
        {
            if (index + 7 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            byte[] temp = BitConverter.GetBytes(value);
            
            // Convert from Little Endian to Big Endian
            for (int i = 0; i < 8; i++)
            {
                buffer[index + i] = temp[7 - i];
            }
        }

        /// <summary>
        /// 将字符串写入缓冲区（S7格式）
        /// </summary>
        public static void SetStringAt(byte[] buffer, int index, int maxLength, string value)
        {
            if (index + 2 + maxLength > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            buffer[index] = (byte)maxLength;
            
            if (string.IsNullOrEmpty(value))
            {
                buffer[index + 1] = 0;
                return;
            }
            
            byte[] strBytes = Encoding.ASCII.GetBytes(value);
            int actualLength = Math.Min(strBytes.Length, maxLength);
            
            buffer[index + 1] = (byte)actualLength;
            Array.Copy(strBytes, 0, buffer, index + 2, actualLength);
            
            // Clear remaining bytes
            for (int i = actualLength; i < maxLength; i++)
            {
                buffer[index + 2 + i] = 0;
            }
        }

        /// <summary>
        /// 转换为S7日期时间（DATE_AND_TIME）
        /// </summary>
        public static DateTime GetDateTimeAt(byte[] buffer, int index)
        {
            if (index + 7 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            // S7 DATE_AND_TIME format (8 bytes):
            // Year (BCD): 90-99 = 1990-1999, 00-89 = 2000-2089
            // Month (BCD): 1-12
            // Day (BCD): 1-31
            // Hour (BCD): 0-23
            // Minute (BCD): 0-59
            // Second (BCD): 0-59
            // Milliseconds (2 BCD bytes): 000-999
            
            int year = BCDtoByte(buffer[index]);
            year = year < 90 ? 2000 + year : 1900 + year;
            int month = BCDtoByte(buffer[index + 1]);
            int day = BCDtoByte(buffer[index + 2]);
            int hour = BCDtoByte(buffer[index + 3]);
            int minute = BCDtoByte(buffer[index + 4]);
            int second = BCDtoByte(buffer[index + 5]);
            int millisecond = BCDtoByte(buffer[index + 6]) * 10 + BCDtoByte(buffer[index + 7]);
            
            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        /// <summary>
        /// BCD转字节
        /// </summary>
        private static byte BCDtoByte(byte bcd)
        {
            return (byte)(((bcd >> 4) * 10) + (bcd & 0x0F));
        }

        /// <summary>
        /// 字节转BCD
        /// </summary>
        private static byte ByteToBCD(byte value)
        {
            return (byte)(((value / 10) << 4) | (value % 10));
        }

        /// <summary>
        /// 设置S7日期时间
        /// </summary>
        public static void SetDateTimeAt(byte[] buffer, int index, DateTime value)
        {
            if (index + 7 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            int year = value.Year;
            year = year >= 2000 ? year - 2000 : year - 1900;
            
            buffer[index] = ByteToBCD((byte)year);
            buffer[index + 1] = ByteToBCD((byte)value.Month);
            buffer[index + 2] = ByteToBCD((byte)value.Day);
            buffer[index + 3] = ByteToBCD((byte)value.Hour);
            buffer[index + 4] = ByteToBCD((byte)value.Minute);
            buffer[index + 5] = ByteToBCD((byte)value.Second);
            buffer[index + 6] = ByteToBCD((byte)(value.Millisecond / 10));
            buffer[index + 7] = ByteToBCD((byte)(value.Millisecond % 10));
        }
    }
}