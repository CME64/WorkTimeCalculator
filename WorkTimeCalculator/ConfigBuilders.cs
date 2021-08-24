using System;
using System.Collections.Generic;
using System.Text;
using WorkTimeCalculatorLib.Models;

namespace WorkTimeCalculatorLib {
    public class ConfigBuilders {
		/// <summary>
		/// Sets configuration objects with calculated data for calculation reference
		/// </summary>
		/// <param name="shiftsConfig">Shifts information</param>
		/// <param name="shifts">Shifts info ordered depending on start time</param>
		/// <param name="stats">Precalculated shifts aggregate data</param>
		/// <param name="totalWeekTime">Total workhours per full week</param>
		/// <exception cref="ArgumentException">On misconfiguration of shifts and empty shifts</exception>
		public static void ShiftsConfigBuilder(Dictionary<DayOfWeek, List<WorkShift>> shiftsConfig, out Dictionary<DayOfWeek, List<WorkShift>> shifts, out Dictionary<DayOfWeek, DayWorkStatistics> stats, out TimeSpan totalWeekTime) {
			stats = new Dictionary<DayOfWeek, DayWorkStatistics>();
			totalWeekTime = new TimeSpan();
			shifts = new Dictionary<DayOfWeek, List<WorkShift>>();

			if (shiftsConfig == null || shiftsConfig.Count == 0)
				throw new ArgumentException("Working schedule is empty");
			TimeSpan totalDay = new TimeSpan();
			WorkShift dayBounds = new WorkShift();

			foreach (var day in shiftsConfig) {
				day.Value.Sort((WorkShift a, WorkShift b) => { return a.Start > b.Start ? 1 : -1; });
				totalDay = new TimeSpan();
				dayBounds = new WorkShift();
				for (int i = 0; i < day.Value.Count; i++) {
					if (day.Value[i].Start >= day.Value[i].End)
						throw new ArgumentException($"Working schedule misconfiguration, a shift ends before the start in ({day.Key})");
					if ( day.Value[i].Start > new TimeSpan(24, 0, 0) || day.Value[i].Start < new TimeSpan(0, 0, 0) || day.Value[i].End > new TimeSpan(24, 0, 0) || day.Value[i].End < new TimeSpan(0, 0, 0))
						throw new ArgumentException($"Working schedule misconfiguration, a value is out of day hours range (0-24) in ({day.Key})");
					if (i == 0)
						dayBounds.Start = day.Value[i].Start;
					if (i == day.Value.Count - 1)
						dayBounds.End = day.Value[i].End;
					totalDay += day.Value[i].End.Subtract(day.Value[i].Start);
				}
				totalWeekTime += totalDay;
				stats.Add(day.Key, new DayWorkStatistics() { TotalHours = totalDay, WorkHoursBounds = dayBounds });
				if(totalDay.TotalSeconds>0)
					shifts.Add(day.Key, day.Value);
			}
		}

		/// <summary>
		/// Sets holiday configuration with precalculated data
		/// </summary>
		/// <param name="currentCalculator">Calculator to be used for holiday period workhours calculation</param>
		/// <param name="holidaysConfig">Holidays info</param>
		/// <param name="holidaysStats">Precalculated holidays info for calculation</param>
		/// <exception cref="ArgumentException">On misconfiguration of holidays</exception>
		public static void HolidaysConfigBuilder(WorkTimeCalculator currentCalculator, List<HolidayConfig> holidaysConfig, out List<Holiday> holidaysStats) {
			holidaysStats = new List<Holiday>();

			foreach (var holiday in holidaysConfig) {
				if (holiday.Start > holiday.End)
					throw new ArgumentException($"Holiday end preceeds the start [from {holiday.Start} to {holiday.End}]");
				holidaysStats.Add(new Holiday() { Start = holiday.Start, End = holiday.End, TotalWork = currentCalculator.CalculateWorkTime(holiday.Start, holiday.End.AddDays(1), false) });
			}
		}
	}
}
