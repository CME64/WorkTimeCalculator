using System;
using System.Collections.Generic;
using System.Text;
using WorkTimeCalculatorLib.Models;

namespace WorkTimeCalculatorLib {
    public class ConfigBuilders {
		/// <summary>
		/// Sets configuration objects with calculated data for calculation reference
		/// </summary>
		/// <param name="shiftsConfig">shifts information</param>
		/// <param name="shifts">shifts info ordered depending on start time</param>
		/// <param name="stats">precalculated shifts aggregate data</param>
		/// <param name="totalWeekTime">total workhours per full week</param>
		public static void ShiftsConfigBuilder(Dictionary<DayOfWeek, List<WorkShift>> shiftsConfig, out Dictionary<DayOfWeek, List<WorkShift>> shifts, out Dictionary<DayOfWeek, DayWorkStatistics> stats, out TimeSpan totalWeekTime) {
			stats = new Dictionary<DayOfWeek, DayWorkStatistics>();
			totalWeekTime = new TimeSpan();
			shifts = new Dictionary<DayOfWeek, List<WorkShift>>();

			if (shiftsConfig == null || shiftsConfig.Count == 0) return;
			TimeSpan totalDay = new TimeSpan();
			WorkShift dayBounds = new WorkShift();

			foreach (var day in shiftsConfig) {
				day.Value.Sort((WorkShift a, WorkShift b) => { return a.Start > b.Start ? 1 : -1; });
				totalDay = new TimeSpan();
				dayBounds = new WorkShift();
				for (int i = 0; i < day.Value.Count; i++) {
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
		/// <param name="currentCalculator">calculator to be used for holiday period workhours calculation</param>
		/// <param name="holidaysConfig">holidays info</param>
		/// <param name="holidaysStats">precalculated holidays info for calculation</param>
		public static void HolidaysConfigBuilder(WorkTimeCalculator currentCalculator, List<HolidayConfig> holidaysConfig, out List<Holiday> holidaysStats) {
			holidaysStats = new List<Holiday>();

			foreach (var holiday in holidaysConfig) {
				holidaysStats.Add(new Holiday() { Start = holiday.Start, End = holiday.End, TotalWork = currentCalculator.CalculateWorkTime(holiday.Start, holiday.End.AddDays(1), false) });
			}
		}
	}
}
