using Prism.Mvvm;

namespace Platform.Models
{
    /// <summary>
    /// 场景标签页模型（顶栏标签条的一项）
    /// </summary>
    public class ScenarioTabModel : BindableBase
    {
        private string _displayName = "";

        /// <summary>
        /// 显示名（标签页上的名字，可右键改名）
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private bool _isCurrent;

        /// <summary>
        /// 是否当前激活标签
        /// </summary>
        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { SetProperty(ref _isCurrent, value); }
        }

        /// <summary>
        /// 场景文件名（含 .db，Scenarios 目录下）
        /// </summary>
        public string FileName { get; set; } = "";
    }

    /// <summary>
    /// 场景命名弹窗的数据载体（新建/重命名共用）：
    /// ViewModel 构造后经 ActionHelper 传入弹窗，弹窗确认后回写 ResultName
    /// </summary>
    public class ScenarioNamePayload
    {
        /// <summary>弹窗标题</summary>
        public string Title { get; set; } = "新建场景";

        /// <summary>预填的名字（重命名时传旧名）</summary>
        public string InitialName { get; set; } = "";

        /// <summary>确认后的结果名（取消时无效）</summary>
        public string ResultName { get; set; } = "";
    }
}
