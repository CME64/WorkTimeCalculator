# WorkTimeCalculator

A utility library to calculate work time within a period, with a configurable work schedule and holidays.

## Support
This project is built using C# .Net Core 3.1 class library but it can be ported and compiled with C# .Net

The package is available for multiple frameworks as a NuGet on [NuGet.org](https://www.nuget.org/packages/WorkTimeCalculator) and you can also use the following *Package Manager* command to add it to your project
```
Install-Package WorkTimeCalculator
```

## How To Use
After importing the library in the dependencies of your project, create an instance of the calculator with the work schedule and holidays configuration
```C#
var calculator = new WorkTimeCalculator(MySchedule, MyHolidays);
```

You can also change the configuration if needed by calling `SetConfigurations`
```C#
calculator.SetConfigurations(MySchedule, MyHolidays);
```

Then call the `CalculateWorkTime` function
```C#
TimeSpan result = calculator.CalculateWorkTime(
  new DateTime(2021, 6, 1, 10, 30, 0), 
  new DateTime(2021, 6, 10, 15, 30, 0),
  true //Optional bool parameter to exclude holidays from calculation [default: true]
);
```

*Work Schedule Configuration Example*
```C#
Dictionary<DayOfWeek, List<WorkShift>> MySchedule = new Dictionary<DayOfWeek, List<WorkShift>>() {
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
```

*Holidays Configuration Example*
```C#
List<HolidayConfig> MyHolidays = new List<HolidayConfig>() {
  new HolidayConfig (){ Start = new DateTime(2021, 6, 2), End = new DateTime(2021, 6, 8) },
  new HolidayConfig (){ Start = new DateTime(2021, 7, 2), End = new DateTime(2021, 7, 8) },
  new HolidayConfig (){ Start = new DateTime(2021, 8, 2), End = new DateTime(2021, 8, 8) }
};
```

## Notes
- The returned type is `TimeSpan` so the user can extract the value in any time format as needed (hours, minutes, seconds ...etc)
- The accuracy of the calculated time is in seconds
- Holidays date configuration is all inclusive of the full days
- If a day was not configured in the schedule or it has no shifts (or shifts total time value is 0) then it is a non working day
- Configuration is not checked for validity and it is the responsilbility of the developer, if there are intersections between shifts or holidays it can lead to wrong results
- The performance of this utility is almost `O(n*k)` where `n` is the maximum number of shifts per day, and `k` is the number of holidays within a single period

