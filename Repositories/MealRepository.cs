using Dapper;
using FoodCalendar.Data;
using FoodCalendar.Models;

namespace FoodCalendar.Repositories;

public sealed class MealRepository
{
    private const string SelectSql =
        "SELECT m.id, m.date, m.restaurant_id, m.is_main, m.label, m.receipt_file, m.total_amount, m.updated_at, " +
        "       r.name AS restaurant_name, r.image_file AS image_file, r.is_active AS is_active " +
        "FROM meals m JOIN restaurants r ON r.id = m.restaurant_id ";

    private readonly SqliteConnectionFactory _factory;

    public MealRepository(SqliteConnectionFactory factory) => _factory = factory;

    public static string Key(DateOnly date) => date.ToString("yyyy-MM-dd");

    /// <summary>Основные блюда за период [from, to] (без участников).</summary>
    public async Task<List<Meal>> GetMainRangeAsync(DateOnly from, DateOnly to)
    {
        using var conn = _factory.Open();
        return (await conn.QueryAsync<Meal>(
            SelectSql + "WHERE m.is_main = 1 AND m.date BETWEEN @from AND @to ORDER BY m.date",
            new { from = Key(from), to = Key(to) })).ToList();
    }

    /// <summary>Сводка по дополнительным приёмам и чекам за период: дата -> (кол-во дополнительных, есть ли чек).</summary>
    public async Task<Dictionary<string, (int Extras, bool HasReceipt)>> GetRangeSummaryAsync(DateOnly from, DateOnly to)
    {
        using var conn = _factory.Open();
        var rows = await conn.QueryAsync<(string Date, long Extras, long Receipts)>(
            "SELECT date, SUM(CASE WHEN is_main = 0 THEN 1 ELSE 0 END), SUM(CASE WHEN receipt_file IS NOT NULL THEN 1 ELSE 0 END) " +
            "FROM meals WHERE date BETWEEN @from AND @to GROUP BY date",
            new { from = Key(from), to = Key(to) });
        return rows.ToDictionary(r => r.Date, r => ((int)r.Extras, r.Receipts > 0));
    }

    /// <summary>Все приёмы за день с участниками и позициями.</summary>
    public async Task<List<Meal>> GetDayAsync(DateOnly date)
    {
        using var conn = _factory.Open();
        var meals = (await conn.QueryAsync<Meal>(
            SelectSql + "WHERE m.date = @date ORDER BY m.is_main DESC, m.id", new { date = Key(date) })).ToList();
        if (meals.Count == 0) return meals;

        var mealIds = meals.Select(m => m.Id).ToArray();
        var participants = (await conn.QueryAsync<MealParticipant>(
            "SELECT * FROM meal_participants WHERE meal_id IN @mealIds ORDER BY sort_order, id", new { mealIds })).ToList();
        if (participants.Count > 0)
        {
            var pIds = participants.Select(p => p.Id).ToArray();
            var items = await conn.QueryAsync<MealItem>(
                "SELECT * FROM meal_items WHERE participant_id IN @pIds ORDER BY id", new { pIds });
            var byParticipant = items.ToLookup(i => i.ParticipantId);
            foreach (var p in participants) p.Items = byParticipant[p.Id].ToList();
        }
        var byMeal = participants.ToLookup(p => p.MealId);
        foreach (var m in meals) m.Participants = byMeal[m.Id].ToList();

        var shared = (await conn.QueryAsync<SharedItem>(
            "SELECT * FROM meal_shared_items WHERE meal_id IN @mealIds ORDER BY id", new { mealIds })).ToList();
        if (shared.Count > 0)
        {
            var sIds = shared.Select(s => s.Id).ToArray();
            var links = await conn.QueryAsync<(long SharedItemId, long ParticipantId)>(
                "SELECT shared_item_id, participant_id FROM meal_shared_item_participants WHERE shared_item_id IN @sIds", new { sIds });
            var byShared = links.ToLookup(l => l.SharedItemId, l => l.ParticipantId);
            foreach (var s in shared) s.ParticipantIds = byShared[s.Id].ToList();
        }
        var sharedByMeal = shared.ToLookup(s => s.MealId);
        foreach (var m in meals) m.SharedItems = sharedByMeal[m.Id].ToList();
        return meals;
    }

    public async Task<Meal?> GetByIdAsync(long id)
    {
        using var conn = _factory.Open();
        return await conn.QuerySingleOrDefaultAsync<Meal>(SelectSql + "WHERE m.id = @id", new { id });
    }

