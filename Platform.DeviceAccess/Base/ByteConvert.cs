using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Platform.DeviceAccess.Base
{
    /// <summary>
    /// 转换类型
    /// </summary>
    public static class ByteConvert
    {
        #region 读方向
        /// <summary>
        /// 字节数组 → 强类型值 的转换器字典
        /// </summary>
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
        #endregion 

        #region 写方向：值字符串 → 字节（与 ToValues 互为镜像）
        /// <summary>
        /// 类型名字符串 → Type("Boolean"/"Int16"/"UInt16"/"Int32"/"UInt32"/"Int64"/"UInt64"/"Single"/"Double"/"String")；查不到抛异常
        /// </summary>
        public static Type ToType(string name)
        {
            switch ((name ?? "").Trim())
            {
                case "Boolean": return typeof(bool);
                case "Int16":   return typeof(short);
                case "UInt16":  return typeof(ushort);
                case "Int32":   return typeof(int);
                case "UInt32":  return typeof(uint);
                case "Int64":   return typeof(long);
                case "UInt64":  return typeof(ulong);
                case "Single":  return typeof(float);
                case "Double":  return typeof(double);
                case "String":  return typeof(string);
                default: throw new Exception($"不支持的类型：{name}");
            }
        }

        /// <summary>
        /// 值字符串数组 → 字节（主机小端，依次拼接）。bool 每值 1 字节("1"/"true"=1)，其余按 Marshal.SizeOf；
        /// 段数与 count 不符、解析失败、类型不支持 → 抛异常，由调用方报错
        /// </summary>
        public static byte[] ToBytes(string[] parts, Type type, int count)
        {
            if (parts == null || parts.Length == 0) throw new Exception("没有可写的值");
            if (count > 0 && count != parts.Length) throw new Exception($"值个数不符:填了{parts.Length}个,要{count}个");

            var bytes = new List<byte>();
            foreach (var p in parts)
                bytes.AddRange(ToBytesOne(p, type));
            return bytes.ToArray();
        }

        /// <summary>单值字符串 → 单值字节（主机小端）</summary>
        private static byte[] ToBytesOne(string s, Type type)
        {
            if (type == typeof(bool))   return new[] { (byte)(s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0) };
            if (type == typeof(short))  return BitConverter.GetBytes(Convert.ToInt16(s));
            if (type == typeof(ushort)) return BitConverter.GetBytes(Convert.ToUInt16(s));
            if (type == typeof(int))    return BitConverter.GetBytes(Convert.ToInt32(s));
            if (type == typeof(uint))   return BitConverter.GetBytes(Convert.ToUInt32(s));
            if (type == typeof(long))   return BitConverter.GetBytes(Convert.ToInt64(s));
            if (type == typeof(ulong))  return BitConverter.GetBytes(Convert.ToUInt64(s));
            if (type == typeof(float))  return BitConverter.GetBytes(Convert.ToSingle(s));
            if (type == typeof(double)) return BitConverter.GetBytes(Convert.ToDouble(s));
            if (type == typeof(string)) return Encoding.UTF8.GetBytes(s);
            throw new Exception($"不支持的类型：{type.Name}");
        }
        #endregion
    }
}
