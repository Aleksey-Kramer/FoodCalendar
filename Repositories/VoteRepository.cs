using Dapper;
using FoodCalendar.Data;
using FoodCalendar.Models;

namespace FoodCalendar.Repositories;

public sealed class VoteRepository
{
    private const string SelectSql =
        "SELECT v.*, r.name AS winner_name, r.image_file AS winner_image_file " +
        "FROM votes v LEFT JOIN restaurants r ON r.id = v.winner_restaurant_id ";

    private readonly SqliteConnectionFactory _factory;

    public VoteRepository(SqliteConnectionFactory factory) => _factory = factory;

    /// <summary>Последнее голосование за дату (открытое или завершённое) со всеми вариантами и участниками.</summary>
    public async Task<Vote?> GetLatestAsync(DateOnly date)
    {
        using var conn = _factory.Open();
        var vote = await conn.QueryFirstOrDefaultAsync<Vote>(
            SelectSql + "WHERE v.date = @date ORDER BY (v.status = 'open') DESC, v.id DESC LIMIT 1",
            new { date = MealRepository.Key(date) });
        if (vote is null) return null;
        await FillAsync(conn, vote);
        return vote;
    }

    /// <summary>Все открытые голосования по возрастанию даты.</summary>
    public async Task<List<Vote>> GetOpenAsync()
    {
        using var conn = _factory.Open();
        var votes = (await conn.QueryAsync<Vote>(SelectSql + "WHERE v.status = 'open' ORDER BY v.date, v.id")).ToList();
        foreach (var v in votes) await FillAsync(conn, v);
        return votes;
    }

    /// <summary>Последние завершённые голосования начиная с даты, по одному на дату.</summary>
    public async Task<List<Vote>> GetFinishedFromAsync(DateOnly from)
    {
        using var conn = _factory.Open();
        var votes = (await conn.QueryAsync<Vote>(
                SelectSql + "WHERE v.status = 'finished' AND v.date >= @from ORDER BY v.date, v.id DESC",
                new { from = MealRepository.Key(from) }))
            .GroupBy(v => v.Date).Select(g => g.First()).ToList();
        foreach (var v in votes) await FillAsync(conn, v);
        return votes;
    }

    /// <summary>Открытые голосования на прошедшие даты отменяются: голосовать в прошлое нельзя.</summary>
    public async Task<int> CancelExpiredAsync(DateOnly before)
    {
        using var conn = _factory.Open();
        return await conn.ExecuteAsync(
            "UPDATE votes SET status = 'cancelled', finished_at = @now WHERE status = 'open' AND date < @before",
            new { before = MealRepository.Key(before), now = RestaurantRepository.Now() });
    }

    public async Task<Vote?> GetByIdAsync(long id)
    {
        using var conn = _factory.Open();
        var vote = await conn.QuerySingleOrDefaultAsync<Vote>(SelectSql + "WHERE v.id = @id", new { id });
        if (vote is null) return null;
        await FillAsync(conn, vote);
        return vote;
    }

    private static async Task FillAsync(Microsoft.Data.Sqlite.SqliteConnection conn, Vote vote)
    {
        vote.Options = (await conn.QueryAsync<VoteOption>(
            "SELECT o.*, r.name AS restaurant_name, r.image_file AS image_file, " +
            "       (SELECT COUNT(*) FROM vote_ballots b WHERE b.option_id = o.id) AS votes " +
            "FROM vote_options o JOIN restaurants r ON r.id = o.restaurant_id WHERE o.vote_id = @id ORDER BY o.id",
            new { id = vote.Id })).ToList();
        vote.Participants = (await conn.QueryAsync<VoteParticipant>(
            "SELECT * FROM vote_participants WHERE vote_id = @id ORDER BY joined_at, id", new { id = vote.Id })).ToList();
        if (vote.Participants.Count > 0)
        {
            var ids = vote.Participants.Select(p => p.Id).ToArray();
            var ballots = await conn.QueryAsync<(long ParticipantId, long OptionId)>(
                "SELECT participant_id, option_id FROM vote_ballots WHERE participant_id IN @ids", new { ids });
            var byP = ballots.ToLookup(b => b.ParticipantId, b => b.OptionId);
            foreach (var p in vote.Participants) p.OptionIds = byP[p.Id].ToList();
        }
    }

    public async Task<long> InsertAsync(DateOnly date, string createdBy)
    {
        using var conn = _factory.Open();
        return await conn.ExecuteScalarAsync<long>(
            "INSERT INTO votes (date, status, round, created_by, created_at) VALUES (@date, 'open', 1, @createdBy, @now); SELECT last_insert_rowid();",
            new { date = MealRepository.Key(date), createdBy, now = RestaurantRepository.Now() });
    }

    public async Task AddOptionAsync(long voteId, long restaurantId, string addedBy)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync(
            "INSERT OR IGNORE INTO vote_options (vote_id, restaurant_id, added_by) VALUES (@voteId, @restaurantId, @addedBy)",
            new { voteId, restaurantId, addedBy });
    }

    public async Task<long> AddParticipantAsync(long voteId, string name)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync(
            "INSERT OR IGNORE INTO vote_participants (vote_id, name, joined_at) VALUES (@voteId, @name, @now)",
            new { voteId, name, now = RestaurantRepository.Now() });
        return await conn.ExecuteScalarAsync<long>(
            "SELECT id FROM vote_participants WHERE vote_id = @voteId AND name = @name", new { voteId, name });
    }

    public async Task SetBallotsAsync(long participantId, IEnumerable<long> optionIds)
    {
        using var conn = _factory.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync("DELETE FROM vote_ballots WHERE participant_id = @participantId", new { participantId }, tx);
        foreach (var optionId in optionIds.Distinct())
            await conn.ExecuteAsync("INSERT INTO vote_ballots (participant_id, option_id) VALUES (@participantId, @optionId)",
                new { participantId, optionId }, tx);
        await conn.ExecuteAsync("UPDATE vote_participants SET has_voted = 1, is_finished = 0 WHERE id = @participantId",
            new { participantId }, tx);
        tx.Commit();
    }

    public async Task SetFinishedAsync(long participantId, bool finished)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync("UPDATE vote_participants SET is_finished = @f WHERE id = @participantId",
            new { participantId, f = finished ? 1 : 0 });
    }

    public async Task CloseAsync(long voteId, string status, long? winnerRestaurantId)
    {
        using var conn = _factory.Open();
        await conn.ExecuteAsync(
            "UPDATE votes SET status = @status, winner_restaurant_id = @winnerRestaurantId, finished_at = @now WHERE id = @voteId",
            new { voteId, status, winnerRestaurantId, now = RestaurantRepository.Now() });
    }

    /// <summary>Новый раунд: остаются только указанные варианты, голоса и статусы участников сбрасываются.</summary>
    public async Task StartNewRoundAsync(long voteId, long[] keepOptionIds, string tieNames)
    {
        using var conn = _factory.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync("DELETE FROM vote_options WHERE vote_id = @voteId AND id NOT IN @keepOptionIds", new { voteId, keepOptionIds }, tx);
        await conn.ExecuteAsync("DELETE FROM vote_ballots WHERE participant_id IN (SELECT id FROM vote_participants WHERE vote_id = @voteId)", new { voteId }, tx);
        await conn.ExecuteAsync("UPDATE vote_participants SET has_voted = 0, is_finished = 0 WHERE vote_id = @voteId", new { voteId }, tx);
        await conn.ExecuteAsync("UPDATE votes SET round = round + 1, tie_names = @tieNames WHERE id = @voteId", new { voteId, tieNames }, tx);
        tx.Commit();
    }
}
