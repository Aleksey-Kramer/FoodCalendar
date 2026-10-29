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
    /// <summary>Подсказки меток дополнительных приёмов на текущем языке.</summary>
    public string[] LabelSuggestions => _l["day.labels"].Split('|');
    public const int MaxLabelLength = 40;
    public const int MaxItemLength = 100;

    private readonly MealRepository _meals;
    private readonly RestaurantRepository _restaurants;
    private readonly RestaurantItemRepository _items;
    private readonly PeopleService _people;
    private readonly ReceiptService _receipts;
    private readonly I18n _l;

    public MealService(MealRepository meals, RestaurantRepository restaurants, RestaurantItemRepository items,
        PeopleService people, ReceiptService receipts, I18n? l = null)
    {
        _l = l ?? new I18n();
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

    /// <summary>Детали всех дней периода (для подсказок с чеком в календаре), ключ — дата.</summary>
    public async Task<Dictionary<DateOnly, DayDetails>> GetRangeDetailsAsync(DateOnly from, DateOnly to)
    {
        var meals = await _meals.GetRangeAsync(from, to);
        return meals
            .GroupBy(m => DateOnly.ParseExact(m.Date, "yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => new DayDetails
            {
                Date = g.Key,
                Main = g.FirstOrDefault(m => m.IsMain),
                Extras = g.Where(m => !m.IsMain).ToList()
            });
    }

    /// <summary>Назначает основное блюдо дня (создаёт или меняет ресторан).</summary>
    public async Task AssignMainAsync(DateOnly date, long restaurantId)
    {
        EnsureWorkday(date);
        await EnsureActiveRestaurantAsync(restaurantId);
        // скрытое основное (ресторан убирали) восстанавливается вместе с чеком
        var main = await _meals.GetMainAnyAsync(date);
        if (main is null)
            await _meals.InsertAsync(date, restaurantId, isMain: true, label: null);
        else
            await _meals.SetRestaurantAsync(main.Id, restaurantId);
    }

    /// <summary>Убирает основное блюдо. Заполненный чек не удаляется, а скрывается до повторного выбора ресторана.</summary>
    public async Task ClearMainAsync(DateOnly date)
    {
        var main = (await GetDayAsync(date)).Main;
        if (main is null) return;
        if (main.HasReceiptData)
            await _meals.SetHiddenAsync(main.Id, true);
        else
            await DeleteMealAsync(main.Id);
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
        if (total is < 0) throw new ValidationException("err.amountNegative");
        return _meals.SetTotalAsync(mealId, total);
    }

    public async Task AttachReceiptAsync(long mealId, IBrowserFile file)
    {
        var meal = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("err.mealNotFound");
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
        var meal = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("err.mealNotFound");
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
            ?? throw new ValidationException("err.participantNotFound");
        if (saveToMenu)
        {
            var meal = await _meals.GetByIdAsync(participant.MealId) ?? throw new ValidationException("err.mealNotFound");
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

    public static readonly string[] Modes = { "delivery", "dinein" };
    public static readonly string[] SharedKinds = { "item", "delivery", "service", "servicecharge", "tips" };

    /// <summary>Типы общих позиций для формата чека: доставка — «Доставка» и «Работа сервиса», на месте — «Обслуживание» и «Чаевые».</summary>
    public static string[] KindsFor(string mode) =>
        mode == "dinein" ? new[] { "item", "servicecharge", "tips" } : new[] { "item", "delivery", "service" };

    public async Task SetModeAsync(long mealId, string mode)
    {
        if (!Modes.Contains(mode)) throw new ValidationException("err.sharedKind");
        _ = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("err.mealNotFound");
        await _meals.SetModeAsync(mealId, mode);
    }

    /// <summary>Название типа общей позиции на текущем языке.</summary>
    public string SharedKindTitle(string kind) => _l[$"shared.{kind}"];

    /// <summary>Общая позиция или сбор; делится на всех (splitAll) или на выбранных участников.</summary>
    /// <summary>Добавляет общую позицию или сбор. Общая позиция (kind = item) при saveToMenu сохраняется в меню ресторана с ценой за штуку.</summary>
    public async Task<long> AddSharedItemAsync(long mealId, string rawName, string kind, decimal? price, int qty, bool splitAll, IReadOnlyCollection<long> participantIds, bool saveToMenu = false)
    {
        var name = NormalizeItemName(rawName);
        ValidatePriceQty(price, qty);
        if (!SharedKinds.Contains(kind))
            throw new ValidationException("err.sharedKind");
        var meal = (await _meals.GetDayAsync(DateOnly.ParseExact((await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("err.mealNotFound")).Date, "yyyy-MM-dd")))
            .First(m => m.Id == mealId);
        if (!splitAll)
        {
            var valid = participantIds.Distinct().Where(id => meal.Participants.Any(p => p.Id == id)).ToList();
            if (valid.Count == 0)
                throw new ValidationException("err.pickParticipants");
            participantIds = valid;
        }
        if (saveToMenu && kind == "item")
            await _items.UpsertAsync(meal.RestaurantId, name, price);
        return await _meals.AddSharedItemAsync(mealId, name, kind, price, qty, splitAll, participantIds);
    }

    public Task RemoveSharedItemAsync(long id) => _meals.DeleteSharedItemAsync(id);

    // ---- вспомогательное ----

    private static void EnsureWorkday(DateOnly date)
    {
        if (CalendarService.IsWeekend(date))
            throw new ValidationException("err.weekend");
    }

    private async Task EnsureActiveRestaurantAsync(long restaurantId)
    {
        var restaurant = await _restaurants.GetByIdAsync(restaurantId)
            ?? throw new ValidationException("err.restaurantNotFound");
        if (!restaurant.IsActive)
            throw new ValidationException("err.restaurantInactive");
    }

    private static string? NormalizeLabel(string? raw)
    {
        var label = Regex.Replace((raw ?? "").Trim(), @"\s+", " ");
        if (label.Length == 0)
            throw new ValidationException("err.labelRequired");
        if (label.Length > MaxLabelLength)
            throw new ValidationException("err.labelTooLong", MaxLabelLength);
        return label;
    }

    public static string NormalizeItemName(string? raw)
    {
        var name = Regex.Replace((raw ?? "").Trim(), @"\s+", " ");
        if (name.Length == 0)
            throw new ValidationException("err.itemRequired");
        if (name.Length > MaxItemLength)
            throw new ValidationException("err.nameTooLong", MaxItemLength);
        return name;
    }

    public static void ValidatePriceQty(decimal? price, int qty)
    {
        if (price is < 0)
            throw new ValidationException("err.priceNegative");
        if (qty < 1 || qty > 99)
            throw new ValidationException("err.qtyRange");
    }
}
