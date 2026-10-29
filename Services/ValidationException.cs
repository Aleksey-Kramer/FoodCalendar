namespace FoodCalendar.Services;

/// <summary>Ошибка, текст которой можно показать пользователю как есть.</summary>
public sealed class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
