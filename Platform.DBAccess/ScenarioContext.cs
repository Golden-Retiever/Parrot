using Microsoft.EntityFrameworkCore;
using Platform.DataEntities;

namespace Platform.DBAccess
{
    /// <summary>
    /// 场景库 Context：设备/变量/报警/监控记录等 17 张表，随场景 .db 文件走
    /// （Scenarios\*.db，一个场景一个文件，类比 VS 一个解决方案一个 .sln）
    /// </summary>
    /// <remarks>
    /// 【迁移链已重置（2026-10）】本链从 InitScenario 重新开始，
    /// 建库/升级一律走 Migrate()（ScenarioManager 统一调度）。
    /// 改表结构正常加迁移（设计时用 ScenarioContextFactory 兜底路径）：
    /// dotnet ef migrations add 名字 --context ScenarioContext --project Platform.DBAccess --startup-project Platform --output-dir Migrations/Scenario
    /// 旧结构场景库由 DbUpgrader 启动时自动按表搬运升级。
    /// </remarks>
    public class ScenarioContext : DbContext
    {
        private readonly string? _explicitDbPath;

        /// <summary>
        /// 常规构造：连接 ScenarioManager.CurrentDbPath（当前激活场景）
        /// </summary>
        public ScenarioContext()
        {
        }

        /// <summary>
        /// 显式路径构造：仅供 ScenarioManager 建新场景库 / 修复用，业务代码勿用
        /// </summary>
        /// <param name="dbPath">场景库文件绝对路径</param>
        public ScenarioContext(string dbPath)
        {
            _explicitDbPath = dbPath;
        }

        /// <summary>
        /// 设备表（位置参数全在这：X/Y/Z/宽高/流向/旋转 + 刷新率）
        /// </summary>
        public virtual DbSet<DeviceEntity> Devices { get; set; }

        /// <summary>
        /// 设备属性表（通信口等参数）
        /// </summary>
        public virtual DbSet<DevicePropEntity> DeviceProps { get; set; }

        /// <summary>
        /// 设备变量表（地址/类型/偏移/系数）
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
        /// 七日耗电量表
        /// </summary>
        public virtual DbSet<SevenPowerEntity> SevenPowers { get; set; }

        /// <summary>
        /// 七日能耗-用气量表
        /// </summary>
        public virtual DbSet<SevenAirEntity> SevenAirs { get; set; }

        /// <summary>
        /// 七日能耗-泄露量表
        /// </summary>
        public virtual DbSet<SevenLeakEntity> SevenLeaks { get; set; }

        /// <summary>
        /// 设备提醒表
        /// </summary>
        public virtual DbSet<DeviceWarningEntity> DeviceWarnings { get; set; }

        /// <summary>
        /// 用气排行表
        /// </summary>
        public virtual DbSet<AirRankingEntity> AirRankings { get; set; }

        /// <summary>
        /// 趋势表
        /// </summary>
        public virtual DbSet<TrendEntity> Trends { get; set; }

        /// <summary>
        /// 趋势纵轴表
        /// </summary>
        public virtual DbSet<TrendAxisEntity> TrendAxises { get; set; }

        /// <summary>
        /// 趋势预警表
        /// </summary>
        public virtual DbSet<TrendSectionEntity> TrendSections { get; set; }

        /// <summary>
        /// 趋势序列表
        /// </summary>
        public virtual DbSet<TrendSeriesEntity> TrendSerieses { get; set; }

        /// <summary>
        /// 监控配置表
        /// </summary>
        public virtual DbSet<MonitorSettingEntity> MonitorSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 无显式路径时用当前场景；无打开场景时兜底到隔离的空库文件，
            // 避免退出时的收尾写入(如 OnExit 落监控记录)因 null 路径而崩
            string dbPath = _explicitDbPath
                ?? ScenarioManager.CurrentDbPath
                ?? ScenarioManager.NoScenarioDbPath;

            optionsBuilder.UseSqlite("Data Source=" + dbPath);
        }
    }
}
