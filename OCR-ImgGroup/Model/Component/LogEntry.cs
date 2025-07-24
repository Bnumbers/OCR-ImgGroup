using OCR_ImgGroup.Service.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_ImgGroup.Model.Component
{
    public class LogEntry
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel Level { get; set; }
        /// <summary>
        /// 日志等级名称
        /// </summary>
        public string LevelName => Level.GetDescription();
    }
    public enum LogLevel
    {
        [Description("全部")]
        ALL,
        [Description("调试")]
        Debug,
        [Description("信息")]
        Info,
        [Description("成功")]
        Success,
        [Description("警告")]
        Warning,
        [Description("错误")]
        Error
    }
    public class EnumItem
    {
        public Enum Value { get; set; }
        public string Description { get; set; }
    }
}
