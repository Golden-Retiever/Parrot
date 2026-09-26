using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 字节数组 → 强类型值 的纯函数转换器（与协议无关，供各执行层复用）
    /// </summary>
    internal static class ByteConvert
    {
        /// <summary>字节数组 → 强类型值 的转换器字典</summary>
        private static readonly Dictionary<Type, Func<byte[], object>> _converters = new Dictionary<Type, Func<byte[], object>>
        {
            [typeof(bool)]   = b => b[0] != 0,
            [typeof(string)] = b => Encoding.UTF8.GetString(b),
            [typeof(short)]  = b => BitConverter.ToInt16(b, 0),
            [typeof(ushort)] = b => BitConverter.ToUInt16(b, 0),
            [typeof(int)]    = b => BitConverter.ToInt32(b, 0),
            [typeof(uint)]   = b => BitConverter.ToUInt32(b, 0),
            [typeof(long)]   = b => BitConverter.ToInt64(b, 0),
            [typeof(ulong)]  = b => BitConverter.ToUInt64(b, 0),
            [typeof(float)]  = b => BitConverter.ToSingle(b, 0),
            [typeof(double)] = b => BitConverter.ToDouble(b, 0),
        };

        /// <summary>
        /// 单值转换：valueBytes 恰好是一个值的字节（主机小端）
        /// </summary>
        public static object ToValue(byte[] valueBytes, Type type)
        {
            if (valueBytes == null || valueBytes.Length == 0) throw new Exception("字节数组为空");
            if (type == null) throw new Exception("类型为空");
            if (!_converters.TryGetValue(type, out var conv)) throw new Exception($"不支持的类型：{type.Name}");
            return conv(valueBytes);
        }

        /// <summary>
        /// 多值转换：bytes 是 count 个值连续排列的字节。count≤1 返回单值，否则返回 object[]
        /// </summary>
        public static object ToValues(byte[] bytes, Type type, int count)
        {
            if (count <= 1)
                return ToValue(bytes, type);

            // bool 特例：每字节一个值（Marshal.SizeOf(bool) 会返回 4，不能用）
            int perValue = type == typeof(bool) ? 1 : Marshal.SizeOf(type);
            var arr = new object[count];
            for (int i = 0; i < count; i++)
                arr[i] = ToValue(bytes.Skip(i * perValue).Take(perValue).ToArray(), type);
            return arr;
        }
    }
}
