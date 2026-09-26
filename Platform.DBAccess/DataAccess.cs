using Microsoft.EntityFrameworkCore;
using Platform.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Platform.DBAccess
{
    /// <summary>
    /// 数据库访问操作
    /// </summary>
    public class DataAccess : IDataAccess
    {
        //取消共享 AppContext：DbContext 不允许多线程同时使用，
        //改为每个方法自建短生命周期 context，用完即释放

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="account">账号</param>
        /// <param name="pwd">密码(密文)</param>
        /// <returns></returns>
        public SysUserEntity? Login(string account, string pwd)
        {
            using var ctx = new AppContext();
            return ctx.SysUsers.FirstOrDefault(u => u.Account == account && u.Password == pwd);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="userId">用户Id</param>
        ///  <param name="newPwd">新密码(经过了md5处理了的密文)</param>
        /// <returns>影响的行数</returns>
        public int ResetPwd(int userId, string newPwd)
        {
            using var ctx = new AppContext();
            SysUserEntity sysUserEntity = ctx.SysUsers.FirstOrDefault(u => u.UserId == userId);
            if (sysUserEntity != null)
            {
                sysUserEntity.Password = newPwd;

                return ctx.SaveChanges();
            }
            return 0;
        }

        /// <summary>
        /// 取出所有的组件
        /// </summary>
        /// <returns></returns>
        public List<ComponentEntity> GetComponents()
        {
            using var ctx = new AppContext();
            return ctx.Components.ToList();
        }

        /// <summary>
        /// 获取所有设备
        /// </summary>
        /// <returns></returns>
        public List<DeviceEntity> GetDevices()
        {
            //每次都是新 context，不跟踪旧实体，不再需要 ChangeTracker.Clear()
            using var ctx = new AppContext();
            return ctx.Devices.ToList();
        }

        /// <summary>
        /// 保存设备
        /// </summary>
        /// <param name="deviceList">需要保存的设备集合</param>
        /// <param name="devicePropList">设备属性集合</param>
        /// <param name="deviceVarList">设备变量集合</param>
        /// <param name="manualControlList">手动控制集合</param>
        /// <param name="varAlarmConfList">变量报警配置集合</param>
        /// <returns>保存是否成功</returns>
        public bool SaveDevice(List<DeviceEntity> deviceList, List<DevicePropEntity> devicePropList = null, List<DeviceVarEntity> deviceVarList = null, List<ManualControlEntity> manualControlList = null, List<VarAlarmConfEntity> varAlarmConfList = null)
        {
            using var ctx = new AppContext();
            try
            {
                #region 保存设备基本信息
                //先删除数据库所有数据，然后再添加deviceList
                ctx.Devices.RemoveRange(ctx.Devices);//删除所有

                ctx.Devices.AddRange(deviceList);
                #endregion

                #region 保存设备属性
                if (devicePropList != null)//如果设备属性不是null
                {
                    //1、删除所有设备属性数据
                    ctx.DeviceProps.RemoveRange(ctx.DeviceProps);
                    //2、保存传过来的数据
                    ctx.DeviceProps.AddRange(devicePropList);
                }
                #endregion

                #region 保存设备变量
                if (deviceVarList != null)//如果设备变量不是null
                {
                    //1、删除所有设备变量数据
                    ctx.DeviceVars.RemoveRange(ctx.DeviceVars);
                    //2、保存传过来的数据
                    ctx.DeviceVars.AddRange(deviceVarList);
                }
                #endregion

                #region 保存手动空置
                if (manualControlList != null)
                {
                    //1、删除
                    ctx.ManualControls.RemoveRange(ctx.ManualControls);
                    //2、保存
                    ctx.ManualControls.AddRange(manualControlList);
                }
                #endregion

                #region 保存变量报警配置
                if (varAlarmConfList != null)
                {
                    //1、删除
                    ctx.VarAlarmConfs.RemoveRange(ctx.VarAlarmConfs);
                    //2、保存
                    ctx.VarAlarmConfs.AddRange(varAlarmConfList);
                }
                #endregion

                ctx.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取所有属性
        /// </summary>
        /// <returns></returns>
        public List<PropEntity> GetPropList()
        {
            using var ctx = new AppContext();
            return ctx.Properties.ToList();
        }

        /// <summary>
        /// 获取所有设备属性
        /// </summary>
        /// <returns></returns>
        public List<DevicePropEntity> GetDeviceProps()
        {
            using var ctx = new AppContext();
            return ctx.DeviceProps.ToList();
        }

        /// <summary>
        /// 获取所有设备变量
        /// </summary>
        /// <returns></returns>
        public List<DeviceVarEntity> GetDeviceVarList()
        {
            using var ctx = new AppContext();
            return ctx.DeviceVars.ToList();
        }

        /// <summary>
        /// 获取所有手动控制
        /// </summary>
        /// <returns></returns>
        public List<ManualControlEntity> GetManualControlList()
        {
            using var ctx = new AppContext();
            return ctx.ManualControls.ToList();
        }

        /// <summary>
        /// 获取所有变量报警配置
        /// </summary>
        /// <returns></returns>
        public List<VarAlarmConfEntity> GetVarAlarmConfList()
        {
            using var ctx = new AppContext();
            return ctx.VarAlarmConfs.ToList();
        }

        /// <summary>
        /// 保存设备报警信息
        /// </summary>
        /// <param name="deviceAlarmEntity">设备报警信息</param>
        public void SaveDeviceAlarm(DeviceAlarmEntity deviceAlarmEntity)
        {
            using var ctx = new AppContext();
            ctx.DeviceAlarms.Add(deviceAlarmEntity);

            ctx.SaveChanges();
        }

        /// <summary>
        /// 记录监控数据
        /// </summary>
        /// <param name="monitorRecordList">监控数据集合</param>
        public void SaveMonitorRecords(List<MonitorRecordEntity> monitorRecordList)
        {
            using var ctx = new AppContext();
            ctx.MonitorRecords.AddRange(monitorRecordList);

            ctx.SaveChanges();
        }

        /// <summary>
        /// 获取七日耗电量
        /// </summary>
        /// <returns></returns>
        public List<SevenPowerEntity> GetSevenPowerList()
        {
            using var ctx = new AppContext();
            return ctx.SevenPowers.ToList();
        }

        /// <summary>
        /// 获取七日耗能-用气量
        /// </summary>
        /// <returns></returns>
        public List<SevenAirEntity> GetSevenAirList()
        {
            using var ctx = new AppContext();
            return ctx.SevenAirs.ToList();
        }

        /// <summary>
        /// 获取七日耗能-泄露量
        /// </summary>
        /// <returns></returns>
        public List<SevenLeakEntity> GetSevenLeakList()
        {
            using var ctx = new AppContext();
            return ctx.SevenLeaks.ToList();
        }

        /// <summary>
        /// 获取设备提醒
        /// </summary>
        /// <returns></returns>
        public List<DeviceWarningEntity> GetDeviceWarnings()
        {
            using var ctx = new AppContext();
            return ctx.DeviceWarnings.ToList();
        }

        /// <summary>
        /// 获取用气量排行
        /// </summary>
        /// <returns></returns>
        public List<AirRankingEntity> GetAirRankings()
        {
            using var ctx = new AppContext();
            return ctx.AirRankings.ToList();
        }

        /// <summary>
        /// 保存趋势信息
        /// </summary>
        /// <param name="trendList">趋势集合</param>
        /// <param name="axisList">纵轴集合</param>
        /// <param name="sectionList">预警集合</param>
        /// <param name="seriesList">序列集合</param>
        public void SaveTrend(List<TrendEntity> trendList, List<TrendAxisEntity>? axisList = null, List<TrendSectionEntity>? sectionList = null, List<TrendSeriesEntity>? seriesList = null)
        {
            using var ctx = new AppContext();

            //保存趋势
            ctx.Trends.RemoveRange(ctx.Trends);
            ctx.Trends.AddRange(trendList);

            //保存纵轴信息
            if (axisList != null)
            {
                ctx.TrendAxises.RemoveRange(ctx.TrendAxises);
                ctx.TrendAxises.AddRange(axisList);
            }

            //保存预警线信息
            if (sectionList != null)
            {
                ctx.TrendSections.RemoveRange(ctx.TrendSections);
                ctx.TrendSections.AddRange(sectionList);
            }

            //保存图表序列
            if (seriesList != null)
            {
                ctx.TrendSerieses.RemoveRange(ctx.TrendSerieses);
                ctx.TrendSerieses.AddRange(seriesList);
            }

            ctx.SaveChanges();
        }

        /// <summary>
        /// 获取所有趋势
        /// </summary>
        public List<TrendEntity> GetTrends()
        {
            using var ctx = new AppContext();
            return ctx.Trends.ToList();
        }

        /// <summary>
        /// 获取所有纵轴
        /// </summary>
        public List<TrendAxisEntity> GetTrendAxises()
        {
            using var ctx = new AppContext();
            return ctx.TrendAxises.ToList();
        }

        /// <summary>
        /// 获取所有预警
        /// </summary>
        public List<TrendSectionEntity> GetTrendSections()
        {
            using var ctx = new AppContext();
            return ctx.TrendSections.ToList();
        }

        /// <summary>
        /// 获取图表序列
        /// </summary>
        public List<TrendSeriesEntity> GetTrendSerieses()
        {
            using var ctx = new AppContext();
            return ctx.TrendSerieses.ToList();
        }

        /// <summary>
        /// 根据关键词查询报警信息
        /// </summary>
        /// <param name="keyWord">关键词</param>
        /// <returns></returns>
        public List<DeviceAlarmEntity> GetDeviceAlarmList(string keyWord)
        {
            using var ctx = new AppContext();
            if (string.IsNullOrEmpty(keyWord))
            {
                return ctx.DeviceAlarms.OrderByDescending(a => a.RecordTime).ToList();//根据记录时间降序
            }
            else
            {
                return ctx.DeviceAlarms.Where(a => a.DeviceNum.Contains(keyWord) || a.DeviceName.Contains(keyWord) || a.VarNum.Contains(keyWord) || a.VarName.Contains(keyWord) || a.AlarmContent.Contains(keyWord)).OrderByDescending(a => a.RecordTime).ToList();
            }
        }

        /// <summary>
        /// 处理报警
        /// </summary>
        /// <param name="alarNum">报警编号</param>
        /// <param name="solveTime">解决时间</param>
        public void HandleAlarmState(string alarNum, DateTime solveTime)
        {
            using var ctx = new AppContext();
            var alarm = ctx.DeviceAlarms.FirstOrDefault(t => t.AlarmNum == alarNum);
            if (alarm != null)
            {
                alarm.State = 1;
                alarm.SolveTime = solveTime;
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// 获取所有监控数据
        /// </summary>
        /// <returns></returns>
        public List<MonitorRecordEntity> GetMonitorRecords()
        {
            using var ctx = new AppContext();
            return ctx.MonitorRecords.ToList();
        }

        /// <summary>
        /// 获取所有监控配置
        /// </summary>
        /// <returns></returns>
        public List<MonitorSettingEntity> GetMonitorSettings()
        {
            using var ctx = new AppContext();
            return ctx.MonitorSettings.ToList();
        }

        /// <summary>
        /// 保存监测配置
        /// </summary>
        /// <param name="monitorSettingList"></param>
        public void SaveMonitorSets(List<MonitorSettingEntity> monitorSettingList)
        {
            using var ctx = new AppContext();
            if (monitorSettingList != null)
            {
                ctx.MonitorSettings.RemoveRange(ctx.MonitorSettings);
                ctx.MonitorSettings.AddRange(monitorSettingList);
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns></returns>
        public List<SysUserEntity> GetSysUserList()
        {
            using var ctx = new AppContext();
            return ctx.SysUsers.ToList();
        }

        /// <summary>
        /// 保存用户配置
        /// </summary>
        /// <param name="userList"></param>
        public void SaveUserSets(List<SysUserEntity> userList)
        {
            using var ctx = new AppContext();
            if (userList != null)
            {
                ctx.SysUsers.RemoveRange(ctx.SysUsers);
                ctx.SysUsers.AddRange(userList);
                ctx.SaveChanges();
            }
        }

        public void DeleteDealAlarm(int state)
        {
            using var ctx = new AppContext();
            var alarm = ctx.DeviceAlarms.Where(a => a.State == state);
            int count = alarm.Count();
            if (count == 0) return;

            ctx.DeviceAlarms.RemoveRange(alarm);
            int temp = ctx.SaveChanges();
            if (temp != count) MessageBox.Show("删除已处理信息失败\n--App.xaml.cs");
        }

        /// <summary>
        /// 添加新用户
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(SysUserEntity user)
        {
            using var ctx = new AppContext();
            ctx.SysUsers.Add(user);
            ctx.SaveChanges();
        }
    }
}