    public async Task<Meal?> GetMainAsync(DateOnly date)
    {
        using var conn = _factory.Open();
        return await conn.QuerySingleOrDefaultAsync<Meal>(
            SelectSql + "WHERE m.date = @date AND m.is_main = 1", new { date = Key(date) });
    }

    public async Task<long> InsertAsync(DateOnly date, long restaurantId, bool isMain, string? label)
    {
        using var conn = _factory.Open();
        var now = RestaurantRepository.Now();
        return await conn.ExecuteScalarAsync<long>(
            "INSERT INTO meals (date, restaurant_id, is_main, label, created_at, updated_at) " +
            "VALUES (@date, @restaurantId, @isMain, @label, @now, @now); SELECT last_insert_rowid();",
            new { date = Key(date), restaurantId, isMain = isMain ? 1 : 0, label, now });
    }

    public async Task SetRestaurantAsync(long id, long restaurantId)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("UPDATE meals SET restaurant_id = @restaurantId, updated_at = @now WHERE id = @id",
            new { id, restaurantId, now = RestaurantRepository.Now() });
    }

    public async Task SetLabelAsync(long id, string? label)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("UPDATE meals SET label = @label, updated_at = @now WHERE id = @id",
            new { id, label, now = RestaurantRepository.Now() });
    }

    public async Task SetTotalAsync(long id, decimal? total)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("UPDATE meals SET total_amount = @total, updated_at = @now WHERE id = @id",
            new { id, total = (double?)total, now = RestaurantRepository.Now() });
    }

    public async Task SetReceiptAsync(long id, string? receiptFile)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("UPDATE meals SET receipt_file = @receiptFile, updated_at = @now WHERE id = @id",
            new { id, receiptFile, now = RestaurantRepository.Now() });
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("DELETE FROM meals WHERE id = @id", new { id });
    }

    public async Task<long> CountAsync()
    {
        using var conn = _factory.Open();
        return await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM meals");
    }

    // ---- участники ----

    public async Task<long> AddParticipantAsync(long mealId, long? personId, string name)
    {
        using var conn = _factory.Open();
        return await conn.ExecuteScalarAsync<long>(
            "INSERT INTO meal_participants (meal_id, person_id, name, sort_order) " +
            "VALUES (@mealId, @personId, @name, (SELECT COALESCE(MAX(sort_order), 0) + 1 FROM meal_participants WHERE meal_id = @mealId)); " +
            "SELECT last_insert_rowid();",
            new { mealId, personId, name });
    }

    public async Task<MealParticipant?> GetParticipantAsync(long id)
    {
        using var conn = _factory.Open();
        return await conn.QuerySingleOrDefaultAsync<MealParticipant>("SELECT * FROM meal_participants WHERE id = @id", new { id });
    }

    public async Task DeleteParticipantAsync(long id)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("DELETE FROM meal_participants WHERE id = @id", new { id });
    }

    // ---- позиции ----

    public async Task<long> AddItemAsync(long participantId, string name, decimal? price, int qty)
    {
        using var conn = _factory.Open();
        return await conn.ExecuteScalarAsync<long>(
            "INSERT INTO meal_items (participant_id, name, price, qty) VALUES (@participantId, @name, @price, @qty); SELECT last_insert_rowid();",
            new { participantId, name, price = (double?)price, qty });
    }

    public async Task UpdateItemAsync(long id, string name, decimal? price, int qty)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("UPDATE meal_items SET name = @name, price = @price, qty = @qty WHERE id = @id",
            new { id, name, price = (double?)price, qty });
    }

    public async Task DeleteItemAsync(long id)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("DELETE FROM meal_items WHERE id = @id", new { id });
    }

    // ---- общие позиции и сборы ----

    public async Task<long> AddSharedItemAsync(long mealId, string name, string kind, decimal? price, int qty, bool splitAll, IEnumerable<long> participantIds)
    {
        using var conn = _factory.Open();
        using var tx = conn.BeginTransaction();
        var id = await conn.ExecuteScalarAsync<long>(
            "INSERT INTO meal_shared_items (meal_id, name, kind, price, qty, split_all) VALUES (@mealId, @name, @kind, @price, @qty, @splitAll); SELECT last_insert_rowid();",
            new { mealId, name, kind, price = (double?)price, qty, splitAll = splitAll ? 1 : 0 }, tx);
        if (!splitAll)
            foreach (var pid in participantIds.Distinct())
                await conn.ExecuteAsync("INSERT INTO meal_shared_item_participants (shared_item_id, participant_id) VALUES (@id, @pid)", new { id, pid }, tx);
        tx.Commit();
        return id;
    }

    public async Task DeleteSharedItemAsync(long id)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("DELETE FROM meal_shared_items WHERE id = @id", new { id });
    }
}
