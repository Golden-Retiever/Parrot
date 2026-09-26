using Platform.DataEntities;
using Platform.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.ViewModels
{
    /// <summary>
    /// 监控数据操作
    /// </summary>
    public class MonitorRecordOperation
    {
        /// <summary>
        /// 存放需要写道db的监控数据
        /// </summary>
        private static List<MonitorRecordEntity> monitorRecordList = new List<MonitorRecordEntity>();

        private static object _lock = new object();

        /// <summary>
        /// 添加监控数据
        /// </summary>
        /// <param name="dataAccess">数据访问对象</param>
        /// <param name="monitorRecord">要写的监控数据。null表示的时，应用退出的时候</param>
        public static void SaveRecords(IDataAccess dataAccess, MonitorRecordEntity? monitorRecord)
        {
            lock (_lock)
            {
                if (monitorRecord != null)
                {
                    monitorRecordList.Add(monitorRecord);
                }
                //退出或达到200条都要记录下
                if (monitorRecord == null || monitorRecordList.Count >= 200)
                {
                    dataAccess.SaveMonitorRecords(monitorRecordList);
                    monitorRecordList.Clear();//清空已经写进去的数据
                }
            }
        }
    }
}
