using FoodCalendar.Models;
using FoodCalendar.Repositories;

namespace FoodCalendar.Services;

public sealed record CalendarDay(
    DateOnly Date, bool InMonth, bool IsWeekend, bool IsToday,
    Meal? Main, int ExtrasCount, bool HasReceipt);

public sealed class MonthGrid
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string Title { get; init; } = "";
    public List<CalendarDay[]> Weeks { get; init; } = new();
}

public sealed class CalendarService
{
    public static readonly string[] MonthNames =
    {
        "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
    };

    public static readonly string[] MonthNamesGenitive =
    {
        "января", "февраля", "марта", "апреля", "мая", "июня",
        "июля", "августа", "сентября", "октября", "ноября", "декабря"
    };

    public static readonly string[] WeekdayNames =
    {
        "воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота"
    };

    private readonly MealRepository _meals;

    public CalendarService(MealRepository meals) => _meals = meals;

    public static bool IsWeekend(DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    /// <summary>Например: 29 октября, среда</summary>
    public static string FormatLong(DateOnly date) =>
        $"{date.Day} {MonthNamesGenitive[date.Month - 1]}, {WeekdayNames[(int)date.DayOfWeek]}";

    /// <summary>Например: 29 октября 2026, среда</summary>
    public static string FormatFull(DateOnly date) =>
        $"{date.Day} {MonthNamesGenitive[date.Month - 1]} {date.Year}, {WeekdayNames[(int)date.DayOfWeek]}";

    public static string Key(DateOnly date) => MealRepository.Key(date);

    /// <summary>Сетка месяца: недели с понедельника, включая хвосты соседних месяцев.</summary>
    public async Task<MonthGrid> BuildMonthAsync(int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        var offset = ((int)first.DayOfWeek + 6) % 7; // понедельник = 0
        var start = first.AddDays(-offset);
        var weeksCount = (int)Math.Ceiling((offset + DateTime.DaysInMonth(year, month)) / 7.0);
        var end = start.AddDays(weeksCount * 7 - 1);

        var mains = (await _meals.GetMainRangeAsync(start, end)).ToDictionary(m => m.Date);
        var summary = await _meals.GetRangeSummaryAsync(start, end);
        var today = Today;

        var weeks = new List<CalendarDay[]>(weeksCount);
        for (var w = 0; w < weeksCount; w++)
        {
            var week = new CalendarDay[7];
            for (var d = 0; d < 7; d++)
            {
                var date = start.AddDays(w * 7 + d);
                var key = Key(date);
                mains.TryGetValue(key, out var main);
                summary.TryGetValue(key, out var s);
                week[d] = new CalendarDay(date, date.Month == month, IsWeekend(date), date == today, main, s.Extras, s.HasReceipt);
            }
            weeks.Add(week);
        }

        return new MonthGrid
        {
            Year = year,
            Month = month,
            Title = $"{MonthNames[month - 1]} {year}",
            Weeks = weeks
        };
    }
}
