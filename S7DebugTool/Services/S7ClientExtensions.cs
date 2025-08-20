using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S7DebugTool.Services
{
    /// <summary>
    /// S7Client扩展方法 - 数据类型读写
    /// </summary>
    public static class S7ClientExtensions
    {
        #region 读取方法

        /// <summary>
        /// 读取单个位
        /// </summary>
        public static async Task<bool?> ReadBitAsync(this S7Client client, int dbNumber, int byteAddress, int bitAddress)
        {
            var data = await client.ReadDBAsync(dbNumber, byteAddress, 1);
            if (data != null && data.Length > 0)
            {
                return S7DataHelper.GetBitAt(data, 0, bitAddress);
            }
            return null;
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        public static async Task<byte?> ReadByteAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 1);
            if (data != null && data.Length >= 1)
            {
                return data[0];
            }
            return null;
        }

        /// <summary>
        /// 读取字（Word, 2字节）
        /// </summary>
        public static async Task<ushort?> ReadWordAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 2);
            if (data != null && data.Length >= 2)
            {
                return S7DataHelper.GetWordAt(data, 0);
            }
            return null;
        }

        /// <summary>
        /// 读取双字（DWord, 4字节）
        /// </summary>
        public static async Task<uint?> ReadDWordAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 4);
            if (data != null && data.Length >= 4)
            {
                return S7DataHelper.GetDWordAt(data, 0);
            }
            return null;
        }

        /// <summary>
        /// 读取有符号整数（Int, 2字节）
        /// </summary>
        public static async Task<short?> ReadIntAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 2);
            if (data != null && data.Length >= 2)
            {
                return S7DataHelper.GetIntAt(data, 0);
            }
            return null;
        }

        /// <summary>
        /// 读取有符号双整数（DInt, 4字节）
        /// </summary>
        public static async Task<int?> ReadDIntAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 4);
            if (data != null && data.Length >= 4)
            {
                return S7DataHelper.GetDIntAt(data, 0);
            }
            return null;
        }

        /// <summary>
        /// 读取浮点数（Real, 4字节）
        /// </summary>
        public static async Task<float?> ReadRealAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 4);
            if (data != null && data.Length >= 4)
            {
                return S7DataHelper.GetRealAt(data, 0);
            }
            return null;
        }

        /// <summary>
        /// 读取双精度浮点数（LReal, 8字节）
        /// </summary>
        public static async Task<double?> ReadLRealAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 8);
            if (data != null && data.Length >= 8)
            {
                return S7DataHelper.GetLRealAt(data, 0);
            }
            return null;
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        public static async Task<string?> ReadStringAsync(this S7Client client, int dbNumber, int address, int maxLength = 254)
        {
            // S7字符串需要读取 maxLength + 2 字节（最大长度 + 实际长度 + 数据）
            var data = await client.ReadDBAsync(dbNumber, address, maxLength + 2);
            if (data != null && data.Length >= 2)
            {
                return S7DataHelper.GetStringAt(data, 0);
            }
            return null;
        }

        /// <summary>
        /// 读取日期时间
        /// </summary>
        public static async Task<DateTime?> ReadDateTimeAsync(this S7Client client, int dbNumber, int address)
        {
            var data = await client.ReadDBAsync(dbNumber, address, 8);
            if (data != null && data.Length >= 8)
            {
                return S7DataHelper.GetDateTimeAt(data, 0);
            }
            return null;
        }

        #endregion

        #region 写入方法

        /// <summary>
        /// 写入单个位
        /// </summary>
        public static async Task<bool> WriteBitAsync(this S7Client client, int dbNumber, int byteAddress, int bitAddress, bool value)
        {
            // 先读取字节，修改位，再写回
            var data = await client.ReadDBAsync(dbNumber, byteAddress, 1);
            if (data != null && data.Length > 0)
            {
                byte byteValue = data[0];
                S7DataHelper.SetBitAt(ref byteValue, bitAddress, value);
                return await client.WriteDBAsync(dbNumber, byteAddress, new byte[] { byteValue });
            }
            return false;
        }

        /// <summary>
        /// 写入字节
        /// </summary>
        public static async Task<bool> WriteByteAsync(this S7Client client, int dbNumber, int address, byte value)
        {
            return await client.WriteDBAsync(dbNumber, address, new byte[] { value });
        }

        /// <summary>
        /// 写入字（Word, 2字节）
        /// </summary>
        public static async Task<bool> WriteWordAsync(this S7Client client, int dbNumber, int address, ushort value)
        {
            byte[] buffer = new byte[2];
            S7DataHelper.SetWordAt(buffer, 0, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 写入双字（DWord, 4字节）
        /// </summary>
        public static async Task<bool> WriteDWordAsync(this S7Client client, int dbNumber, int address, uint value)
        {
            byte[] buffer = new byte[4];
            S7DataHelper.SetDWordAt(buffer, 0, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 写入有符号整数（Int, 2字节）
        /// </summary>
        public static async Task<bool> WriteIntAsync(this S7Client client, int dbNumber, int address, short value)
        {
            byte[] buffer = new byte[2];
            S7DataHelper.SetIntAt(buffer, 0, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 写入有符号双整数（DInt, 4字节）
        /// </summary>
        public static async Task<bool> WriteDIntAsync(this S7Client client, int dbNumber, int address, int value)
        {
            byte[] buffer = new byte[4];
            S7DataHelper.SetDIntAt(buffer, 0, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 写入浮点数（Real, 4字节）
        /// </summary>
        public static async Task<bool> WriteRealAsync(this S7Client client, int dbNumber, int address, float value)
        {
            byte[] buffer = new byte[4];
            S7DataHelper.SetRealAt(buffer, 0, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 写入双精度浮点数（LReal, 8字节）
        /// </summary>
        public static async Task<bool> WriteLRealAsync(this S7Client client, int dbNumber, int address, double value)
        {
            byte[] buffer = new byte[8];
            S7DataHelper.SetLRealAt(buffer, 0, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        public static async Task<bool> WriteStringAsync(this S7Client client, int dbNumber, int address, string value, int maxLength = 254)
        {
            byte[] buffer = new byte[maxLength + 2];
            S7DataHelper.SetStringAt(buffer, 0, maxLength, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 写入日期时间
        /// </summary>
        public static async Task<bool> WriteDateTimeAsync(this S7Client client, int dbNumber, int address, DateTime value)
        {
            byte[] buffer = new byte[8];
            S7DataHelper.SetDateTimeAt(buffer, 0, value);
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        #endregion

        #region 批量读写方法

        /// <summary>
        /// 批量读取整数数组
        /// </summary>
        public static async Task<short[]?> ReadIntArrayAsync(this S7Client client, int dbNumber, int address, int count)
        {
            var data = await client.ReadDBAsync(dbNumber, address, count * 2);
            if (data != null && data.Length >= count * 2)
            {
                short[] result = new short[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = S7DataHelper.GetIntAt(data, i * 2);
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// 批量写入整数数组
        /// </summary>
        public static async Task<bool> WriteIntArrayAsync(this S7Client client, int dbNumber, int address, short[] values)
        {
            byte[] buffer = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                S7DataHelper.SetIntAt(buffer, i * 2, values[i]);
            }
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        /// <summary>
        /// 批量读取浮点数数组
        /// </summary>
        public static async Task<float[]?> ReadRealArrayAsync(this S7Client client, int dbNumber, int address, int count)
        {
            var data = await client.ReadDBAsync(dbNumber, address, count * 4);
            if (data != null && data.Length >= count * 4)
            {
                float[] result = new float[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = S7DataHelper.GetRealAt(data, i * 4);
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// 批量写入浮点数数组
        /// </summary>
        public static async Task<bool> WriteRealArrayAsync(this S7Client client, int dbNumber, int address, float[] values)
        {
            byte[] buffer = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                S7DataHelper.SetRealAt(buffer, i * 4, values[i]);
            }
            return await client.WriteDBAsync(dbNumber, address, buffer);
        }

        #endregion
    }
}