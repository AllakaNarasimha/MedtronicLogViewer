using BusinessLayer.Models;
using MVVMBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ViewModels
{
    public class ScaleUitls
    {
        ProgressColors progressColors = new ProgressColors(string.Empty);
        RangeObservableCollection<DeviceLog> eventList = new RangeObservableCollection<DeviceLog>();
        public int onePercent { get; set; } = 1;

        private DeviceLog GetLogType(int sindex, int rindex, out int logIndex)
        {
            logIndex = sindex;
            var nLogs = eventList.Select((log, index) => new { Log = log, Index = index });
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

        private void updateScale(dynamic eventList)
        {
            this.eventList = eventList;
            int widthStep = onePercent <= 0 ? (100 / eventList.Count) : onePercent;
            int i = 0;
            while (i < eventList.Count)
            {

                int j = i * widthStep <= 0 ? 1 : i * widthStep;
                var logIndex = onePercent <= 0 ? i : j;
                var listIndex = onePercent <= 0 ? i : j;

                if (eventList[logIndex] != null)
                {
                    bool isdev = (eventList[logIndex].GetType() == typeof(DeviceLog));
                    int rCount = onePercent <= 0 ? 1 : widthStep;
                    var event_item = onePercent <= 0 ? eventList[logIndex] : GetLogType(j, rCount, out listIndex);
                    int sPercent = onePercent <= 0 ? j : i;
                    UpdateColor(event_item, sPercent, listIndex, rCount);
                }
                int nextJ = (i + 1) * widthStep;
                if (onePercent > 0 && nextJ >= eventList.Count)
                {
                    break;
                }
                i++;
            }
            if (100.00 / eventList.Count > 0)
            {
                int error = 100 - (100 / eventList.Count) + 1;
                updateProgressColor(Colors.White, error, widthStep, 0);
            }
            progressColors.OnClickedolor += ProgressColors_OnClickedolor;
        }
        private void updateProgressColor(Color color, int s_index, int r_count, int listIndex)
        {
            progressColors.UpdateScaleColorAt(TimeLineGrid, color, s_index, r_count, listIndex);
        }
        private void UpdateColor(DeviceLog event_item, int sPercent, int logIndex, int widthStep = 1)
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
        }
    }
}
