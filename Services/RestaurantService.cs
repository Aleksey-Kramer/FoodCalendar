using System.Text.RegularExpressions;
using FoodCalendar.Models;
using FoodCalendar.Repositories;
using Microsoft.Data.Sqlite;

namespace FoodCalendar.Services;

public sealed class RestaurantService
{
    public const int MaxNameLength = 100;

    private readonly RestaurantRepository _repo;
    private readonly RestaurantItemRepository _items;

    public RestaurantService(RestaurantRepository repo, RestaurantItemRepository items)
    {
        _repo = repo;
        _items = items;
    }

    public Task<List<Restaurant>> GetActiveAsync() => _repo.GetAllAsync(includeInactive: false);

    public Task<List<Restaurant>> GetAllAsync() => _repo.GetAllAsync(includeInactive: true);

    public Task<Restaurant?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);

    public async Task<long> CreateAsync(string name, string? imageFile)
    {
        name = await ValidateNameAsync(name, excludeId: null);
        try
        {
            return await _repo.InsertAsync(name, imageFile);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // constraint violation
        {
            throw new ValidationException("err.restaurantExists");
        }
    }

    public async Task UpdateAsync(long id, string name, string? imageFile)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new ValidationException("err.restaurantNotFound");
        name = await ValidateNameAsync(name, excludeId: id);
        try
        {
            await _repo.UpdateAsync(id, name, imageFile);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new ValidationException("err.restaurantExists");
        }
    }

    /// <summary>Soft delete: ресторан остаётся в БД и в прошлых назначениях, но не предлагается к выбору.</summary>
    public Task DeactivateAsync(long id) => _repo.SetActiveAsync(id, false);

    public Task ActivateAsync(long id) => _repo.SetActiveAsync(id, true);

    // ---- меню ресторана ----

    public Task<List<RestaurantItem>> GetItemsAsync(long restaurantId) => _items.GetByRestaurantAsync(restaurantId);

    public Task<Dictionary<long, int>> GetItemCountsAsync() => _items.CountByRestaurantAsync();

    /// <summary>Добавляет позицию меню или обновляет цену, если такая уже есть.</summary>
    public async Task<long> SaveItemAsync(long restaurantId, string rawName, decimal? price)
    {
        var name = MealService.NormalizeItemName(rawName);
        if (price is < 0) throw new ValidationException("err.priceNegative");
        _ = await _repo.GetByIdAsync(restaurantId) ?? throw new ValidationException("err.restaurantNotFound");
        return await _items.UpsertAsync(restaurantId, name, price);
    }

    public async Task UpdateItemAsync(long itemId, string rawName, decimal? price)
    {
        var name = MealService.NormalizeItemName(rawName);
        if (price is < 0) throw new ValidationException("err.priceNegative");
        try
        {
            await _items.UpdateAsync(itemId, name, price);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new ValidationException("err.menuItemExists");
        }
    }

    public Task DeleteItemAsync(long itemId) => _items.DeleteAsync(itemId);

    private async Task<string> ValidateNameAsync(string? raw, long? excludeId)
    {
        var name = Regex.Replace((raw ?? "").Trim(), @"\s+", " ");
        if (name.Length == 0)
            throw new ValidationException("err.restaurantNameRequired");
        if (name.Length > MaxNameLength)
            throw new ValidationException("err.nameTooLong", MaxNameLength);

        // Сравниваем в C#: COLLATE NOCASE в SQLite не учитывает регистр кириллицы.
        var all = await _repo.GetAllAsync(includeInactive: true);
        if (all.Any(r => r.Id != excludeId && string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("err.restaurantExists");

        return name;
    }
}
