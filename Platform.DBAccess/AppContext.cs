using Microsoft.EntityFrameworkCore;
using Platform.DataEntities;
using System.Configuration;
using System.IO;

namespace Platform.DBAccess
{
    /// <summary>
    /// 数据库入口
    /// </summary>
    public class AppContext : DbContext
    {
        private readonly string? _explicitDbPath;

        /// <summary>
        /// 
        /// </summary>
        public AppContext()
        {

        }

        /// <summary>
        /// 显式路径构造：仅供 DbUpgrader 建临时新库用
        /// </summary>
        /// <param name="dbPath">主库文件绝对路径</param>
        public AppContext(string dbPath)
        {
            _explicitDbPath = dbPath;
        }

        /// <summary>
        /// 用户表
        /// </summary>
        public virtual DbSet<SysUserEntity> SysUsers { get; set; }

        /// <summary>
        /// 组件表
        /// </summary>
        public virtual DbSet<ComponentEntity> Components { get; set; }

        /// <summary>
        /// 属性表(参数库)
        /// </summary>
        public virtual DbSet<PropEntity> Properties { get; set; }

        /// <summary>
        /// 管理员命令表
        /// </summary>
        public virtual DbSet<AdminCmdEntity> AdminCommands { get; set; }

        /// <summary>
        /// 设置表
        /// </summary>
        public virtual DbSet<SettingEntity> Settings { get; set; }

        /// <summary>
        /// 种子: 内置的数据
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //内置登录用户
            modelBuilder.Entity<SysUserEntity>().HasData(new SysUserEntity
            {
                UserId = 1,
                Account = "你好唐姆",
                Password = "e10adc3949ba59abbe56e057f20f883e",  // MD5("123456")
                Role = 2,                                       // 一级管理员
                RealName = "唐姆",
                Gender = "男",
                Department = "初始账号"
            });
            //内置管理员口令
            modelBuilder.Entity<AdminCmdEntity>().HasData(new AdminCmdEntity { Id = 1, CommandText = "我不是唐人儿" });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //显式路径(DbUpgrader建临时库)优先;否则读配置连接串
            if (_explicitDbPath != null)
            {
                optionsBuilder.UseSqlite("Data Source=" + _explicitDbPath);
                return;
            }

            // Sqlite数据库的连接字符串
            //连接串相对路径锚定 exe 目录:运行时与ef设计时不同设备,但落盘是同一个
            const string prefix = "Data Source=";
            string connStr = ConfigurationManager.ConnectionStrings["SqliteConnString"].ConnectionString;
            string path = connStr.Substring(prefix.Length).Trim();
            if (!Path.IsPathRooted(path))//相对路径
                path = Path.Combine(System.AppContext.BaseDirectory, path);
            optionsBuilder.UseSqlite(prefix + path);
        }
    }
}
