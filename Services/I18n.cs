namespace FoodCalendar.Services;

/// <summary>
/// Язык интерфейса (ru / en / uz). Scoped: один на цепь Blazor; выставляется из cookie в Routes.razor.
/// Пользовательские данные (названия ресторанов, позиции, метки, имена) не переводятся.
/// </summary>
public sealed class I18n
{
    public const string CookieName = "fc.lang";
    public static readonly string[] Languages = { "ru", "en", "uz" };
    public static readonly (string Code, string Title)[] LanguageTitles =
    {
        ("ru", "Русский"), ("en", "English"), ("uz", "O‘zbekcha")
    };

    public string Lang { get; private set; } = "ru";

    private int Index => Lang switch { "en" => 1, "uz" => 2, _ => 0 };

    public static string Normalize(string? lang) =>
        lang is not null && Languages.Contains(lang) ? lang : "ru";

    public void Set(string? lang) => Lang = Normalize(lang);

    /// <summary>Перевод по ключу; неизвестный ключ возвращается как есть (видно в интерфейсе).</summary>
    public string this[string key] => Texts.Map.TryGetValue(key, out var v) ? v[Index] : key;

    public string F(string key, params object?[] args) => string.Format(this[key], args);

    /// <summary>Текст ошибки проверки на текущем языке.</summary>
    public string Error(ValidationException ex) => Texts.Map.ContainsKey(ex.Key) ? F(ex.Key, ex.Args) : ex.Message;

    /// <summary>Форма слова для числа: формы в переводе разделены «|» (ru: 1|2-4|5, en: 1|много, uz: одна).</summary>
    public string Plural(string key, int n)
    {
        var forms = this[key].Split('|');
        if (forms.Length == 1) return forms[0];
        if (Lang == "ru")
        {
            var m10 = n % 10; var m100 = n % 100;
            if (m10 == 1 && m100 != 11) return forms[0];
            if (m10 is >= 2 and <= 4 && m100 is < 12 or > 14) return forms[1];
            return forms[^1];
        }
        return n == 1 ? forms[0] : forms[^1];
    }

    // ---- даты ----

    private string[] Months => this["months"].Split('|');
    private string[] MonthsGenitive => this["months.genitive"].Split('|');
    private string[] Weekdays => this["weekdays"].Split('|');   // с воскресенья, как DayOfWeek

    /// <summary>Заголовки колонок календаря, с понедельника.</summary>
    public string[] WeekdayHeaders => this["weekdays.short"].Split('|');

    public string MonthTitle(int year, int month) => $"{Months[month - 1]} {year}";

    /// <summary>29 октября, четверг / Thursday, October 29 / 29-oktabr, payshanba</summary>
    public string DateLong(DateOnly d) => Lang switch
    {
        "en" => $"{Weekdays[(int)d.DayOfWeek]}, {Months[d.Month - 1]} {d.Day}",
        "uz" => $"{d.Day}-{MonthsGenitive[d.Month - 1]}, {Weekdays[(int)d.DayOfWeek]}",
        _ => $"{d.Day} {MonthsGenitive[d.Month - 1]}, {Weekdays[(int)d.DayOfWeek]}"
    };

    /// <summary>29 октября 2026, четверг / Thursday, October 29, 2026 / 29-oktabr 2026, payshanba</summary>
    public string DateFull(DateOnly d) => Lang switch
    {
        "en" => $"{Weekdays[(int)d.DayOfWeek]}, {Months[d.Month - 1]} {d.Day}, {d.Year}",
        "uz" => $"{d.Day}-{MonthsGenitive[d.Month - 1]} {d.Year}, {Weekdays[(int)d.DayOfWeek]}",
        _ => $"{d.Day} {MonthsGenitive[d.Month - 1]} {d.Year}, {Weekdays[(int)d.DayOfWeek]}"
    };
}
