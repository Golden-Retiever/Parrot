using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Logger
{
    /// <summary>
    /// NLog日志服务
    /// </summary>
    /// <typeparam name="T">目的:记录是哪个class里的日志</typeparam>
    public class NLogLoggerService<T> : ILoggerService<T>
    {
        // NLog 原生日志器
        private readonly ILogger _logger;

        public NLogLoggerService()
        {
            //GetLogger--获取日志记录实例；typeof(T).FullName--获取当前泛型的完整命令空间   
            _logger = LogManager.GetLogger($"{typeof(T).FullName}");
        }

        /// <summary>
        /// 最详细的调试信息，用于开发阶段追踪代码执行流程（生产环境禁用）
        /// </summary>
        /// <param name="message"></param>
        public void Trace(string message)
        {
            _logger.Trace(message);

            //_logger.Trace(new Exception("异常"));

        }

        /// <summary>
        /// 调试信息，用于排查问题（生产环境通常禁用）
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message) => _logger.Debug(message);

        /// <summary>
        /// 普通运行信息，记录系统正常操作（生产环境常用）
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message) => _logger.Info(message);

        /// <summary>
        /// 警告信息，非致命问题（不影响系统运行，但需关注）
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message) => _logger.Warn(message);

        /// <summary>
        /// 错误信息，单个操作失败（但系统仍能运行）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex = null)
        {
            if (ex == null)
            {
                _logger.Error(message);
            }
            else
            {
                _logger.Error(ex, message);
            }
        }

        /// <summary>
        /// 致命错误，系统不可用（需立即处理）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Fatal(string message, Exception ex = null)
        {
            if (ex == null)
            {
                _logger.Fatal(message);
            }
            else
            {
                _logger.Fatal(ex, message);
            }
        }
    }
}
