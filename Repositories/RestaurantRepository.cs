using Dapper;
using FoodCalendar.Data;
using FoodCalendar.Models;

namespace FoodCalendar.Repositories;

public sealed class RestaurantRepository
{
    private readonly SqliteConnectionFactory _factory;

    public RestaurantRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<List<Restaurant>> GetAllAsync(bool includeInactive)
    {
        using var conn = _factory.Open();
        var sql = includeInactive
            ? "SELECT * FROM restaurants ORDER BY is_active DESC, name COLLATE NOCASE"
            : "SELECT * FROM restaurants WHERE is_active = 1 ORDER BY name COLLATE NOCASE";
        return (await conn.QueryAsync<Restaurant>(sql)).ToList();
    }

    public async Task<Restaurant?> GetByIdAsync(long id)
    {
        using var conn = _factory.Open();
        return await conn.QuerySingleOrDefaultAsync<Restaurant>(
            "SELECT * FROM restaurants WHERE id = @id", new { id });
    }

    public async Task<long> CountAsync()
    {
        using var conn = _factory.Open();
        return await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM restaurants");
    }

    public async Task<long> InsertAsync(string name, string? imageFile)
    {
        using var conn = _factory.Open();
        return await conn.ExecuteScalarAsync<long>(
            "INSERT INTO restaurants (name, image_file, is_active, created_at) VALUES (@name, @imageFile, 1, @now); " +
            "SELECT last_insert_rowid();",
            new { name, imageFile, now = Now() });
    }

    public async Task UpdateAsync(long id, string name, string? imageFile)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync(
            "UPDATE restaurants SET name = @name, image_file = @imageFile WHERE id = @id",
            new { id, name, imageFile });
    }

    public async Task SetActiveAsync(long id, bool isActive)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync(
            "UPDATE restaurants SET is_active = @isActive WHERE id = @id",
            new { id, isActive = isActive ? 1 : 0 });
    }

    internal static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}
