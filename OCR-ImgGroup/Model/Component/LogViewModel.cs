using OCR_ImgGroup.Service.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OCR_ImgGroup.Model.Component
{
    public class LogViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Logs原数据集合
        /// </summary>
        public ObservableCollection<LogEntry> Logs { get; } = new ObservableCollection<LogEntry>();
        /// <summary>
        /// 应用筛选器之后的Logs数据集合
        /// </summary>
        public ICollectionView FilteredLogs
        {
            get => _filteredLogs ??= InitializeFilteredLogs();
            private set => _filteredLogs = value;
        }
        /// <summary>
        /// 日志等级列表-下拉框数据源
        /// </summary>
        public List<EnumItem> LogLevels => typeof(LogLevel).GetEnumItems();
        /// <summary>
        /// 当前所选下拉框值
        /// </summary>
        public LogLevel SelectedLogLevel
        {
            get => _selectedLogLevel;
            set
            {
                _selectedLogLevel = value;
                OnPropertyChanged(nameof(SelectedLogLevel));
                UpdateFilteredLogs();
            }
        }

        private LogLevel _selectedLogLevel = LogLevel.ALL;

        private ICollectionView? _filteredLogs;

        private ICollectionView InitializeFilteredLogs()
        {
            var view = new CollectionViewSource { Source = Logs }.View;
            view.Filter = item => SelectedLogLevel == LogLevel.ALL || ((LogEntry)item).Level == SelectedLogLevel;
            return view;
        }

        private void UpdateFilteredLogs()
        {
            FilteredLogs.Filter = item => SelectedLogLevel == LogLevel.ALL || ((LogEntry)item).Level == SelectedLogLevel;
            FilteredLogs.Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
