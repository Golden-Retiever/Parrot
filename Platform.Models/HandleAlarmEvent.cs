using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 报警处理 发布订阅。
    /// </summary>
    public class HandleAlarmEvent:PubSubEvent<string>
    {
    }
}
