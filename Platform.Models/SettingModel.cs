using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    public class SettingModel:BindableBase
    {
        /// <summary>
        /// 主题
        /// </summary>
        private string? _Theme;
        public string? Theme 
        { 
            get { return _Theme; } 
            set { SetProperty(ref _Theme, value); } 
        }

        /// <summary>
        /// 语言
        /// </summary>
        private string? _Language;
        public string? Language
        {
            get { return _Language; }
            set { SetProperty(ref _Language, value); }
        }

        /// <summary>
        /// 背景
        /// </summary>
        private string? _Background;
        public string? Background
        {
            get { return _Background; }
            set { SetProperty(ref _Background, value); }
        }
    }
}
