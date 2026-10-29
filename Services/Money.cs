using System.Globalization;

namespace FoodCalendar.Services;

/// <summary>Форматирование сумм: 12 500 или 12 500,5 (без валюты).</summary>
public static class Money
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public static string Format(decimal? value) =>
        value is null ? "—" : value.Value.ToString("#,0.##", Ru);

    /// <summary>Текст для поля ввода: разряды через обычный пробел, дробная часть через запятую; пусто для null.</summary>
    public static string FormatInput(decimal? value) =>
        value is null ? "" : value.Value.ToString("#,0.##", Ru).Replace(' ', ' ');

    /// <summary>Разбор введённой суммы: пробелы между разрядами игнорируются, допустимы «,» и «.».</summary>
    public static decimal? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var clean = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray()).Replace(',', '.');
        return decimal.TryParse(clean, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var v)
            ? Math.Round(v, 2)
            : null;
    }
}
