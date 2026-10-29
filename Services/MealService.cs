using System.Text.RegularExpressions;
using FoodCalendar.Models;
using FoodCalendar.Repositories;
using Microsoft.AspNetCore.Components.Forms;

namespace FoodCalendar.Services;

/// <summary>Все приёмы еды за день: основное блюдо и дополнительные.</summary>
public sealed class DayDetails
{
    public DateOnly Date { get; init; }
    public Meal? Main { get; init; }
    public List<Meal> Extras { get; init; } = new();
    public IEnumerable<Meal> All => Main is null ? Extras : Extras.Prepend(Main);
}

public sealed class MealService
{
    public static readonly string[] LabelSuggestions = { "Завтрак", "Перекус", "Кофе", "Обед", "Ужин", "Десерт" };
    public const int MaxLabelLength = 40;
    public const int MaxItemLength = 100;

    private readonly MealRepository _meals;
    private readonly RestaurantRepository _restaurants;
    private readonly RestaurantItemRepository _items;
    private readonly PeopleService _people;
    private readonly ReceiptService _receipts;

    public MealService(MealRepository meals, RestaurantRepository restaurants, RestaurantItemRepository items,
        PeopleService people, ReceiptService receipts)
    {
        _meals = meals;
        _restaurants = restaurants;
        _items = items;
        _people = people;
        _receipts = receipts;
    }

    public async Task<DayDetails> GetDayAsync(DateOnly date)
    {
        var meals = await _meals.GetDayAsync(date);
        return new DayDetails
        {
            Date = date,
            Main = meals.FirstOrDefault(m => m.IsMain),
            Extras = meals.Where(m => !m.IsMain).ToList()
        };
    }

    /// <summary>Назначает основное блюдо дня (создаёт или меняет ресторан).</summary>
    public async Task AssignMainAsync(DateOnly date, long restaurantId)
    {
        EnsureWorkday(date);
        await EnsureActiveRestaurantAsync(restaurantId);
        var main = await _meals.GetMainAsync(date);
        if (main is null)
            await _meals.InsertAsync(date, restaurantId, isMain: true, label: null);
        else
            await _meals.SetRestaurantAsync(main.Id, restaurantId);
    }

    /// <summary>Убирает основное блюдо вместе с участниками, позициями и чеком.</summary>
    public async Task ClearMainAsync(DateOnly date)
    {
        var main = await _meals.GetMainAsync(date);
        if (main is not null) await DeleteMealAsync(main.Id);
    }

    public async Task<long> AddExtraAsync(DateOnly date, long restaurantId, string? label)
    {
        EnsureWorkday(date);
        await EnsureActiveRestaurantAsync(restaurantId);
        return await _meals.InsertAsync(date, restaurantId, isMain: false, label: NormalizeLabel(label));
    }

    public async Task ChangeRestaurantAsync(long mealId, long restaurantId)
    {
        await EnsureActiveRestaurantAsync(restaurantId);
        await _meals.SetRestaurantAsync(mealId, restaurantId);
    }

    public Task SetLabelAsync(long mealId, string? label) => _meals.SetLabelAsync(mealId, NormalizeLabel(label));

    public async Task DeleteMealAsync(long mealId)
    {
        var meal = await _meals.GetByIdAsync(mealId);
        if (meal is null) return;
        await _meals.DeleteAsync(mealId);
        _receipts.Delete(meal.ReceiptFile);
    }

    public Task SetTotalAsync(long mealId, decimal? total)
    {
        if (total is < 0) throw new ValidationException("Сумма не может быть отрицательной.");
        return _meals.SetTotalAsync(mealId, total);
    }

    public async Task AttachReceiptAsync(long mealId, IBrowserFile file)
    {
        var meal = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("Приём еды не найден.");
        var fileName = await _receipts.SaveAsync(file);
        await _meals.SetReceiptAsync(mealId, fileName);
        _receipts.Delete(meal.ReceiptFile);
    }

    public async Task RemoveReceiptAsync(long mealId)
    {
        var meal = await _meals.GetByIdAsync(mealId);
        if (meal is null) return;
        await _meals.SetReceiptAsync(mealId, null);
        _receipts.Delete(meal.ReceiptFile);
    }

    // ---- участники ----

    /// <summary>Добавляет участника по имени; при saveToList имя попадает в справочник людей.</summary>
    public async Task<long> AddParticipantAsync(long mealId, string rawName, bool saveToList)
    {
        var name = PeopleService.NormalizeName(rawName);
        var meal = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("Приём еды не найден.");
        var person = saveToList ? await _people.GetOrCreateAsync(name) : await _people.FindAsync(name);
        return await _meals.AddParticipantAsync(meal.Id, person?.Id, person?.Name ?? name);
    }

