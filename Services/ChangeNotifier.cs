namespace FoodCalendar.Services;

/// <summary>Singleton-шина: сообщает всем открытым страницам, что данные изменились (голосование, календарь).</summary>
public sealed class ChangeNotifier
{
    public event Func<Task>? Changed;

    public async Task NotifyAsync()
    {
        var handlers = Changed;
        if (handlers is null) return;
        foreach (var h in handlers.GetInvocationList().Cast<Func<Task>>())
        {
            try { await h(); }
            catch { /* отключившийся клиент не должен ломать остальных */ }
        }
    }
}
