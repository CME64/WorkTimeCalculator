using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeCalculatorLib.Models;

namespace WorkTimeCalculatorLib {
	
	public class WorkTimeCalculator {

		private Dictionary<DayOfWeek, List<WorkShift>> DayShifts;
		private Dictionary<DayOfWeek, DayWorkStatistics> DaysWorkStats;
		private TimeSpan TotalWeekWorkTime = new TimeSpan();
		private List<Holiday> Holidays = new List<Holiday>();

		/// <summary>
		/// Creates a configured calculator instance with the provided work schedule and holidays
		/// </summary>
		/// <param name="workSchedule">The weekly work schedule</param>
		/// <param name="holidaysConfig">Holidays list</param>
		/// <exception cref="ArgumentException">On misconfiguration of holidays, shifts, and empty shifts</exception>
		public WorkTimeCalculator(Dictionary<DayOfWeek, List<WorkShift>> workSchedule, List<HolidayConfig> holidaysConfig) {
			this.SetConfigurations(workSchedule, holidaysConfig);
		}

		/// <summary>
		/// Will be called by the constructor to initialize configuration. can be used externally to change configuration as well
		/// </summary>
		/// <param name="shiftsConfig">The weekly work schedule</param>
		/// <param name="holidaysConfig">Holidays list</param>
		/// <exception cref="ArgumentException">On misconfiguration of holidays, shifts, and empty shifts</exception>
		public void SetConfigurations(Dictionary<DayOfWeek, List<WorkShift>> shiftsConfig, List<HolidayConfig> holidaysConfig) {
			ConfigBuilders.ShiftsConfigBuilder(shiftsConfig, out DayShifts, out DaysWorkStats, out TotalWeekWorkTime);
			ConfigBuilders.HolidaysConfigBuilder(this, holidaysConfig, out Holidays);
		}

		/// <summary>
		/// For internal use to calculate paritial work hours from shifts information in a specific day of week
		/// </summary>
		/// <param name="start">Start time</param>
		/// <param name="end">End time</param>
		/// <param name="day">Day of week</param>
		/// <returns>Total workhours timespan</returns>
		private TimeSpan CalculatePartialDayWorktime(TimeSpan start, TimeSpan end, DayOfWeek day) {
			if (!DaysWorkStats.ContainsKey(day) || !DayShifts.ContainsKey(day))
				return new TimeSpan();

			var dayShifts = DayShifts[day];
			var thisDay = DaysWorkStats[day];

			TimeSpan totalTime = new TimeSpan();

			if (start <= thisDay.WorkHoursBounds.Start && end >= thisDay.WorkHoursBounds.End) { // range covers all shifts
				return thisDay.TotalHours;
			}
			else if (start >= thisDay.WorkHoursBounds.End || end <= thisDay.WorkHoursBounds.Start) { //range outside shifts
				return new TimeSpan();
			}

			//partial shifts calculation in seconds
			for (int i = 0; i < dayShifts.Count; i++) {
				if (dayShifts[i].Start <= start && start <= dayShifts[i].End) { // start in this shift
					if (dayShifts[i].Start <= end && end <= dayShifts[i].End) { // end in the same shift
						return end - start;
					}
					else { // calculate remaining time of shift
						totalTime += dayShifts[i].End - start;
					}
				}
				else if (start < dayShifts[i].Start && dayShifts[i].Start <= end && end <= dayShifts[i].End) { // start before and end in this shift
					totalTime += end - dayShifts[i].Start;
					return totalTime;
				}
				else if (start < dayShifts[i].Start) { // start is before this shift
					totalTime += dayShifts[i].End - dayShifts[i].Start;
				}
			}

			return totalTime;
		}

