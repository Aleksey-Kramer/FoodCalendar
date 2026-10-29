namespace FoodCalendar.Services;

/// <summary>
/// Ошибка проверки данных. Хранит ключ перевода и аргументы; Message — русский текст (для логов и тестов),
/// интерфейс показывает перевод через I18n.Error(ex).
/// </summary>
public sealed class ValidationException : Exception
{
    public string Key { get; }
    public object?[] Args { get; }

    public ValidationException(string key, params object?[] args)
        : base(Texts.Map.TryGetValue(key, out var v) ? string.Format(v[0], args) : key)
    {
        Key = key;
        Args = args;
    }
}
