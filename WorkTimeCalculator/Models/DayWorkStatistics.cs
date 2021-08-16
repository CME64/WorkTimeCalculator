using System;
using System.Collections.Generic;
using System.Text;

namespace WorkTimeCalculatorLib.Models {
    public class DayWorkStatistics {
        public TimeSpan TotalHours { get; set; }
        public WorkShift WorkHoursBounds { get; set; }
    }
}
