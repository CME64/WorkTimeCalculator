using System;
using System.Collections.Generic;
using System.Text;

namespace WorkTimeCalculatorLib.Models {
    public class Holiday {
        public DateTime Start { set; get; }
        public DateTime End { get; set; }
        public TimeSpan TotalWork { get; set; }
    }
}
