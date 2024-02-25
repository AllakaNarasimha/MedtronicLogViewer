
using BusinessLayer;
using BusinessLayer.Models;
using Microsoft.Win32;
using MVVMBase;
using ScaleIndicator;
using System.Windows.Input;
using static System.Reflection.Metadata.BlobBuilder;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows;

namespace ViewModels
{
    public class LogViewModel : BaseViewModel
    {
        public ICommand SearchLogCommand { get; }
        public ICommand AnalyzeLogCommand { get; }
        public ICommand ClearLogCommand { get; }
        public LogViewModel()
        {
            SearchLogCommand = CreateCommand(OnSearchLog, CanExecuteSearchLog);
            AnalyzeLogCommand = CreateCommand(OnAnalyzeLog, CanAnalyzeLog);
            ClearLogCommand = CreateCommand(OnClearLog, CanClearLog);
            UpdateScaleMethod = new Action(() =>
            {
                UpdateScale();
            });
        }
        private DeviceLog selectedLog;

        private int gridSelectedIndex;

        public int GridSelectedIndex
        {
            get { return gridSelectedIndex; }
            set
            {
                gridSelectedIndex = value;
                if (selectedTabIndex == 1 && !isTimelineClicked)
                {
                    TimeLineIndex = Math.Min((gridSelectedIndex / widthStep) - 1, 99);
                }
                OnPropertyChanged();
            }
        }
        private int widthStep = 0;
        private int onePercent = 0;
        private bool isTimelineClicked = false;

        public DeviceLog SelectedLog
        {
            get { return selectedLog; }
            set
            {
                if (selectedLog != value)
                {
                    selectedLog = value;
                    OnPropertyChanged();
                }
            }
        }


        private TimeLineControl timelineControl;

        public TimeLineControl TimelineControl
        {
            get { return timelineControl; }
            set
            {
                timelineControl = value;
                OnPropertyChanged();
            }
        }

        private void UpdateScale()
        {
            Console.WriteLine("update scale triggered");
            //this.TimelineControl.ResetScale();
            var eventList = EventLogCollection;
            if (SelectedTabIndex == 0)
            {
                //eventList = ErrorLogCollection;
            }
            if (eventList.Count <= 0)
            {                
                return;
            }
           
            onePercent = (int)(eventList.Count * 0.01);
            widthStep = onePercent <= 0 ? 1 : onePercent;            
            PercentText = $"One percent equals {widthStep} rows in total rows of {eventList.Count}";
            int i = 0;
            while (i < eventList.Count)
            {
                int j = i <= 0 ? 1 : i;
                var logIndex = onePercent <= 0 ? (i / widthStep) : j;
                var listIndex = onePercent <= 0 ? (i / widthStep) : j;

                if (eventList[logIndex] != null)
                {
                    bool isdev = (eventList[logIndex].GetType() == typeof(DeviceLog));
                    int rCount = onePercent <= 0 ? 1 : widthStep;
                    var event_item = onePercent <= 0 ? eventList[logIndex] : GetLogType(j, rCount, out listIndex);
                    int sPercent = onePercent <= 0 ? j : (i / widthStep);
                    UpdateColor(event_item, sPercent, listIndex, rCount);
                }
                int nextJ = (i + widthStep);
                if (onePercent > 0 && nextJ >= eventList.Count)
                {
                    break;
                }
                i += widthStep;
            }
            if (100.00 / eventList.Count > 0)
            {
                int error = 100 - (100 / eventList.Count) + 1;
                updateProgressColor(Colors.White, error, widthStep, 0);
            }
            this.TimelineControl.OnClickedColor += ProgressColors_OnClickedolor;
        }

