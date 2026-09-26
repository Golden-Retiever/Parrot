using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Logger
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">目的:记录是哪个class里的日志</typeparam>
    public interface ILoggerService<T>
    {
        //由小轻到大  由轻微到严重

        /// <summary>
        /// 最详细的调试信息，用于开发阶段追踪代码执行流程（生产环境禁用）
        /// </summary>
        /// <param name="message"></param>
        void Trace(string message);

        /// <summary>
        /// 调试信息，用于排查问题（生产环境通常禁用）
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);

        /// <summary>
        /// 普通运行信息，记录系统正常操作（生产环境常用）
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);

        /// <summary>
        /// 警告信息，非致命问题（不影响系统运行，但需关注）
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);

        /// <summary>
        /// 错误信息，单个操作失败（但系统仍能运行）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Error(string message, Exception ex = null);

        /// <summary>
        /// 致命错误，系统不可用（需立即处理）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Fatal(string message, Exception ex = null);

    }
}
