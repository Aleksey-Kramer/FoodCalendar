using System.Globalization;

namespace FoodCalendar.Services;

/// <summary>Форматирование сумм: 12 500 или 12 500,5 (без валюты).</summary>
public static class Money
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public static string Format(decimal? value) =>
        value is null ? "—" : value.Value.ToString("#,0.##", Ru);
}
