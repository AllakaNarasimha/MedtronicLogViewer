using CsvHelper;
using CsvHelper.Configuration;
using MVVMBase;
using System.Globalization;
using System.IO;

namespace BusinessLayer
{
    public class LogReader<T>
    {
        public static List<T> ReadLogFile(string filePath)
        {
            var logs = new List<T>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<T>();
                foreach (var rec in records)
                {
                    logs.Add(rec);
                }

            }
            return logs;
        }        
    }
}
