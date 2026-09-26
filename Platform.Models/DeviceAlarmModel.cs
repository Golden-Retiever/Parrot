using Platform.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Models
{
    /// <summary>
    /// 报警model
    /// </summary>
    public class DeviceAlarmModel : BindableBase
    {
        IDataAccess _dataAccess;//db访问
        IEventAggregator _eventAggregator;

        public DeviceAlarmModel(IDataAccess dataAccess,IEventAggregator eventAggregator)
        {
            _dataAccess = dataAccess;
            _eventAggregator= eventAggregator;
        }

        /// <summary>
        /// 报警编号
        /// </summary>
        public string AlarmNum { get; set; }

        /// <summary>
        /// 显示在界面的序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 变量编号
        /// </summary>
        public string VarNum { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string VarName { get; set; }

        /// <summary>
        /// 报警值
        /// </summary>
        public string? AlarmValue { get; set; }

        private int _state;

        /// <summary>
        /// 状态(0:未处理  1-已处理)
        /// </summary>
        public int State
        {
            get { return _state; }
            set
            {
                SetProperty(ref _state, value);
                if (value == 0)
                {
                    StateName = "未处理";
                }
                else if (value == 1)
                {
                    StateName = "已处理";
                }
            }
        }

        private string _stateName;

        /// <summary>
        /// 状态名称
        /// </summary>
        public string StateName
        {
            get { return _stateName; }
            set
            {
                SetProperty(ref _stateName, value);
            }
        }

        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime RecordTime { get; set; }

        private DateTime? _solveTime;

        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? SolveTime
        {
            get { return _solveTime; }
            set { SetProperty(ref _solveTime, value); }
        }

        /// <summary>
        /// 报警内容
        /// </summary>
        public string AlarmContent { get; set; }

        /// <summary>
        /// 记录账号
        /// </summary>
        public string Account { get; set; }

        #region 报警处理
        public DelegateCommand HandleAlarmCommand => new DelegateCommand(DoHandleAlarm, DoCanHandleAlarm);//DoHandleAlarm如何执行,DoCanHandleAlarm能不能执行

        /// <summary>
        /// 执行处理
        /// </summary>
        private void DoHandleAlarm()
        {
            #region  1、修改db数据状态

            State = 1;//ui要显示已处理

            DateTime now = DateTime.Now;
            SolveTime = now;//处理时间 ui要显示处理时间

            _dataAccess.HandleAlarmState(AlarmNum, now);
            #endregion

            #region 2、通知监控模块的设备 发布设备编号
            _eventAggregator.GetEvent<HandleAlarmEvent>().Publish(DeviceNum);
            #endregion
        }

        /// <summary>
        /// 命令是否能执行
        /// </summary>
        /// <returns>true:可以执行  false:不能执行</returns>
        private bool DoCanHandleAlarm()
        {
            return State == 0;
        }
        #endregion
    }
}
