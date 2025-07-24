using OCR_ImgGroup.Model.Component;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace OCR_ImgGroup.Component
{
    /// <summary>
    /// LogControl.xaml 的交互逻辑
    /// </summary>
    public partial class LogControl : UserControl
    {
        public LogViewModel ViewModel { get; } = new LogViewModel();
        private readonly static object _lock = new();
        private const int MaxLines = 5000;
        public LogControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => ViewModel.Logs.Clear());
        }

        private void LogListBoxCopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (LogListBox.SelectedItem == null) return;
            var item = LogListBox.SelectedItem as LogEntry;
            try
            {
                Clipboard.SetText(item!.Message);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                // 处理异常，例如：剪贴板被其他进程占用
                MessageBox.Show("复制失败，请重试。" + ex.Message);
            }
        }

        #region 基础日志方法
        private void LogBase(string message, LogLevel level)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.Logs.Add(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Level = level
                });

                // 限制日志数量
                if (ViewModel.Logs.Count > MaxLines)
                {
                    ViewModel.Logs.RemoveAt(0);
                }
                if (LogListBox.Items.Count > 0) LogListBox.ScrollIntoView(LogListBox.Items[^1]);
            }, DispatcherPriority.Background);
        }

        private async Task LogBaseAsync(string message, LogLevel level)
        {
            // 异步更新UI
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                lock (_lock)
                {
                    ViewModel.Logs.Add(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Message = message,
                        Level = level
                    });

                    // 限制日志数量
                    if (ViewModel.Logs.Count > MaxLines)
                    {
                        ViewModel.Logs.RemoveAt(0);
                    }
                    if (LogListBox.Items.Count > 0) LogListBox.ScrollIntoView(LogListBox.Items[^1]);
                }
            }, DispatcherPriority.Background);
        }
        #endregion 基础日志方法

        #region 快速调用日志方法
        public void Debug(string msg) => LogBase(msg, LogLevel.Debug);
        public void Info(string msg) => LogBase(msg, LogLevel.Info);
        public void Success(string msg) => LogBase(msg, LogLevel.Success);
        public void Warning(string msg) => LogBase(msg, LogLevel.Warning);
        public void Error(string msg) => LogBase(msg, LogLevel.Error);

        public async Task DebugAsync(string msg) => await LogBaseAsync(msg, LogLevel.Debug);
        public async Task InfoAsync(string msg) => await LogBaseAsync(msg, LogLevel.Info);
        public async Task SuccessAsync(string msg) => await LogBaseAsync(msg, LogLevel.Success);
        public async Task WarningAsync(string msg) => await LogBaseAsync(msg, LogLevel.Warning);
        public async Task ErrorAsync(string msg) => await LogBaseAsync(msg, LogLevel.Error);
        #endregion 快速调用日志方法
    }

    // 日志级别到样式的转换器
    public class LevelToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (LogLevel)value switch
            {
                LogLevel.Debug => Brushes.Gray,
                LogLevel.Info => Brushes.DeepSkyBlue,
                LogLevel.Warning => Brushes.Orange,
                LogLevel.Error => Brushes.Red,
                LogLevel.Success => Brushes.Green,
                _ => Brushes.White
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    // 日志级别到图标的转换器
    public class LevelToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (LogLevel)value switch
            {
                LogLevel.Debug => "\xEBE8",
                LogLevel.Info => "\xE90A",
                LogLevel.Warning => "\xE814",
                LogLevel.Error => "\xEB90",
                LogLevel.Success => "\xEC61",
                _ => "\xE71D"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
