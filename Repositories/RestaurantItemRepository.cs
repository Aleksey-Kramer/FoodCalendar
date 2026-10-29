using Dapper;
using FoodCalendar.Data;
using FoodCalendar.Models;

namespace FoodCalendar.Repositories;

public sealed class RestaurantItemRepository
{
    private readonly SqliteConnectionFactory _factory;

    public RestaurantItemRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<List<RestaurantItem>> GetByRestaurantAsync(long restaurantId)
    {
        using var conn = _factory.Open();
        return (await conn.QueryAsync<RestaurantItem>(
            "SELECT * FROM restaurant_items WHERE restaurant_id = @restaurantId ORDER BY name COLLATE NOCASE",
            new { restaurantId })).ToList();
    }

    public async Task<Dictionary<long, int>> CountByRestaurantAsync()
    {
        using var conn = _factory.Open();
        var rows = await conn.QueryAsync<(long RestaurantId, long Count)>(
            "SELECT restaurant_id, COUNT(*) FROM restaurant_items GROUP BY restaurant_id");
        return rows.ToDictionary(r => r.RestaurantId, r => (int)r.Count);
    }

    /// <summary>Добавляет позицию или обновляет цену существующей (имя сравнивается в C#, без учёта регистра).</summary>
    public async Task<long> UpsertAsync(long restaurantId, string name, decimal? price)
    {
        var existing = (await GetByRestaurantAsync(restaurantId))
            .FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        using var conn = _factory.Open();
        if (existing is not null)
        {
            await conn.ExecuteAsync("UPDATE restaurant_items SET name = @name, price = @price WHERE id = @id",
                new { id = existing.Id, name, price = (double?)price });
            return existing.Id;
        }
        return await conn.ExecuteScalarAsync<long>(
            "INSERT INTO restaurant_items (restaurant_id, name, price) VALUES (@restaurantId, @name, @price); SELECT last_insert_rowid();",
            new { restaurantId, name, price = (double?)price });
    }

    public async Task UpdateAsync(long id, string name, decimal? price)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("UPDATE restaurant_items SET name = @name, price = @price WHERE id = @id",
            new { id, name, price = (double?)price });
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("DELETE FROM restaurant_items WHERE id = @id", new { id });
    }
}
