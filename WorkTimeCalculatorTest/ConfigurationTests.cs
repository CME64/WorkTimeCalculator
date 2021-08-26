using System;
using System.Collections.Generic;
using System.Text;
using WorkTimeCalculatorLib;
using WorkTimeCalculatorLib.Models;
using Xunit;

namespace WorkTimeCalculatorTest {
    public class ConfigurationTests {
        [Fact]
        public void ShiftsTimeBeyondAllowedRange() {
            Dictionary<DayOfWeek, List<WorkShift>> DayShifts = new Dictionary<DayOfWeek, List<WorkShift>>() {
                { DayOfWeek.Sunday, new List<WorkShift>(){
                    new WorkShift(){ Start = new TimeSpan(25,0,0), End = new TimeSpan(26,0,0)}
                } }
            };

            List<HolidayConfig> Holidays = new List<HolidayConfig>();

            var ex = Assert.Throws<ArgumentException>(() => new WorkTimeCalculator(DayShifts, Holidays));
            Assert.Equal($"Working schedule misconfiguration, a value is out of day hours range (0-24) in ({DayOfWeek.Sunday})", ex.Message);
        }

        [Fact]
        public void ShiftsTimeLessThanAllowedRange() {
            Dictionary<DayOfWeek, List<WorkShift>> DayShifts = new Dictionary<DayOfWeek, List<WorkShift>>() {
                { DayOfWeek.Sunday, new List<WorkShift>(){
                    new WorkShift(){ Start = new TimeSpan(-5,0,0), End = new TimeSpan(-1,0,0)}
                } }
            };

            List<HolidayConfig> Holidays = new List<HolidayConfig>();

            var ex = Assert.Throws<ArgumentException>(() => new WorkTimeCalculator(DayShifts, Holidays));
            Assert.Equal($"Working schedule misconfiguration, a value is out of day hours range (0-24) in ({DayOfWeek.Sunday})", ex.Message);
        }

        [Fact]
        public void ShiftEndPreceedsBeginning() {
            Dictionary<DayOfWeek, List<WorkShift>> DayShifts = new Dictionary<DayOfWeek, List<WorkShift>>() {
                { DayOfWeek.Sunday, new List<WorkShift>(){
                    new WorkShift(){ Start = new TimeSpan(10,0,0), End = new TimeSpan(5,0,0)}
                } }
            };

            List<HolidayConfig> Holidays = new List<HolidayConfig>();

            var ex = Assert.Throws<ArgumentException>(() => new WorkTimeCalculator(DayShifts, Holidays));
            Assert.Equal($"Working schedule misconfiguration, a shift ends before the start in ({DayOfWeek.Sunday})", ex.Message);
        }

        [Fact]
        public void HolidayRangeReversed() {
            Dictionary<DayOfWeek, List<WorkShift>> DayShifts = new Dictionary<DayOfWeek, List<WorkShift>>() {
                { DayOfWeek.Sunday, new List<WorkShift>(){
                    new WorkShift(){ Start = new TimeSpan(5,0,0), End = new TimeSpan(10,0,0)}
                } }
            };

            List<HolidayConfig> Holidays = new List<HolidayConfig>() {
                new HolidayConfig(){ Start = new DateTime(2021,8,24), End = new DateTime(2021,8,20)}
            };

            var ex = Assert.Throws<ArgumentException>(() => new WorkTimeCalculator(DayShifts, Holidays));
            Assert.Equal($"Holiday end preceeds the start [from {Holidays[0].Start} to {Holidays[0].End}]", ex.Message);
        }

        [Fact]
        public void ExactPeriodRange() {
            Dictionary<DayOfWeek, List<WorkShift>> DayShifts = new Dictionary<DayOfWeek, List<WorkShift>>() {
                { DayOfWeek.Sunday, new List<WorkShift>(){
                    new WorkShift(){ Start = new TimeSpan(0,0,0), End = new TimeSpan(24,0,0)}
                } }
            };

            List<HolidayConfig> Holidays = new List<HolidayConfig>();
            var c = new WorkTimeCalculator(DayShifts, Holidays);

            Assert.NotNull(c);
        }

        [Fact]
        public void FullDaysWork() {
            Dictionary<DayOfWeek, List<WorkShift>> DayShifts = new Dictionary<DayOfWeek, List<WorkShift>>() {
                { DayOfWeek.Sunday, new List<WorkShift>(){
                    new WorkShift(){ Start = new TimeSpan(0,0,0), End = new TimeSpan(24,0,0)}
                } }
            };

            List<HolidayConfig> Holidays = new List<HolidayConfig>();

            var c = new WorkTimeCalculator(DayShifts, Holidays);
            Assert.Equal(new TimeSpan(5,0,0), c.CalculateWorkTime(new DateTime(2021,8,1,10,0,0), new DateTime(2021, 8, 1, 15, 0, 0)));
        }
    }
    
}
