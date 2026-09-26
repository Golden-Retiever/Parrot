using Platform.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.DBAccess
{
    /// <summary>
    /// 数据访问接口
    /// </summary>
    public interface IDataAccess
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        SysUserEntity? Login(string account, string pwd);

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="userId">用户Id</param>
        ///  <param name="newPwd">新密码(经过了md5处理了的密文)</param>
        /// <returns>影响的行数</returns>
        int ResetPwd(int userId, string newPwd);

        /// <summary>
        /// 取出所有的组件
        /// </summary>
        /// <returns></returns>
        List<ComponentEntity> GetComponents();

        /// <summary>
        /// 获取所有设备
        /// </summary>
        /// <returns></returns>
        List<DeviceEntity> GetDevices();

        /// <summary>
        /// 保存设备
        /// </summary>
        /// <param name="deviceList">需要保存的设备集合</param>
        /// <param name="devicePropList">设备属性集合</param>
        /// <param name="deviceVarList">设备变量集合</param>
        /// <param name="manualControlList">手动控制集合</param>
        /// <param name="varAlarmConfList">变量报警配置集合</param>
        /// <returns>保存是否成功</returns>
        bool SaveDevice(List<DeviceEntity> deviceList,List<DevicePropEntity> devicePropList=null, List<DeviceVarEntity> deviceVarList = null, List<ManualControlEntity> manualControlList=null,List<VarAlarmConfEntity> varAlarmConfList=null);

        /// <summary>
        /// 获取所有属性
        /// </summary>
        /// <returns></returns>
        List<PropEntity> GetPropList();

        /// <summary>
        /// 获取所有设备属性
        /// </summary>
        /// <returns></returns>
        List<DevicePropEntity> GetDeviceProps();

        /// <summary>
        /// 获取所有设备变量
        /// </summary>
        /// <returns></returns>
        List<DeviceVarEntity> GetDeviceVarList();

        /// <summary>
        /// 获取所有手动控制
        /// </summary>
        /// <returns></returns>
        List<ManualControlEntity> GetManualControlList();

        /// <summary>
        /// 获取所有变量报警配置
        /// </summary>
        /// <returns></returns>
        List<VarAlarmConfEntity> GetVarAlarmConfList();

        /// <summary>
        /// 保存设备报警信息
        /// </summary>
        /// <param name="deviceAlarmEntity">设备报警信息</param>
        void SaveDeviceAlarm(DeviceAlarmEntity deviceAlarmEntity);

        /// <summary>
        /// 记录监控数据
        /// </summary>
        /// <param name="monitorRecordList">监控数据集合</param>
        void SaveMonitorRecords(List<MonitorRecordEntity> monitorRecordList);

        /// <summary>
        /// 获取七日耗电量
        /// </summary>
        /// <returns></returns>
        List<SevenPowerEntity> GetSevenPowerList();

        /// <summary>
        /// 获取七日耗能-用气量
        /// </summary>
        /// <returns></returns>
        List<SevenAirEntity> GetSevenAirList();

        /// <summary>
        /// 获取七日耗能-泄露量
        /// </summary>
        /// <returns></returns>
        List<SevenLeakEntity> GetSevenLeakList();

        /// <summary>
        /// 获取设备提醒
        /// </summary>
        /// <returns></returns>
        List<DeviceWarningEntity> GetDeviceWarnings();

        /// <summary>
        /// 获取用气量排行
        /// </summary>
        /// <returns></returns>
        List<AirRankingEntity> GetAirRankings();

        /// <summary>
        /// 保存趋势信息
        /// </summary>
        /// <param name="trendList">趋势集合</param>
        /// <param name="axisList">纵轴集合</param>
        /// <param name="sectionList">预警集合</param>
        /// <param name="seriesList">序列集合</param>
        void SaveTrend(List<TrendEntity> trendList, List<TrendAxisEntity>? axisList = null, List<TrendSectionEntity>? sectionList = null, List<TrendSeriesEntity>? seriesList = null);

        /// <summary>
        /// 获取所有趋势
        /// </summary>
        List<TrendEntity> GetTrends();

        /// <summary>
        /// 获取所有纵轴
        /// </summary>
        List<TrendAxisEntity> GetTrendAxises();

        /// <summary>
        /// 获取所有预警
        /// </summary>
        List<TrendSectionEntity> GetTrendSections();

        /// <summary>
        /// 获取图表序列
        /// </summary>
        List<TrendSeriesEntity> GetTrendSerieses();

        /// <summary>
        /// 根据关键词查询报警信息
        /// </summary>
        /// <param name="keyWord">关键词</param>
        /// <returns></returns>
        List<DeviceAlarmEntity> GetDeviceAlarmList(string keyWord);

        /// <summary>
        /// 处理报警
        /// </summary>
        /// <param name="alarNum">报警编号</param>
        /// <param name="solveTime">解决时间</param>
        void HandleAlarmState(string alarNum, DateTime solveTime);

        /// <summary>
        /// 获取所有监控数据
        /// </summary>
        /// <returns></returns>
        List<MonitorRecordEntity> GetMonitorRecords();

        /// <summary>
        /// 获取所有监控配置
        /// </summary>
        /// <returns></returns>
        List<MonitorSettingEntity> GetMonitorSettings();

        /// <summary>
        /// 保存监测配置
        /// </summary>
        /// <param name="monitorSettingList"></param>
        void SaveMonitorSets(List<MonitorSettingEntity> monitorSettingList);

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns></returns>
        List<SysUserEntity> GetSysUserList();

        /// <summary>
        /// 保存用户配置
        /// </summary>
        /// <param name="userList"></param>
        void SaveUserSets(List<SysUserEntity> userList);

        /// <summary>
        /// 删除已处理的警报
        /// </summary>
        /// <param name="state">1-已解决；0-未解决</param>
        /// <returns></returns>
        void DeleteDealAlarm(int state);

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        void AddUser(SysUserEntity user);
    }
}