    public Task RemoveParticipantAsync(long participantId) => _meals.DeleteParticipantAsync(participantId);

    // ---- позиции ----

    /// <summary>Добавляет позицию участнику; при saveToMenu позиция с ценой сохраняется в меню ресторана.</summary>
    public async Task<long> AddItemAsync(long participantId, string rawName, decimal? price, int qty, bool saveToMenu)
    {
        var name = NormalizeItemName(rawName);
        ValidatePriceQty(price, qty);
        var participant = await _meals.GetParticipantAsync(participantId)
            ?? throw new ValidationException("Участник не найден.");
        if (saveToMenu)
        {
            var meal = await _meals.GetByIdAsync(participant.MealId) ?? throw new ValidationException("Приём еды не найден.");
            await _items.UpsertAsync(meal.RestaurantId, name, price);
        }
        return await _meals.AddItemAsync(participantId, name, price, qty);
    }

    public Task UpdateItemAsync(long itemId, string rawName, decimal? price, int qty)
    {
        var name = NormalizeItemName(rawName);
        ValidatePriceQty(price, qty);
        return _meals.UpdateItemAsync(itemId, name, price, qty);
    }

    public Task RemoveItemAsync(long itemId) => _meals.DeleteItemAsync(itemId);

    // ---- общие позиции и сборы ----

    public static readonly (string Kind, string Title)[] SharedKinds =
    {
        ("item", "Общая позиция"),
        ("delivery", "Доставка"),
        ("service", "Работа сервиса"),
    };

    /// <summary>Общая позиция или сбор; делится на всех (splitAll) или на выбранных участников.</summary>
    public async Task<long> AddSharedItemAsync(long mealId, string rawName, string kind, decimal? price, int qty, bool splitAll, IReadOnlyCollection<long> participantIds)
    {
        var name = NormalizeItemName(rawName);
        ValidatePriceQty(price, qty);
        if (SharedKinds.All(k => k.Kind != kind))
            throw new ValidationException("Неизвестный тип общей позиции.");
        var meal = (await _meals.GetDayAsync(DateOnly.ParseExact((await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("Приём еды не найден.")).Date, "yyyy-MM-dd")))
            .First(m => m.Id == mealId);
        if (!splitAll)
        {
            var valid = participantIds.Distinct().Where(id => meal.Participants.Any(p => p.Id == id)).ToList();
            if (valid.Count == 0)
                throw new ValidationException("Выберите хотя бы одного участника или делите на всех.");
            participantIds = valid;
        }
        return await _meals.AddSharedItemAsync(mealId, name, kind, price, qty, splitAll, participantIds);
    }

    public Task RemoveSharedItemAsync(long id) => _meals.DeleteSharedItemAsync(id);

    // ---- вспомогательное ----

    private static void EnsureWorkday(DateOnly date)
    {
        if (CalendarService.IsWeekend(date))
            throw new ValidationException("На выходные еду не заказываем: назначение возможно только на будние дни.");
    }

    private async Task EnsureActiveRestaurantAsync(long restaurantId)
    {
        var restaurant = await _restaurants.GetByIdAsync(restaurantId)
            ?? throw new ValidationException("Ресторан не найден.");
        if (!restaurant.IsActive)
            throw new ValidationException("Этот ресторан деактивирован, выберите другой.");
    }

    private static string? NormalizeLabel(string? raw)
    {
        var label = Regex.Replace((raw ?? "").Trim(), @"\s+", " ");
        if (label.Length == 0)
            throw new ValidationException("Укажите метку: завтрак, перекус, кофе и т.п.");
        if (label.Length > MaxLabelLength)
            throw new ValidationException($"Метка не должна быть длиннее {MaxLabelLength} символов.");
        return label;
    }

    public static string NormalizeItemName(string? raw)
    {
        var name = Regex.Replace((raw ?? "").Trim(), @"\s+", " ");
        if (name.Length == 0)
            throw new ValidationException("Введите название позиции.");
        if (name.Length > MaxItemLength)
            throw new ValidationException($"Название не должно быть длиннее {MaxItemLength} символов.");
        return name;
    }

    public static void ValidatePriceQty(decimal? price, int qty)
    {
        if (price is < 0)
            throw new ValidationException("Цена не может быть отрицательной.");
        if (qty < 1 || qty > 99)
            throw new ValidationException("Количество от 1 до 99.");
    }
}
