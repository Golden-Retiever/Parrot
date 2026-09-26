
using Microsoft.EntityFrameworkCore;
using Platform.DataEntities;
using System.Configuration;
using System.IO;

namespace Platform.DBAccess
{
    public class AppContext : DbContext
    {
        /// <summary>
        /// 用户表
        /// </summary>
        public virtual DbSet<SysUserEntity> SysUsers { get; set; }

        /// <summary>
        /// 组件表
        /// </summary>
        public virtual DbSet<ComponentEntity> Components { get; set; }

        /// <summary>
        /// 设备表
        /// </summary>
        public virtual DbSet<DeviceEntity> Devices { get; set; }

        /// <summary>
        /// 属性表(基础表)
        /// </summary>
        public virtual DbSet<PropEntity> Properties { get; set; }

        /// <summary>
        /// 设备属性表
        /// </summary>
        public virtual DbSet<DevicePropEntity> DeviceProps { get; set; }

        /// <summary>
        /// 设备变量表
        /// </summary>
        public virtual DbSet<DeviceVarEntity> DeviceVars { get; set; }

        /// <summary>
        /// 手动控制表
        /// </summary>
        public virtual DbSet<ManualControlEntity> ManualControls { get; set; }

        /// <summary>
        /// 变量报警配置表
        /// </summary>
        public virtual DbSet<VarAlarmConfEntity> VarAlarmConfs { get; set; }

        /// <summary>
        /// 设备报警信息表
        /// </summary>
        public virtual DbSet<DeviceAlarmEntity> DeviceAlarms { get; set; }

        /// <summary>
        /// 监控数据表
        /// </summary>
        public virtual DbSet<MonitorRecordEntity> MonitorRecords { get; set; }

        /// <summary>
        /// 七日耗电量
        /// </summary>
        public virtual DbSet<SevenPowerEntity> SevenPowers { get; set; }

        /// <summary>
        /// 七日耗能-用气量
        /// </summary>
        public virtual DbSet<SevenAirEntity> SevenAirs { get; set; }

        /// <summary>
        /// 七日耗能-泄露量
        /// </summary>
        public virtual DbSet<SevenLeakEntity> SevenLeaks { get; set; }

        /// <summary>
        /// 设备提醒
        /// </summary>
        public virtual DbSet<DeviceWarningEntity> DeviceWarnings { get; set; }

        /// <summary>
        /// 用气量排行
        /// </summary>
        public virtual DbSet<AirRankingEntity> AirRankings { get; set; }

        /// 趋势
        /// </summary>
        public virtual DbSet<TrendEntity> Trends { get; set; }

        /// <summary>
        /// 纵轴
        /// </summary>
        public virtual DbSet<TrendAxisEntity> TrendAxises { get; set; }

        /// <summary>
        /// 预警信息
        /// </summary>
        public virtual DbSet<TrendSectionEntity> TrendSections { get; set; }

        /// <summary>
        /// 图序列
        /// </summary>
        public virtual DbSet<TrendSeriesEntity> TrendSerieses { get; set; }

        /// <summary>
        /// 监控配置表
        /// </summary>
        public virtual DbSet<MonitorSettingEntity> MonitorSettings { get; set; }

        /// <summary>
        /// 配置sqlite 连接
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Sqlite数据库的连接字符串
            string connStr = ConfigurationManager.ConnectionStrings["SqliteConnString"].ConnectionString;
            optionsBuilder.UseSqlite(connStr);
        }
    }

}
