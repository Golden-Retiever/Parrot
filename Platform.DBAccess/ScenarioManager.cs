using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.IO;
using System.Text.Json;

namespace Platform.DBAccess
{
    /// <summary>
    /// 场景清单模型（scenarios.json 的一条）：显示名 ↔ 文件名
    /// </summary>
    public class ScenarioInfoModel
    {
        /// <summary>场景文件名（含 .db，不含目录）</summary>
        public string FileName { get; set; } = "";

        /// <summary>显示名（标签页上的名字，可与文件名不同）</summary>
        public string DisplayName { get; set; } = "";
    }

    /// <summary>
    /// 场景管理器：一个场景 = Scenarios 目录下一个 .db 文件（设备+子表+运行数据）。
    /// scenarios.json 记录：场景清单、已打开标签列表、当前标签、标签上限。
    /// 命名/标签状态在关标签页、退出程序时自动落盘（全程零确认弹窗）。
    /// </summary>
    public static class ScenarioManager
    {
        #region 场景状态（内存态 = json 的映像）

        private class ScenarioState
        {
            public List<ScenarioInfoModel> Scenarios { get; set; } = new();

            /// <summary>已打开的标签页（文件名，按打开顺序）</summary>
            public List<string> OpenTabs { get; set; } = new();

            /// <summary>当前激活标签（文件名），null = 没有打开的场景</summary>
            public string? CurrentTab { get; set; }

            /// <summary>标签页数量上限</summary>
            public int MaxTabs { get; set; } = 6;
        }

        private static readonly ScenarioState _state = new();

        private static bool _loaded = false;

        #endregion

        #region 路径

        /// <summary>场景库目录（exe 目录下 Scenarios\）</summary>
        public static string ScenariosDir => Path.Combine(System.AppContext.BaseDirectory, "Scenarios");

        /// <summary>清单文件路径</summary>
        private static string JsonPath => Path.Combine(ScenariosDir, "scenarios.json");

        /// <summary>当前场景库文件绝对路径（null = 没有打开的场景）</summary>
        public static string? CurrentDbPath { get; private set; }

        /// <summary>当前标签的文件名（null = 无）</summary>
        public static string? CurrentTabFileName => _state.CurrentTab;

        /// <summary>标签页数量上限（用户定案：6）</summary>
        public static int MaxTabCount => _state.MaxTabs;

        /// <summary>无场景打开时的兜底库（隔离文件，防止收尾写入崩）</summary>
        public static string NoScenarioDbPath => Path.Combine(ScenariosDir, "__no_scenario.db");

        #endregion

        #region 启动/退出

        /// <summary>
        /// 启动时调用（登录之前）：
        /// 1. 首次运行把主库整文件复制成"默认场景"（拆分动作，只发生一次）；
        /// 2. 读取 scenarios.json 恢复：场景清单、标签列表、当前标签。
        /// json 丢失时按目录里现存 .db 自愈重建清单。
        /// </summary>
        public static void EnsureFirstRunSplit()
        {
            Directory.CreateDirectory(ScenariosDir);

            if (File.Exists(JsonPath))
            {
                LoadState();
            }
            else
            {
                //---- 首次拆分 ----
                string defaultDb = Path.Combine(ScenariosDir, "默认场景.db");
                if (!File.Exists(defaultDb))
                {
                    string mainDb = GetMainDbPath();
                    if (File.Exists(mainDb))
                    {
                        //主库存在:从主库按表搬 17 张场景表建默认场景
                        //(旧结构主库 → 搬设备数据;新结构主库无场景表 → 等价建空库)
                        DbUpgrader.BuildNewScenarioDbFrom(defaultDb, mainDb);
                    }
                    else
                    {
                        //主库都不存在(全新机器):建空的默认场景
                        CreateScenarioDb(defaultDb);
                    }
                }

                // 清单 = 目录里现存的全部 .db（自愈：json 删了也能重建）
                foreach (string db in Directory.GetFiles(ScenariosDir, "*.db"))
                {
                    string fileName = Path.GetFileName(db);
                    if (fileName.Equals("__no_scenario.db", StringComparison.OrdinalIgnoreCase)) continue;
                    AddScenarioInfo(fileName, Path.GetFileNameWithoutExtension(fileName));
                }
                _state.OpenTabs = new List<string> { "默认场景.db" };
                _state.CurrentTab = "默认场景.db";
                SaveStateJson();
                _loaded = true;//首次拆分后即视为已加载，防二次加载覆盖内存态
            }

            // 校正：打开的标签里文件已不存在的剔除；当前标签无效则退到第一个
            _state.OpenTabs = _state.OpenTabs
                .Where(f => File.Exists(Path.Combine(ScenariosDir, f)))
                .ToList();
            if (_state.CurrentTab == null || !_state.OpenTabs.Contains(_state.CurrentTab))
            {
                _state.CurrentTab = _state.OpenTabs.FirstOrDefault();
            }

            CurrentDbPath = _state.CurrentTab == null
                ? null
                : Path.Combine(ScenariosDir, _state.CurrentTab);
        }

