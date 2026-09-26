using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Helper
{
    /// <summary>
    /// md5工具类
    /// </summary>
    public class Md5Hepler
    {
        /// <summary>
        /// 转换成md5
        /// </summary>
        /// <param name="input">明文</param>
        /// <returns>md5结果串</returns>
        public static string ComputeMD5Hash(string input)
        {
            // 使用MD5创建哈希对象
            using (MD5 md5 = MD5.Create())
            {
                // 将字符串转换为字节数组并计算哈希
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                // 将字节数组转换为十六进制字符串的表示形式，并以小写形式返回
                return BitConverter.ToString(data).Replace("-", "").ToLower();
            }
        }
    }
}