        private void ProgressColors_OnClickedolor(object? sender, ColorEventArgs e)
        {
            isTimelineClicked = true;
            var item = EventLogCollection[e.Index.ListIndex];
            if (item != null)
            {
                TimeLineIndex = e.Index.Index;
                if (IsHigh || IsLow || IsMedium)
                {
                    int listIndex = (int)(TimeLineIndex * ErrorLogCollection.Count * 0.01);
                    SelectedLog = ErrorLogCollection[listIndex];
                    isTimelineClicked = false;
                    return;
                }
                else if (e.Index.ListIndex == 0 && e.Index.StartIndex == 0)
                {
                    int listIndex = (int)(TimeLineIndex * EventLogCollection.Count * 0.01);
                    SelectedLog = EventLogCollection[listIndex];
                    isTimelineClicked = false;
                    return;
                }
                SelectedLog = item;
            }
            isTimelineClicked = false;
        }

        private int getErrorId(DeviceLog event_item)
        {
            if (event_item?.Detail?.ToUpper()?.Contains("ERROR ") == true)
            {
                return ExtractErrorCode(event_item.Detail);
            }

            return -1;
        }
        int ExtractErrorCode(string input)
        {
            Regex regex = new Regex(@"Error\s+(\d+)", RegexOptions.IgnoreCase);
            Match match = regex.Match(input);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                return -1;
            }
        }

        private int GetErrorType(int errorIndex)
        {
            switch (errorIndex)
            {
                case 233:
                case 245:
                case 340:
                    return 3;// High

                case 234:
                case 235:
                    return 2; //Medium

                case -1:
                case 225:
                    return 1; //Light
               
            }
            return -1;
        }

        private void UpdateColor(DeviceLog event_item, int sPercent, int logIndex, int widthStep = 1)
        {
            if (event_item != null && event_item.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                int errorIndex = getErrorId(event_item);

                int errorType = GetErrorType(errorIndex);
                if (errorType > 0)
                {
                    Color color = errorType == 3 ? Colors.Red :
                        errorType == 2 ? Colors.OrangeRed : Colors.MediumVioletRed;
                    updateProgressColor(color, sPercent, widthStep, logIndex);
                }
            }
        }
        private DeviceLog GetLogType(int sindex, int rindex, out int logIndex)
        {
            logIndex = sindex;
            var nLogs = EventLogCollection.Select((log, index) => new { Log = log, Index = index });
            var clogs = nLogs.Skip(sindex).Take(rindex);

            string[] logLevels = { "ERROR" };

            foreach (var level in logLevels)
            {
                var logEntry = clogs.FirstOrDefault(x => x.Log.LogLevel.Equals(level, StringComparison.OrdinalIgnoreCase));

                if (logEntry != null)
                {
                    logIndex = logEntry.Index;
                    return logEntry.Log;
                }
            }
            return null;
        }


        /*private void UpdateColor_old(DeviceLog event_item, int sPercent, int logIndex, int widthStep = 1)
        {
            if (event_item.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                updateProgressColor(Colors.Red, sPercent, widthStep, logIndex);
            }
            else if (event_item.LogLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase))
            {
                updateProgressColor(Colors.MediumVioletRed, sPercent, widthStep, logIndex);
            }
            else if (event_item.LogLevel.Equals("INFO", StringComparison.OrdinalIgnoreCase))
            {
                updateProgressColor(Colors.LightPink, sPercent, widthStep, logIndex);
            }
            else
            {
                Console.WriteLine("Not matched with any list");
            }
        }*/


        private void updateProgressColor(Color color, int s_index, int r_count, int listIndex)
        {
            TimelineControl?.UpdateScaleColorAt(color, s_index, r_count, listIndex);
        }