        /// <summary>
        /// 退出时调用：命名/标签列表/当前标签自动落盘（无弹窗）
        /// </summary>
        public static void SaveStateJson()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_state, options);
                File.WriteAllText(JsonPath, json);
            }
            catch
            {
                // 落盘失败不影响退出流程
            }
        }

        #endregion

        #region 标签操作（供 MainUCViewModel 调用）

        /// <summary>
        /// 已打开标签列表（文件名 + 显示名，按打开顺序）
        /// </summary>
        public static List<(string FileName, string DisplayName)> GetOpenTabs()
        {
            EnsureLoaded();
            var result = new List<(string, string)>();
            foreach (string fileName in _state.OpenTabs)
            {
                string display = FindInfo(fileName)?.DisplayName ?? Path.GetFileNameWithoutExtension(fileName);
                result.Add((fileName, display));
            }
            return result;
        }

        /// <summary>
        /// 切换当前标签（点标签页）：改变当前场景库路径。
        /// 停/启监控由 ViewModel 负责（先停再切后启）。
        /// </summary>
        public static void SetCurrentTab(string fileName)
        {
            if (!_state.OpenTabs.Contains(fileName)) return;
            _state.CurrentTab = fileName;
            CurrentDbPath = Path.Combine(ScenariosDir, fileName);
        }

        /// <summary>
        /// 关闭标签页（仅关视图不删文件）。关闭的是当前标签时，自动退到剩余第一个
        /// （没有剩余则无当前场景）。落盘由调用方统一触发。
        /// </summary>
        public static void RemoveOpenTab(string fileName)
        {
            _state.OpenTabs.Remove(fileName);
            if (_state.CurrentTab == fileName)
            {
                _state.CurrentTab = _state.OpenTabs.FirstOrDefault();
                CurrentDbPath = _state.CurrentTab == null
                    ? null
                    : Path.Combine(ScenariosDir, _state.CurrentTab);
            }
        }

        /// <summary>
        /// 新建场景：建库（Migrate，走新迁移链）+ 入清单 + 开标签 + 设为当前。
        /// 返回文件名；名字非法时抛异常。
        /// </summary>
        public static string NewScenario(string displayName)
        {
            EnsureLoaded();
            string fileName = MakeUniqueFileName(displayName);
            string dbPath = Path.Combine(ScenariosDir, fileName);
            CreateScenarioDb(dbPath);

            AddScenarioInfo(fileName, displayName.Trim());
            _state.OpenTabs.Add(fileName);
            SetCurrentTab(fileName);
            SaveStateJson();
            return fileName;
        }

        /// <summary>
        /// 导入场景：把外部 .db 复制进 Scenarios 目录后开为标签。
        /// 源文件名与现有冲突时自动加数字后缀。返回（最终文件名, 是否重命名过）。
        /// </summary>
        public static (string FileName, bool Renamed) ImportScenario(string srcPath)
        {
            EnsureLoaded();
            string fileName = Path.GetFileName(srcPath);
            bool renamed = false;
            if (File.Exists(Path.Combine(ScenariosDir, fileName)))
            {
                fileName = MakeUniqueFileName(Path.GetFileNameWithoutExtension(srcPath));
                renamed = true;
            }

            // 复制前 checkpoint 源库（WAL 里可能有未合并数据）
            WalCheckpoint(srcPath);
            string destPath = Path.Combine(ScenariosDir, fileName);
            File.Copy(srcPath, destPath, true);

            //导入的可能是旧结构/外部库:统一升级到当前结构再入清单
            DbUpgrader.UpgradeScenarioDbIfNeeded(destPath);

            AddScenarioInfo(fileName, Path.GetFileNameWithoutExtension(fileName));
            _state.OpenTabs.Add(fileName);
            SetCurrentTab(fileName);
            SaveStateJson();
            return (fileName, renamed);
        }

        /// <summary>
        /// 导出场景：把指定场景库复制到目标位置（复制前 checkpoint）
        /// </summary>
        public static void ExportScenario(string fileName, string destPath)
        {
            string srcPath = Path.Combine(ScenariosDir, fileName);
            WalCheckpoint(srcPath);
            File.Copy(srcPath, destPath, true);
        }

        /// <summary>
        /// 打开清单里已有的场景为标签（已打开则仅激活）。返回是否真的打开了新标签。
        /// </summary>
        public static bool OpenScenario(string fileName)
        {
            EnsureLoaded();
            if (!File.Exists(Path.Combine(ScenariosDir, fileName))) return false;

            bool newlyOpened = !_state.OpenTabs.Contains(fileName);
            if (newlyOpened)
            {
                _state.OpenTabs.Add(fileName);
            }
            SetCurrentTab(fileName);
            SaveStateJson();
            return newlyOpened;
        }

        /// <summary>
        /// 场景清单（全部场景：文件名 + 显示名），供"打开场景"列表显示
        /// </summary>
        public static List<(string FileName, string DisplayName)> GetScenarioList()
        {
            EnsureLoaded();
            return _state.Scenarios
                .Select(s => (s.FileName, s.DisplayName))
                .ToList();
        }

        /// <summary>
        /// 重命名（只改显示名，不动文件；改完由调用方落盘）
        /// </summary>
        public static void RenameScenario(string fileName, string newDisplayName)
        {
            var info = FindInfo(fileName);
            if (info != null)
            {
                info.DisplayName = newDisplayName.Trim();
            }
        }

        /// <summary>
        /// 删除场景：移出清单与标签列表（当前标签指针自动回退到剩余第一个或空），
        /// 释放 SQLite 连接池占用后把 .db/-wal/-shm 一起删。
        /// 返回主库文件是否删除成功（被占用时清单里已移除，文件留给调用方提示）。
        /// </summary>
        public static bool DeleteScenario(string fileName)
        {
            EnsureLoaded();
            RemoveOpenTab(fileName);//内部已处理"删的是当前标签"的指针回退
            var info = FindInfo(fileName);
            if (info != null) _state.Scenarios.Remove(info);
            SaveStateJson();

            //连接池里的空闲连接握着文件句柄，Windows 上不 ClearPool 删除必失败
            try { SqliteConnection.ClearAllPools(); } catch { }

            string dbPath = Path.Combine(ScenariosDir, fileName);
            foreach (string f in new[] { dbPath, dbPath + "-wal", dbPath + "-shm", dbPath + ".old", dbPath + ".new" })
            {
                try { if (File.Exists(f)) File.Delete(f); } catch { }
            }
            return !File.Exists(dbPath);
        }

        /// <summary>
        /// 按显示名取显示名查场景（检查新建/改名时是否重名）
        /// </summary>
        public static bool DisplayNameExists(string displayName)
        {
            EnsureLoaded();
            return _state.Scenarios.Any(s => s.DisplayName == displayName.Trim());
        }

        #endregion

        #region 私有实现

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            // 理论上 App 启动即 EnsureFirstRunSplit；这里兜底防漏调
            EnsureFirstRunSplit();
            _loaded = true;
        }

        private static void LoadState()
        {
            try
            {
                string json = File.ReadAllText(JsonPath);
                var state = JsonSerializer.Deserialize<ScenarioState>(json);
                if (state != null)
                {
                    _state.Scenarios = state.Scenarios ?? new List<ScenarioInfoModel>();
                    _state.OpenTabs = state.OpenTabs ?? new List<string>();
                    _state.CurrentTab = state.CurrentTab;
                    _state.MaxTabs = state.MaxTabs > 0 ? state.MaxTabs : 6;
                }
            }
            catch
            {
                // json 损坏：按不存在处理（首次拆分逻辑兜底重建）
            }
            _loaded = true;
        }

        private static void AddScenarioInfo(string fileName, string displayName)
        {
            if (FindInfo(fileName) == null)
            {
                _state.Scenarios.Add(new ScenarioInfoModel { FileName = fileName, DisplayName = displayName });
            }
        }

        private static ScenarioInfoModel? FindInfo(string fileName)
        {
            return _state.Scenarios.FirstOrDefault(s => s.FileName == fileName);
        }

        /// <summary>
        /// 显示名 → 合法且不重复的文件名：剔除非法字符；重名加 " 2"、" 3"…
        /// </summary>
        private static string MakeUniqueFileName(string displayName)
        {
            string name = new string(displayName
                .Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                .ToArray()).Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("场景名不能为空");

            string fileName = name + ".db";
            int suffix = 2;
            while (File.Exists(Path.Combine(ScenariosDir, fileName)))
            {
                fileName = $"{name} {suffix}.db";
                suffix++;
            }
            return fileName;
        }

        /// <summary>
        /// 按当前模型建场景库（17 张表）。迁移链已重置（2026-10），
        /// 统一走 Migrate：以后加场景表迁移，新建与旧场景库启动时都自动升级。
        /// </summary>
        private static void CreateScenarioDb(string dbPath)
        {
            using var ctx = new ScenarioContext(dbPath);
            ctx.Database.Migrate();
        }

        /// <summary>
        /// SQLite WAL checkpoint：把 -wal 文件里的数据合并进主文件并清空，
        /// 复制/搬运 .db 前必须做，否则读到的是缺数据的库
        /// </summary>
        internal static void WalCheckpoint(string dbPath)
        {
            using var conn = new SqliteConnection("Data Source=" + dbPath);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 主库路径（与 AppContext 同一逻辑：读配置连接串，相对路径锚定 exe 目录）
        /// </summary>
        internal static string GetMainDbPath()
        {
            const string prefix = "Data Source=";
            string connStr = ConfigurationManager.ConnectionStrings["SqliteConnString"].ConnectionString;
            string path = connStr.Substring(prefix.Length).Trim();
            if (!Path.IsPathRooted(path))
                path = Path.Combine(System.AppContext.BaseDirectory, path);
            return path;
        }

        #endregion
    }
}
