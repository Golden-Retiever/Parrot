using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Platform.DBAccess
{
    /// <summary>
    /// 数据库结构升级器（2026-10 迁移链重置手术配套）。
    ///
    /// 背景：旧库是断头迁移链时代建的（__EFMigrationsHistory 里是 longma*/9.4 等
    /// 旧 id），新链（InitGlobal/InitScenario）无法直接在其上 Migrate。
    ///
    /// 策略：临时新库按新链建好 → ATTACH 旧库按表搬运（公共列交集，逐表 COUNT
    /// 对账）→ 校验通过后换文件，旧文件留 .old 永久备份。
    /// 任何一步失败：删临时文件、旧库原封不动、异常上抛，下次启动重试。
    /// </summary>
    public static class DbUpgrader
    {
        #region 对外入口

        /// <summary>
        /// 主库升级：旧结构 → 新结构（5 张全局表搬运）。
        /// 启动时在 EnsureFirstRunSplit 之后、AppContext.Migrate 之前调用。
        /// </summary>
        public static void UpgradeMainDbIfNeeded()
        {
            string main = ScenarioManager.GetMainDbPath();
            if (!NeedsUpgrade(main, GetAppChainIds())) return;

            ScenarioManager.WalCheckpoint(main);//旧库 WAL 先合并,搬运读的是完整数据
            string temp = main + ".new";
            TryDelete(temp); TryDelete(temp + "-wal"); TryDelete(temp + "-shm");
            try
            {
                using (var ctx = new AppContext(temp)) ctx.Database.Migrate();
                TransportTables(temp, main, GetAppTableNames());
                ScenarioManager.WalCheckpoint(temp);
                ReplaceFile(main, temp);
            }
            catch
            {
                //旧库未动,清掉临时产物,下次启动重试
                TryDelete(temp); TryDelete(temp + "-wal"); TryDelete(temp + "-shm");
                throw;
            }
        }

        /// <summary>
        /// 升级 Scenarios 目录下全部场景库（含兜底库）。
        /// 启动时在 AppContext.Migrate 之后调用。
        /// </summary>
        public static void UpgradeAllScenarioDbs()
        {
            foreach (string db in Directory.GetFiles(ScenarioManager.ScenariosDir, "*.db"))
            {
                //*.db 通配不会匹配 .db.new / .db.old 后缀文件,天然排除临时/备份产物
                UpgradeScenarioDbIfNeeded(db);
            }
        }

        /// <summary>
        /// 单个场景库升级（导入场景后也调用）。返回是否发生了升级。
        /// </summary>
        public static bool UpgradeScenarioDbIfNeeded(string dbPath)
        {
            if (!NeedsUpgrade(dbPath, GetScenarioChainIds())) return false;

            ScenarioManager.WalCheckpoint(dbPath);
            string temp = dbPath + ".new";
            TryDelete(temp); TryDelete(temp + "-wal"); TryDelete(temp + "-shm");
            try
            {
                using (var ctx = new ScenarioContext(temp)) ctx.Database.Migrate();
                TransportTables(temp, dbPath, GetScenarioTableNames());
                ScenarioManager.WalCheckpoint(temp);
                ReplaceFile(dbPath, temp);
            }
            catch
            {
                TryDelete(temp); TryDelete(temp + "-wal"); TryDelete(temp + "-shm");
                throw;
            }
            return true;
        }

        /// <summary>
        /// 从旧库建一个新结构的场景库（首次拆分用：源=旧主库，含 17 张场景表数据；
        /// 源=新结构主库时搬运自动跳过全部表，等价建空库）。目标文件可以不存在。
        /// </summary>
        internal static void BuildNewScenarioDbFrom(string destPath, string sourceDbPath)
        {
            if (File.Exists(sourceDbPath)) ScenarioManager.WalCheckpoint(sourceDbPath);

            string temp = destPath + ".new";
            TryDelete(temp); TryDelete(temp + "-wal"); TryDelete(temp + "-shm");
            try
            {
                using (var ctx = new ScenarioContext(temp)) ctx.Database.Migrate();
                if (File.Exists(sourceDbPath)) TransportTables(temp, sourceDbPath, GetScenarioTableNames());
                ScenarioManager.WalCheckpoint(temp);
                ReplaceFile(destPath, temp);//dest 不存在时只做临时文件转正
            }
            catch
            {
                TryDelete(temp); TryDelete(temp + "-wal"); TryDelete(temp + "-shm");
                throw;
            }
        }

        #endregion

        #region 升级判定

        /// <summary>
        /// 判断库是否需要升级：库文件的 __EFMigrationsHistory 里出现
        /// 新链不认识的迁移 id（= 旧链库），或根本没有历史表（= 外来库），都需要。
        /// 文件不存在返回 false（全新库交给 Migrate 直接建）。
        /// </summary>
        private static bool NeedsUpgrade(string dbPath, IEnumerable<string> currentChainIds)
        {
            if (!File.Exists(dbPath)) return false;
            var known = new HashSet<string>(currentChainIds);
            try
            {
                using var conn = new SqliteConnection("Data Source=" + dbPath);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory'";
                if (Convert.ToInt64(cmd.ExecuteScalar()!) == 0) return true;//非 EF 库(手工复制/外部导入)
                cmd.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!known.Contains(reader.GetString(0))) return true;//旧链 id → 旧库
                }
                return false;
            }
            catch
            {
                return true;//读不了的库按需升级处理(搬运会给出更准确的错误)
            }
        }

        private static string[] GetAppChainIds()
        {
            using var ctx = new AppContext();
            return ctx.Database.GetMigrations().ToArray();
        }

        private static string[] GetScenarioChainIds()
        {
            using var ctx = new ScenarioContext();
            return ctx.Database.GetMigrations().ToArray();
        }

        private static string[] GetAppTableNames()
        {
            using var ctx = new AppContext();
            return ModelTables(ctx);
        }

        private static string[] GetScenarioTableNames()
        {
            using var ctx = new ScenarioContext();
            return ModelTables(ctx);
        }

        /// <summary>当前模型映射的表名（模型级查询,不打开数据库文件）</summary>
        private static string[] ModelTables(DbContext ctx)
        {
            return ctx.Model.GetEntityTypes()
                .Select(e => e.GetTableName())
                .Where(n => n != null)
                .Select(n => n!)
                .Distinct()
                .ToArray();
        }

        #endregion

        #region 按表搬运 + 对账

        /// <summary>
        /// 把 oldDbPath 的数据搬进 newDbPath（新库必须已按新链建好表）。
        /// ATTACH 旧库为 old 别名，逐表：清空新表 → 按公共列交集 INSERT SELECT →
        /// COUNT 对账。旧库缺表（如 Debug 库缺 AdminCommands）→ 跳过留空/留种子；
        /// 任何表对账不平抛异常（调用方回退，旧库不动）。
        /// </summary>
        private static void TransportTables(string newDbPath, string oldDbPath, IEnumerable<string> tables)
        {
            using var conn = new SqliteConnection("Data Source=" + newDbPath);
            conn.Open();

            using (var attach = conn.CreateCommand())
            {
                attach.CommandText = "ATTACH DATABASE @p AS old";
                attach.Parameters.AddWithValue("@p", oldDbPath);
                attach.ExecuteNonQuery();
            }
            try
            {
                foreach (string table in tables)
                {
                    if (table.Contains('"') || table.Contains('\'')) continue;//模型表名不可能,防御性跳过

                    var newCols = GetColumns(conn, "main", table);
                    if (newCols.Count == 0) continue;//新模型没有该表(理论不可达)
                    var oldCols = GetColumns(conn, "old", table);
                    if (oldCols.Count == 0) continue;//旧库没有该表 → 新表保持种子/空
                    var common = newCols.Intersect(oldCols).ToList();
                    if (common.Count == 0) continue;//无公共列,不搬

                    string colList = string.Join(", ", common.Select(c => "\"" + c + "\""));
                    string tableRef = "\"" + table + "\"";

                    //清掉新表已有行(种子等),以旧库数据为准
                    using (var del = conn.CreateCommand())
                    {
                        del.CommandText = $"DELETE FROM main.{tableRef}";
                        del.ExecuteNonQuery();
                    }
                    //按公共列搬运
                    using (var ins = conn.CreateCommand())
                    {
                        ins.CommandText = $"INSERT INTO main.{tableRef}({colList}) SELECT {colList} FROM old.{tableRef}";
                        ins.ExecuteNonQuery();
                    }
                    //COUNT 对账:不平 = 搬运残缺 → 抛异常整体回退
                    long nNew = Count(conn, "main", table);
                    long nOld = Count(conn, "old", table);
                    if (nNew != nOld)
                        throw new InvalidOperationException($"表 {table} 搬运对账不平: 新库 {nNew} 行 vs 旧库 {nOld} 行");
                }
            }
            finally
            {
                using var detach = conn.CreateCommand();
                detach.CommandText = "DETACH DATABASE old";
                detach.ExecuteNonQuery();
            }
        }

        /// <summary>PRAGMA table_info 取列名（schema 为主库 main 或别名 old）</summary>
        private static List<string> GetColumns(SqliteConnection conn, string schema, string table)
        {
            var cols = new List<string>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA {schema}.table_info('{table}')";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cols.Add(reader.GetString(reader.GetOrdinal("name")));
            }
            return cols;
        }

        private static long Count(SqliteConnection conn, string schema, string table)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {schema}.\"{table}\"";
            return Convert.ToInt64(cmd.ExecuteScalar()!);
        }

        #endregion

        #region 文件换位

        /// <summary>
        /// 新库转正：旧文件（如有）改名为 .old 永久备份 → 临时文件上位。
        /// 调用前必须已对两边做过 WAL checkpoint，保证文件自包含。
        /// </summary>
        private static void ReplaceFile(string realPath, string tempPath)
        {
            SqliteConnection.ClearAllPools();//连接池句柄会锁文件,必须先释放

            if (File.Exists(realPath))
            {
                string backup = realPath + ".old";
                TryDelete(backup);//只保留最近一次升级的备份
                File.Move(realPath, backup);
            }
            //旧位置的 wal/shm 残留(已 checkpoint,内容为空)清掉,避免错挂到新库上
            TryDelete(realPath + "-wal");
            TryDelete(realPath + "-shm");

            File.Move(tempPath, realPath);
            TryDelete(tempPath + "-wal");
            TryDelete(tempPath + "-shm");
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        #endregion
    }
}
