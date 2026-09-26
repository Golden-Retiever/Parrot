using Platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.ViewModels
{
    public class AlarmConfViewModel : BindableBase
    {
        //操作集合
        public List<AlarmConfOperatorModel> OperatorList { get; set; } = new List<AlarmConfOperatorModel>();
        public AlarmConfViewModel()
        {
            // 只处理基本的逻辑运算     扩展：组件逻辑处理   &&   ||    ()
            OperatorList.Add(new AlarmConfOperatorModel() { OperatorName = "大于", OperatorSymbol = ">" });
            OperatorList.Add(new AlarmConfOperatorModel() { OperatorName = "小于", OperatorSymbol = "<" });
            OperatorList.Add(new AlarmConfOperatorModel() { OperatorName = "等于", OperatorSymbol = "==" });
            OperatorList.Add(new AlarmConfOperatorModel() { OperatorName = "大于等于", OperatorSymbol = ">=" });
            OperatorList.Add(new AlarmConfOperatorModel() { OperatorName = "小于等于", OperatorSymbol = "<=" });
            OperatorList.Add(new AlarmConfOperatorModel() { OperatorName = "不等于", OperatorSymbol = "!=" });
        }
    }
}
