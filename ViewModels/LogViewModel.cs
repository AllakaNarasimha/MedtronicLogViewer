
using BusinessLayer;
using BusinessLayer.Models;
using Microsoft.Win32;
using MVVMBase;
using ScaleIndicator;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows;
using AutoMapper;
using System.Collections.ObjectModel;

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
        private DeviceLogViewModel selectedLog;

        private int gridSelectedIndex;

        public int GridSelectedIndex
        {
            get { return gridSelectedIndex; }
            set
            {
                if (SelectedTabIndex != 1)
                {
                    SelectedTabIndex = 1;
                    OnPropertyChanged();
                    return;
                }
                gridSelectedIndex = value;                
                if (selectedTabIndex == 1 && !isTimelineClicked)
                {
                    var index = LocalCache.FindKeysByItemInRange(gridSelectedIndex).FirstOrDefault();
                    if(index > 99)
                    {
                        index -= 2;
                    }
                    //TimeLineIndex = Math.Min(index, 99);                    
                }               
                OnPropertyChanged();
            }
        }
        private int listWidthstep = 0;
        private double onePercent = 0;
        private int  columnSpan = 0;
        private bool isTimelineClicked = false;

        public DeviceLogViewModel SelectedLog
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
           
            onePercent = (1.0 / eventList.Count) * 100.0;
            listWidthstep = (int)( (1 / onePercent) <= 0 ? 1 : (1 / onePercent));
            columnSpan = (int)Math.Max(onePercent, 1);
            PercentText = $"One percent equals {listWidthstep} rows in total rows of {eventList.Count}";
            int startListIndex = 0;
            int rCount = onePercent <= 0 ? 1 : listWidthstep;
            int startPercentage = 0;
            int j = 1;
            while (startListIndex < eventList.Count)
            {
                var logIndex = onePercent <= 0 ? startPercentage : j;
                var refListIndex = logIndex;

                if (eventList[logIndex] != null)
                {
                    bool isdev = (eventList[logIndex].GetType() == typeof(DeviceLogViewModel));                    
                    var event_item = onePercent <= 0 ? eventList[logIndex] : GetLogType(j, rCount, out refListIndex);                    
                    UpdateColor(event_item, startPercentage, columnSpan, startListIndex, listWidthstep, refListIndex);
                    LocalCache.AddItem((startPercentage), startListIndex , (startListIndex  + listWidthstep));
                }

                int nextJ = (startListIndex + listWidthstep);
                if (onePercent > 0 && nextJ >= eventList.Count)
                {
                    break;
                }
                startListIndex += listWidthstep;
                j = startListIndex;
                startPercentage++;
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
                else if (e.Index.ListIndex == 0 && e.Index.StartListIndex == 0)
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

        private int getErrorId(DeviceLogViewModel event_item)
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

        private void UpdateColor(DeviceLogViewModel event_item, int startPercentage, int columnSpan, int startListIndex, int listWidthStep, int referListIndex = 1)
        {
            if (event_item != null && event_item.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                int errorIndex = getErrorId(event_item);

                int errorType = GetErrorType(errorIndex);
                if (errorType > 0)
                {
                    System.Windows.Media.Color color = errorType == 3 ? Colors.Red :
                        errorType == 2 ? Colors.OrangeRed : Colors.MediumVioletRed;
                    updateProgressColor(color, startPercentage, columnSpan, referListIndex, startListIndex, listWidthStep);
                }
            }
            else
            {
                updateProgressColor(TimelineControl.GridBackgroundColor, startPercentage, columnSpan, referListIndex, startListIndex, listWidthStep);
            }
        }
        private DeviceLogViewModel GetLogType(int startListIndex, int rindex, out int logIndex)
        {
            logIndex = startListIndex;
            var nLogs = EventLogCollection.Select((log, index) => new { Log = log, Index = index });
            var clogs = nLogs.Skip(logIndex).Take(rindex);
            

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


        /*private void UpdateColor_old(DeviceLog event_item, int sPercent, int logIndex, int listWidthstep = 1)
        {
            if (event_item.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                updateProgressColor(Colors.Red, sPercent, listWidthstep, logIndex);
            }
            else if (event_item.LogLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase))
            {
                updateProgressColor(Colors.MediumVioletRed, sPercent, listWidthstep, logIndex);
            }
            else if (event_item.LogLevel.Equals("INFO", StringComparison.OrdinalIgnoreCase))
            {
                updateProgressColor(Colors.LightPink, sPercent, listWidthstep, logIndex);
            }
            else
            {
                Console.WriteLine("Not matched with any list");
            }
        }*/


        private void updateProgressColor(System.Windows.Media.Color color, int startPercentage, int columnSpan, int referListIndex, int startListIndex, int listWidthStep)
        {
            TimelineControl?.UpdateScaleColorAt(color, startPercentage, columnSpan, referListIndex, startListIndex, listWidthStep);
        }

        private DeviceLogViewModel GetLogType_old(int sindex, int rindex, out int logIndex)
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

        private ObservableCollection<DeviceLogViewModel> errorLogCollection ;
        private ObservableCollection<DeviceLogViewModel> eventLogCollection;

        private int selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return selectedTabIndex; }
            set { selectedTabIndex = value;
                //UpdateScaleMethod();
                OnPropertyChanged();
            }
        }


        public ObservableCollection<DeviceLogViewModel> ErrorLogCollection
        {
            get { return errorLogCollection; }
            set
            {
                errorLogCollection = value;                
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DeviceLogViewModel> EventLogCollection
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
            LocalCache.ClearCache();
            EventLogCollection?.Clear();
            ErrorLogCollection?.Clear();

            Task.Delay(2500).ContinueWith(task =>
            {
                Application.Current.Dispatcher.BeginInvoke(delegate {
                    var logs = LogReader<DeviceLog>.ReadLogFile(logPath);

                    var config = new MapperConfiguration(cfg =>
                    {
                        cfg.CreateMap<DeviceLog, DeviceLogViewModel>();
                    });
                    var mapper = new Mapper(config);

                    var evenLogs = new ObservableCollection<DeviceLogViewModel>();                    
                    int index = 0;
                    foreach (var log in logs)
                    {
                        var lg = mapper.Map<DeviceLogViewModel>(log);
                        lg.RowId = index++;
                        evenLogs.Add(lg);
                    }

                    EventLogCollection = evenLogs;
                    var errors = EventLogCollection.Where(x => x.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase)).ToList();
                    ErrorLogCollection = new ObservableCollection<DeviceLogViewModel>(errors);

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
                LocalCache.SetActiveIndex(value);
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
                ErrorLogCollection = new ObservableCollection<DeviceLogViewModel>(filteredLogs);
            }
            else
            {
                errors = EventLogCollection.Where(x => x.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase)).ToList();
                ErrorLogCollection = new ObservableCollection<DeviceLogViewModel>(errors);
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

    public class DeviceLogViewModel : BaseViewModel
    {
        private int rowId;
        private DateTime dateTime;
        private string localTime;
        private string procedureTime;
        private string moduleName;
        private string detail;
        private string fileName;
        private string line;
        private string logLevel;
        private string logType;
        private string function;
        private string fileSpec;
        private string energyChannel;

        public int RowId { get => rowId; set  
                { rowId = value; OnPropertyChanged(); }
        }

        public DateTime DateTime { get => dateTime; set { dateTime = value; OnPropertyChanged(); } }
        public string LocalTime { get => localTime; set { localTime = value; OnPropertyChanged(); } }
        public string ProcedureTime { get => procedureTime; set { procedureTime = value; OnPropertyChanged(); } }
        public string ModuleName { get => moduleName; set { moduleName = value; OnPropertyChanged(); } }
        public string Detail { get => detail; set { detail = value; OnPropertyChanged(); } }
        public string FileName { get => fileName; set { fileName = value; OnPropertyChanged(); } }
        public string Line { get => line; set { line = value; OnPropertyChanged(); } }
        public string LogLevel { get => logLevel; set { logLevel = value; OnPropertyChanged(); }}
        public string LogType { get => logType; set { logType = value;  OnPropertyChanged(); } }
        public string Function { get => function; set { function = value; OnPropertyChanged(); } }
        public string FileSpec { get => fileSpec; set { fileSpec = value; OnPropertyChanged(); } }
        public string EnergyChannel { get => energyChannel; set { energyChannel = value; OnPropertyChanged(); } }

        public override string ToString()
        {
            return $"{LogLevel} {Detail}";
        }
    }
}