        private DeviceLog GetLogType_old(int sindex, int rindex, out int logIndex)
        {
            logIndex = sindex;
            var nLogs = EventLogCollection.Select((log, index) => new { Log = log, Index = index });
            var clogs = nLogs.Skip(sindex).Take(rindex);
            if (clogs.FirstOrDefault(x => x.Log.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase)) != null)
            {
                var errorLog = clogs.FirstOrDefault(x => x.Log.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase));
                logIndex = errorLog.Index;
                return errorLog.Log;
            }
            else if (clogs.FirstOrDefault(x => x.Log.LogLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase)) != null)
            {
                var errorLog = clogs.FirstOrDefault(x => x.Log.LogLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase));
                logIndex = errorLog.Index;
                return errorLog.Log;
            }
            else if (clogs.FirstOrDefault(x => x.Log.LogLevel.Equals("INFO", StringComparison.OrdinalIgnoreCase)) != null)
            {
                var errorLog = clogs.FirstOrDefault(x => x.Log.LogLevel.Equals("INFO", StringComparison.OrdinalIgnoreCase));
                logIndex = errorLog.Index;
                return errorLog.Log;
            }
            return null;
        }

        private Action updateScaleMethod;
        public Action UpdateScaleMethod
        {
            get { return updateScaleMethod; }
            set
            {
                updateScaleMethod = value;
                OnPropertyChanged(nameof(UpdateScaleMethod));
            }
        }

        private RangeObservableCollection<DeviceLog> errorLogCollection ;
        private RangeObservableCollection<DeviceLog> eventLogCollection;

        private int selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return selectedTabIndex; }
            set { selectedTabIndex = value;
                //UpdateScaleMethod();
                OnPropertyChanged();
            }
        }


        public RangeObservableCollection<DeviceLog> ErrorLogCollection
        {
            get { return errorLogCollection; }
            set
            {
                errorLogCollection = value;                
                OnPropertyChanged();
            }
        }
        public RangeObservableCollection<DeviceLog> EventLogCollection
        {
            get { return eventLogCollection; }
            set { eventLogCollection = value;
                UpdateScaleMethod();
                OnPropertyChanged();
            }
        }


        private void OnClearLog(object obj)
        {
            IsHigh = false;
            IsMedium = false;
            IsLow = false;            
        }

        private bool CanClearLog(object arg)
        {
            return logPath?.Length > 0;
        }

        private bool CanAnalyzeLog(object arg)
        {
            return logPath?.Length > 0;
        }

        private void OnAnalyzeLog(object obj)
        {
            ShowProgress = true;

            Task.Delay(4000).ContinueWith(task =>
            {
                Application.Current.Dispatcher.BeginInvoke(delegate {
                    EventLogCollection = LogReader<DeviceLog>.ReadLogFile(logPath);

                    var errors = EventLogCollection.Where(x => x.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase)).ToList();
                    ErrorLogCollection = new RangeObservableCollection<DeviceLog>(errors);

                    if (ErrorLogCollection.Count > 0)
                    {
                        HighCount = ErrorLogCollection.Count(x => GetErrorType(getErrorId(x)) == 3);
                        MediumCount = ErrorLogCollection.Count(x => GetErrorType(getErrorId(x)) == 2);
                        LowCount = ErrorLogCollection.Count(x => GetErrorType(getErrorId(x)) == 1);
                    }

                    StartTime = EventLogCollection.FirstOrDefault() != null ? EventLogCollection.FirstOrDefault().DateTime.ToString() : ""; // "dd-mm-yy HH:MM:ss") : "";
                    EndTime = EventLogCollection.LastOrDefault() != null ? EventLogCollection.LastOrDefault().DateTime.ToString() : ""; // "dd-mm-yy HH:MM:ss") : "";

                    ShowProgress = false;
                });                
            });           
        }



        private bool CanExecuteSearchLog(object arg)
        {
            return IsEnableSearchLog;
        }

        private void OnSearchLog(object obj)
        {
            OpenFileDialog open = new OpenFileDialog();
            var result = open.ShowDialog();
            LogPath = open.FileName.ToString();
        }

        private string appName;
        private string logPath = "log_result.csv";
        private bool isEnableSearchLog;
		private bool ft10Checked;
		private bool fx8Checked;
		private bool isHighChecked;
        private bool isMediumChecked;
        private bool isLowChecked;
        private int highCount;
        private int mediumCount;
        private int lowCount;
        private string perCentText = "One percent equals 1 row";
        private string startTime = "3/15/2023 10:50";
        private string endTime = "3/15/2023 20:50";
        private int timeLineIndex = 0;
        private bool showProgress;

        public bool ShowProgress
        {
            get { return showProgress; }
            set { showProgress = value;
                OnPropertyChanged();
            }
        }

        public int TimeLineIndex
        {
            get { return timeLineIndex; }
            set { timeLineIndex = value;
                OnPropertyChanged();
            }
        }


        public string EndTime
        {
            get { return endTime; }
            set { endTime = value;
                OnPropertyChanged();
            }
        }


        public string StartTime
        {
            get { return startTime; }
            set { startTime = value;
                OnPropertyChanged();
            }
        }


        public string PercentText
        {
            get { return perCentText; }
            set { perCentText = value;
                OnPropertyChanged();
            }
        }



        public int LowCount
        {
            get { return lowCount; }
            set
            {
                lowCount = value;
                OnPropertyChanged();
            }
        }
        public int MediumCount
        {
            get { return mediumCount; }
            set
            {
                mediumCount = value;
                OnPropertyChanged();
            }
        }

        public int HighCount
        {
            get { return highCount; }
            set { highCount = value;
                OnPropertyChanged();
            }
        }


        public bool IsLow
        {
            get { return isLowChecked; }
            set
            {
                isLowChecked = value;
                UpdateErrorLogs();
                OnPropertyChanged();
            }
        }
        public bool IsMedium
        {
            get { return isMediumChecked; }
            set
            {
                isMediumChecked = value;
                UpdateErrorLogs();
                OnPropertyChanged();
            }
        }
        
        public bool IsHigh
		{
			get { return isHighChecked; }
			set
			{
				isHighChecked = value;
                UpdateErrorLogs();
                OnPropertyChanged();
			}
		}

        private void UpdateErrorLogs()
        {
            var errors = EventLogCollection.Where(x => x.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase)).ToList();
            if (errors.Count > 0  && (IsHigh || IsMedium || IsLow ))
            {
                var filteredLogs = errors.Where(x => IsHigh && GetErrorType(getErrorId(x)) == 3 ||
                                                     IsMedium && GetErrorType(getErrorId(x)) == 2 ||
                                                     IsLow && GetErrorType(getErrorId(x)) == 1).ToList();
                ErrorLogCollection = new RangeObservableCollection<DeviceLog>(filteredLogs);
            }
            else
            {
                errors = EventLogCollection.Where(x => x.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase)).ToList();
                ErrorLogCollection = new RangeObservableCollection<DeviceLog>(errors);
            }
        }

        private int logType;

		public int LogType
		{
			get { return logType; }
			set { logType = value;
				OnPropertyChanged(); }
		}
        public bool FX8Checked
		{
			get { return fx8Checked; }
			set { fx8Checked = value;
				if(fx8Checked )
				{
					AppName = "FX8";
					FT10Checked = false;
				}
				OnPropertyChanged();
			}
		}


		public bool FT10Checked
        {
			get { return ft10Checked; }
			set { ft10Checked = value;
				if(ft10Checked )
				{
					AppName = "FT10";
					FX8Checked = false;
				}
				OnPropertyChanged();
			}
		}


		public string AppName
        {
            get { return appName; }
            set { appName = value;
				if(appName.Length > 0)
				{
                    IsEnableSearchLog = true;
				}
				OnPropertyChanged();
			}
        }

        public bool IsEnableSearchLog
        {
			get { return isEnableSearchLog; }
			set { isEnableSearchLog = value;
				OnPropertyChanged();
			}
		}


		public string LogPath
		{
			get { return logPath; }
			set { logPath = value;
				OnPropertyChanged();
			}
		}

	}
}
