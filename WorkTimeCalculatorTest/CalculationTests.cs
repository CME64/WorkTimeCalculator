using System;
using Xunit;
using WorkTimeCalculatorLib;
using System.Collections.Generic;
using WorkTimeCalculatorLib.Models;

namespace WorkTimeCalculatorTest {
    public class CalculationTests {
        readonly WorkTimeCalculator calc;
		public CalculationTests() {
			Dictionary<DayOfWeek, List<WorkShift>> DayShifts = new Dictionary<DayOfWeek, List<WorkShift>>() {
				{ DayOfWeek.Sunday, new List<WorkShift>(){
					new WorkShift(){ Start = new TimeSpan(9,0,0), End = new TimeSpan(13,0,0)},
					new WorkShift(){ Start = new TimeSpan(14,0,0), End = new TimeSpan(18,0,0)}
				} },
				{ DayOfWeek.Monday, new List<WorkShift>(){
					new WorkShift(){ Start = new TimeSpan(9,0,0), End = new TimeSpan(13,0,0)},
					new WorkShift(){ Start = new TimeSpan(14,0,0), End = new TimeSpan(18,0,0)}
				} },
				{ DayOfWeek.Tuesday, new List<WorkShift>(){
					new WorkShift(){ Start = new TimeSpan(9,0,0), End = new TimeSpan(13,0,0)},
					new WorkShift(){ Start = new TimeSpan(14,0,0), End = new TimeSpan(18,0,0)}
				} },
				{ DayOfWeek.Wednesday, new List<WorkShift>(){
					new WorkShift(){ Start = new TimeSpan(9,0,0), End = new TimeSpan(13,0,0)},
					new WorkShift(){ Start = new TimeSpan(14,0,0), End = new TimeSpan(18,0,0)}
				} },
				{ DayOfWeek.Thursday, new List<WorkShift>(){
					new WorkShift(){ Start = new TimeSpan(9,0,0), End = new TimeSpan(13,0,0)},
					new WorkShift(){ Start = new TimeSpan(14,0,0), End = new TimeSpan(18,0,0)}
				} }
			};

			List<HolidayConfig> Holidays = new List<HolidayConfig>() {
				new HolidayConfig (){ Start = new DateTime(2021, 6, 2), End = new DateTime(2021, 6, 8) },
				new HolidayConfig (){ Start = new DateTime(2021, 7, 2), End = new DateTime(2021, 7, 8) },
				new HolidayConfig (){ Start = new DateTime(2021, 8, 2), End = new DateTime(2021, 8, 8) }
			};

			calc = new WorkTimeCalculator(DayShifts, Holidays);
        }

        [Fact]
        public void FullDay() {
			var time = calc.CalculateWorkTime(new DateTime(2021,6,1,0,0,0), new DateTime(2021,6,2,0,0,0));
			Assert.Equal((new TimeSpan(8, 0, 0)).TotalSeconds, time.TotalSeconds);
        }

		[Fact]
		public void FullWeek() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 8, 15, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
			Assert.Equal((new TimeSpan(40, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void PartialDay() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 8, 15, 10, 0, 0), new DateTime(2021, 8, 15, 15, 0, 0));
			Assert.Equal((new TimeSpan(4, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void PartialFollowingDays() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 8, 15, 10, 0, 0), new DateTime(2021, 8, 16, 15, 0, 0));
			Assert.Equal((new TimeSpan(12, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void WrongDateRange() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 8, 15, 10, 0, 0), new DateTime(2021, 8, 14, 15, 0, 0));
			Assert.Equal((new TimeSpan(0, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void WithinHoliday() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 3, 0, 0, 0), new DateTime(2021, 6, 7, 0, 0, 0));
			Assert.Equal((new TimeSpan(0, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void HolidayInTheMiddle() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 1, 0, 0, 0), new DateTime(2021, 6, 10, 0, 0, 0));
			Assert.Equal((new TimeSpan(16, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void HolidayIntersectEnd() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 1, 0, 0, 0), new DateTime(2021, 6, 5, 0, 0, 0));
			Assert.Equal((new TimeSpan(8, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void HolidayIntersectBeginning() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 5, 0, 0, 0), new DateTime(2021, 6, 10, 0, 0, 0));
			Assert.Equal((new TimeSpan(8, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void HolidayIntersectFirstDay() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 8, 0, 0, 0), new DateTime(2021, 6, 10, 0, 0, 0));
			Assert.Equal((new TimeSpan(8, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void HolidayIntersectLastDay() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 1, 0, 0, 0), new DateTime(2021, 6, 3, 0, 0, 0));
			Assert.Equal((new TimeSpan(8, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void IntersectWithTwoHolidays() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 5, 0, 0, 0), new DateTime(2021, 7, 5, 0, 0, 0));
			Assert.Equal((new TimeSpan(136, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void IntersectWithMultipleHolidays() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 5, 0, 0, 0), new DateTime(2021, 8, 5, 0, 0, 0));
			Assert.Equal((new TimeSpan(264, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void IncludeMultipleHolidays() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 1, 0, 0, 0), new DateTime(2021, 8, 10, 0, 0, 0));
			Assert.Equal((new TimeSpan(280, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void EqualsHoliday() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 2, 0, 0, 0), new DateTime(2021, 6, 8, 0, 0, 0));
			Assert.Equal((new TimeSpan(0, 0, 0)).TotalSeconds, time.TotalSeconds);
		}

		[Fact]
		public void HolidayTwoFollowingDays() {
			var time = calc.CalculateWorkTime(new DateTime(2021, 6, 8, 0, 0, 0), new DateTime(2021, 6, 9, 10, 0, 0));
			Assert.Equal((new TimeSpan(1, 0, 0)).TotalSeconds, time.TotalSeconds);
		}
	}
}
