using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Helper
{
    /// <summary>
    /// 一般用于弹窗  在窗体上进行操作，有时候需要知道操作结果
    /// </summary>
    public class ActionHelper
    {
        /// <summary>
        ///  字典
        /// </summary>
        private static Dictionary<string, Delegate> actionMap = new Dictionary<string, Delegate>();

        /// <summary>
        /// 注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="d"></param>
        public static void Register<T>(string key, Delegate d)
        {
            if (!actionMap.ContainsKey(key))
            {
                actionMap.Add(key, d);
            }
        }

        /// <summary>
        /// 取消注册
        /// </summary>
        /// <param name="key"></param>
        public static void Unregister(string key)
        {
            if (actionMap.ContainsKey(key))
            {
                actionMap.Remove(key);
            }
        }

        /// <summary>
        /// 执行，不关心窗体操作结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public static void Execute<T>(string key, T data)
        {
            if (actionMap.ContainsKey(key))
                actionMap[key].DynamicInvoke(data);
        }

        /// <summary>
        /// 执行，关心窗体操作结果 dialogresult
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data">委托方法执行的参数，可以传null</param>
        /// <returns></returns>
        public static bool ExecuteAndResult<T>(string key, T data)
        {
            if (actionMap.ContainsKey(key))
            {
                var action = (actionMap[key] as Func<T, bool>);
                if (action == null)
                {
                    return false;
                }

                return action.Invoke(data);
            }
            return false;
        }

        public static void Register(string v, object showRegisterWin)
        {
            throw new NotImplementedException();
        }
    }
}
