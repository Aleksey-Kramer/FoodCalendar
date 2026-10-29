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
    private readonly AdminSession _admin;

    public MealService(MealRepository meals, RestaurantRepository restaurants, RestaurantItemRepository items,
        PeopleService people, ReceiptService receipts, I18n? l = null, AdminSession? admin = null)
    {
        _l = l ?? new I18n();
        _admin = admin ?? AdminSession.Trusted; // без DI (тесты, скрипты) проверок прав нет
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

    /// <summary>Назначает основное блюдо дня. Чек прежнего ресторана скрывается, чек нового (если был) возвращается.</summary>
    public Task AssignMainAsync(DateOnly date, long restaurantId)
    {
        _admin.Require();
        return AssignMainCoreAsync(date, restaurantId);
    }

    /// <summary>Итог голосования: основное ставится от имени системы, без прав администратора.</summary>
    internal Task AssignMainFromVoteAsync(DateOnly date, long restaurantId) => AssignMainCoreAsync(date, restaurantId);

    private async Task AssignMainCoreAsync(DateOnly date, long restaurantId)
    {
        EnsureWorkday(date);
        await EnsureActiveRestaurantAsync(restaurantId);
        var main = (await GetDayAsync(date)).Main;
        if (main?.RestaurantId == restaurantId) return;
        if (main is not null) await HideOrDeleteAsync(main);
        await ShowAsync(date, restaurantId, isMain: true, label: null);
    }

    /// <summary>Убирает основное блюдо. Заполненный чек не удаляется, а скрывается: вернётся, когда этот ресторан снова выберут в этот день.</summary>
    public async Task ClearMainAsync(DateOnly date)
    {
        _admin.Require();
        var main = (await GetDayAsync(date)).Main;
        if (main is not null) await HideOrDeleteAsync(main);
    }

    public async Task<long> AddExtraAsync(DateOnly date, long restaurantId, string? label)
    {
        _admin.Require();
        EnsureWorkday(date);
        await EnsureActiveRestaurantAsync(restaurantId);
        return await ShowAsync(date, restaurantId, isMain: false, label: NormalizeLabel(label));
    }

    /// <summary>Смена ресторана у приёма: чек старого ресторана скрывается, у нового — возвращается его собственный чек или создаётся пустой.</summary>
    public async Task ChangeRestaurantAsync(long mealId, long restaurantId)
    {
        _admin.Require();
        await EnsureActiveRestaurantAsync(restaurantId);
        var meal = await GetFullAsync(mealId);
        if (meal.RestaurantId == restaurantId) return;
        await HideOrDeleteAsync(meal);
        await ShowAsync(meal.DateValue, restaurantId, meal.IsMain, meal.Label);
    }

    /// <summary>Приём с данными чека скрывается (чек сохраняется за рестораном в этом дне), пустой — удаляется.</summary>
    private async Task HideOrDeleteAsync(Meal meal)
    {
        if (meal.HasReceiptData)
            await _meals.SetHiddenAsync(meal.Id, true);
        else
            await DeleteCoreAsync(meal.Id);
    }

    /// <summary>Показывает ресторан в дне в нужной роли: возвращает его скрытый чек или создаёт новый приём.</summary>
    private async Task<long> ShowAsync(DateOnly date, long restaurantId, bool isMain, string? label)
    {
        var hidden = await _meals.FindHiddenAsync(date, restaurantId);
        if (hidden is null)
            return await _meals.InsertAsync(date, restaurantId, isMain, label);
        await _meals.RestoreAsync(hidden.Id, isMain, label);
        return hidden.Id;
    }

    /// <summary>Приём с участниками, позициями и сборами.</summary>
    private async Task<Meal> GetFullAsync(long mealId)
    {
        var brief = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("err.mealNotFound");
        return (await _meals.GetDayAsync(brief.DateValue)).FirstOrDefault(m => m.Id == mealId)
            ?? throw new ValidationException("err.mealNotFound");
    }

    public Task SetLabelAsync(long mealId, string? label)
    {
        _admin.Require();
        return _meals.SetLabelAsync(mealId, NormalizeLabel(label));
    }

    public Task DeleteMealAsync(long mealId)
    {
        _admin.Require();
        return DeleteCoreAsync(mealId);
    }

    private async Task DeleteCoreAsync(long mealId)
    {
        var meal = await _meals.GetByIdAsync(mealId);
        if (meal is null) return;
        await _meals.DeleteAsync(mealId);
        _receipts.Delete(meal.ReceiptFile);
    }

    public Task SetTotalAsync(long mealId, decimal? total)
    {
        _admin.Require();
        if (total is < 0) throw new ValidationException("err.amountNegative");
        return _meals.SetTotalAsync(mealId, total);
    }

    public async Task AttachReceiptAsync(long mealId, IBrowserFile file)
    {
        _admin.Require();
        var meal = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("err.mealNotFound");
        var fileName = await _receipts.SaveAsync(file);
        await _meals.SetReceiptAsync(mealId, fileName);
        _receipts.Delete(meal.ReceiptFile);
    }

    public async Task RemoveReceiptAsync(long mealId)
    {
        _admin.Require();
        var meal = await _meals.GetByIdAsync(mealId);
        if (meal is null) return;
        await _meals.SetReceiptAsync(mealId, null);
        _receipts.Delete(meal.ReceiptFile);
    }

    // ---- участники ----

    /// <summary>Добавляет участника по имени; при saveToList имя попадает в справочник людей.</summary>
    public async Task<long> AddParticipantAsync(long mealId, string rawName, bool saveToList)
    {
        _admin.Require();
        var name = PeopleService.NormalizeName(rawName);
        var meal = await _meals.GetByIdAsync(mealId) ?? throw new ValidationException("err.mealNotFound");
        var person = saveToList ? await _people.GetOrCreateAsync(name) : await _people.FindAsync(name);
        return await _meals.AddParticipantAsync(meal.Id, person?.Id, person?.Name ?? name);
    }

    public Task RemoveParticipantAsync(long participantId)
    {
        _admin.Require();
        return _meals.DeleteParticipantAsync(participantId);
    }

    /// <summary>Отметка «оплатил» / «ожидает оплаты» у участника.</summary>
    public async Task SetParticipantPaidAsync(long participantId, bool paid)
    {
        _admin.Require();
        _ = await _meals.GetParticipantAsync(participantId) ?? throw new ValidationException("err.participantNotFound");
        await _meals.SetParticipantPaidAsync(participantId, paid);
    }

    // ---- позиции ----

    /// <summary>Добавляет позицию участнику; при saveToMenu позиция с ценой сохраняется в меню ресторана.</summary>
    public async Task<long> AddItemAsync(long participantId, string rawName, decimal? price, int qty, bool saveToMenu)
    {
        _admin.Require();
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
        _admin.Require();
        var name = NormalizeItemName(rawName);
        ValidatePriceQty(price, qty);
        return _meals.UpdateItemAsync(itemId, name, price, qty);
    }

    public Task RemoveItemAsync(long itemId)
    {
        _admin.Require();
        return _meals.DeleteItemAsync(itemId);
    }

    // ---- общие позиции и сборы ----

    public static readonly string[] Modes = { "delivery", "dinein" };
    public static readonly string[] SharedKinds = { "item", "delivery", "service", "servicecharge", "tips" };

    /// <summary>Типы общих позиций для формата чека: доставка — «Доставка» и «Работа сервиса», на месте — «Обслуживание» и «Чаевые».</summary>
    public static string[] KindsFor(string mode) =>
        mode == "dinein" ? new[] { "item", "servicecharge", "tips" } : new[] { "item", "delivery", "service" };

    public async Task SetModeAsync(long mealId, string mode)
    {
        _admin.Require();
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
        _admin.Require();
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

    public Task RemoveSharedItemAsync(long id)
    {
        _admin.Require();
        return _meals.DeleteSharedItemAsync(id);
    }

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
