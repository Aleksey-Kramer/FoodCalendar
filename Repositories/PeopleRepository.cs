using Dapper;
using FoodCalendar.Data;
using FoodCalendar.Models;

namespace FoodCalendar.Repositories;

public sealed class PeopleRepository
{
    private readonly SqliteConnectionFactory _factory;

    public PeopleRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<List<Person>> GetAllAsync()
    {
        using var conn = _factory.Open();
        return (await conn.QueryAsync<Person>("SELECT * FROM people ORDER BY name COLLATE NOCASE")).ToList();
    }

    public async Task<Person?> FindByNameAsync(string name)
    {
        using var conn = _factory.Open();
        var all = await conn.QueryAsync<Person>("SELECT * FROM people");
        return all.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<long> InsertAsync(string name)
    {
        using var conn = _factory.Open();
        return await conn.ExecuteScalarAsync<long>(
            "INSERT INTO people (name, created_at) VALUES (@name, @now); SELECT last_insert_rowid();",
            new { name, now = RestaurantRepository.Now() });
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("DELETE FROM people WHERE id = @id", new { id });
    }
}
