using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class DeviceLog
    {
        [Name("TZ Timestamp")]
        public DateTime DateTime { get; set; }
        [Name("Local Timestamp")]
        public string LocalTime { get; set; }
        [Name("Procedure Time")]
        public string ProcedureTime { get; set; }
        [Name("Module Name")]
        public string ModuleName { get; set; }
        public string Detail { get; set; }
        [Name("File Name")]
        public string FileName { get; set; }
        public string Line { get; set; }
        [Name("Log Level")]
        public string LogLevel { get; set; }
        [Name("Log Type")]
        public string LogType { get; set; }
        public string Function { get; set; }
        [Name("File Spec")]
        public string FileSpec { get; set; }
        [Name("Energy Channel")]
        public string EnergyChannel { get; set; }

        public override string ToString()
        {
            return $"{LogLevel} {Detail}";
        }
    }
}