		/// <summary>
		/// Calculate holiday workhours to be subtracted from actual workhours
		/// </summary>
		/// <param name="start">Start of period datetime</param>
		/// <param name="end">End of period datetime</param>
		/// <returns>Total holiday work hours as a timespan</returns>
		private TimeSpan CalculateHolidaysWorkTime(DateTime start, DateTime end) {
			if (Holidays.Count == 0) return new TimeSpan();

			var holidaysWithin = Holidays.Where(h => h.Start.Date.CompareTo(start.Date) >= 0 && h.End.Date.CompareTo(end.Date) <= 0);
			var borderStartHolidays = Holidays.Where(h => h.Start.Date.CompareTo(start.Date) < 0 && h.End.Date.CompareTo(start.Date) >= 0 && h.End.Date.CompareTo(end.Date) < 0);
			var borderEndHolidays = Holidays.Where(h => h.Start.Date.CompareTo(end.Date) <= 0 && h.Start.Date.CompareTo(start.Date) > 0 && h.End.Date.CompareTo(end.Date) > 0);
			TimeSpan totalTime = new TimeSpan(holidaysWithin.Select(h => h.TotalWork.Ticks).Sum());

			foreach (var hd in borderStartHolidays) {
				totalTime += CalculateWorkTime(start, hd.End.AddDays(1), false);
			}

			foreach (var hd in borderEndHolidays) {
				totalTime += CalculateWorkTime(hd.Start, end, false);
			}

			return totalTime;
		}

		/// <summary>
		/// Calculate workhours from datetime to datetime (accuracy is calculated in seconds)
		/// </summary>
		/// <param name="start">Start of period datetime</param>
		/// <param name="end">End of period datetime</param>
		/// <param name="calculateHolidays">Set to false to ignore holidays configuration [default: true]</param>
		/// <returns>Total work hours as a timespan</returns>
		public TimeSpan CalculateWorkTime(DateTime start, DateTime end, bool calculateHolidays = true) {
			//wrong date range
			if (end.CompareTo(start) <= 0 || calculateHolidays && Holidays.Any(h => h.Start.Date.CompareTo(start.Date) <= 0 && h.End.Date.CompareTo(end.Date) >= 0))
				return new TimeSpan();

			bool firstDayHoliday = calculateHolidays && Holidays.Any(d => d.Start.Date.CompareTo(start.Date) <= 0 && d.End.Date.CompareTo(start.Date) >= 0);

			//single day
			if (end.Date.CompareTo(start.Date) == 0) {
				if (firstDayHoliday)
					return new TimeSpan();

				return CalculatePartialDayWorktime(start.TimeOfDay, end.TimeOfDay, start.DayOfWeek);
			}

			TimeSpan firstDay = CalculatePartialDayWorktime(start.TimeOfDay, new TimeSpan(24, 0, 0), start.DayOfWeek);
			TimeSpan lastDay = CalculatePartialDayWorktime(new TimeSpan(), end.TimeOfDay, end.DayOfWeek);

			//following days
			if (end.Date.Subtract(start.Date).Days == 1) {
				bool lastDayHoliday = calculateHolidays ? Holidays.Any(d => d.Start.Date.CompareTo(end.Date) <= 0 && d.End.Date.CompareTo(end.Date) >= 0) : false;
				return (firstDayHoliday?new TimeSpan():firstDay) + (lastDayHoliday ? new TimeSpan() : lastDay);
			}

			int startDay = ((int)start.DayOfWeek + 1) % 7;
			int totalDays = (int)Math.Ceiling(end.Date.Subtract(start.Date).TotalDays - 1); // subtract calculated days (first and last) -1 day for the time reset
			int totalWeeks = (int)(totalDays / 7);
			int additionalDays = (int)(totalDays % 7);
			TimeSpan totalHolidaysWork = calculateHolidays ? CalculateHolidaysWorkTime(start, end) : new TimeSpan();
			TimeSpan totalWeekTime = TotalWeekWorkTime;
			TimeSpan remainingWorkSecs = new TimeSpan();
			for (int i = 0; i < additionalDays; i++) {
				DayOfWeek dayIter = (DayOfWeek)((startDay + i) % 7);
				if (DaysWorkStats.ContainsKey(dayIter) && DaysWorkStats[dayIter].TotalHours.TotalSeconds > 0) {
					remainingWorkSecs += DaysWorkStats[dayIter].TotalHours;
				}
			}

			return new TimeSpan(totalWeeks * totalWeekTime.Ticks) + remainingWorkSecs + firstDay + lastDay - totalHolidaysWork;
		}
	}
}
