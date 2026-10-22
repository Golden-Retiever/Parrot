using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace Platform.DBAccess
{
    /// <summary>
    /// 设计时工厂：dotnet ef / Add-Migration 需要实例化 ScenarioContext，
    /// 但常规构造依赖运行时的 ScenarioManager.CurrentDbPath（设计时是 null），
    /// 工厂提供临时目录占位库，避免设计时崩溃。
    /// 迁移链已重置（2026-10），加迁移命令示例：
    /// dotnet ef migrations add 名字 --context ScenarioContext --project Platform.DBAccess --startup-project Platform --output-dir Migrations/Scenario
    /// </summary>
    public class ScenarioContextFactory : IDesignTimeDbContextFactory<ScenarioContext>
    {
        public ScenarioContext CreateDbContext(string[] args)
        {
            // 设计期只建模型不真正建库；指向临时目录避免污染工程目录
            string dbPath = Path.Combine(Path.GetTempPath(), "platform_scenario_design.db");
            return new ScenarioContext(dbPath);
        }
    }
}
