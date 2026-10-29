using FoodCalendar.Models;
using FoodCalendar.Repositories;

namespace FoodCalendar.Services;

/// <summary>Открытые голосования и свежие результаты для панели на главной.</summary>
public sealed class VoteBoard
{
    public List<Vote> Open { get; init; } = new();
    public List<Vote> Finished { get; init; } = new();
}

/// <summary>Голосование за основное блюдо на сегодня или будущий рабочий день.</summary>
public sealed class VoteService
{
    private readonly VoteRepository _votes;
    private readonly RestaurantRepository _restaurants;
    private readonly MealService _meals;
    private readonly ChangeNotifier _notifier;

    public VoteService(VoteRepository votes, RestaurantRepository restaurants, MealService meals, ChangeNotifier notifier)
    {
        _votes = votes;
        _restaurants = restaurants;
        _meals = meals;
        _notifier = notifier;
    }

    public Task<Vote?> GetTodayAsync() => GetLatestAsync(CalendarService.Today);

    public Task<Vote?> GetLatestAsync(DateOnly date) => _votes.GetLatestAsync(date);

    /// <summary>Все открытые голосования (прошедшие даты закрываются) и результаты начиная с сегодня.</summary>
    public async Task<VoteBoard> GetBoardAsync()
    {
        var today = CalendarService.Today;
        await _votes.CancelExpiredAsync(today);
        return new VoteBoard { Open = await _votes.GetOpenAsync(), Finished = await _votes.GetFinishedFromAsync(today) };
    }

    /// <summary>Ближайший день, на который можно голосовать: сегодня или следующий рабочий.</summary>
    public static DateOnly DefaultDate()
    {
        var d = CalendarService.Today;
        while (CalendarService.IsWeekend(d)) d = d.AddDays(1);
        return d;
    }

    /// <summary>Начинает голосование на дату (по умолчанию сегодня). В прошлое и на выходные нельзя.</summary>
    public async Task<long> StartAsync(string rawName, IReadOnlyCollection<long> restaurantIds, DateOnly? forDate = null)
    {
        var name = PeopleService.NormalizeName(rawName);
        var date = forDate ?? CalendarService.Today;
        if (date < CalendarService.Today)
            throw new ValidationException("err.votePast");
        if (CalendarService.IsWeekend(date))
            throw new ValidationException("err.voteWeekend");
        if (restaurantIds.Distinct().Count() < 2)
            throw new ValidationException("err.voteTwo");
        var existing = await _votes.GetLatestAsync(date);
        if (existing is { IsOpen: true })
            throw new ValidationException("err.voteExists");
        foreach (var id in restaurantIds.Distinct())
            await EnsureActiveAsync(id);

        var voteId = await _votes.InsertAsync(date, name);
        foreach (var id in restaurantIds.Distinct())
            await _votes.AddOptionAsync(voteId, id, name);
        await _votes.AddParticipantAsync(voteId, name);
        await _notifier.NotifyAsync();
        return voteId;
    }

    public async Task AddOptionAsync(long voteId, string rawName, long restaurantId)
    {
        var name = PeopleService.NormalizeName(rawName);
        var vote = await GetOpenAsync(voteId);
        await EnsureActiveAsync(restaurantId);
        if (vote.Options.Any(o => o.RestaurantId == restaurantId))
            throw new ValidationException("err.voteOptionExists");
        await _votes.AddOptionAsync(voteId, restaurantId, name);
        await _votes.AddParticipantAsync(voteId, name);
        await _notifier.NotifyAsync();
    }

    /// <summary>Сохраняет выбор участника. Повторное голосование снимает отметку «завершил».</summary>
    public async Task CastAsync(long voteId, string rawName, IReadOnlyCollection<long> optionIds)
    {
        var name = PeopleService.NormalizeName(rawName);
        var vote = await GetOpenAsync(voteId);
        var chosen = optionIds.Distinct().Where(id => vote.Options.Any(o => o.Id == id)).ToList();
        if (chosen.Count == 0)
            throw new ValidationException("err.voteOneOption");
        var pid = await _votes.AddParticipantAsync(voteId, name);
        await _votes.SetBallotsAsync(pid, chosen);
        await _notifier.NotifyAsync();
    }

    /// <summary>Участник завершил голосование. Когда завершили все, подводится итог.</summary>
    public async Task FinishAsync(long voteId, string rawName)
    {
        var name = PeopleService.NormalizeName(rawName);
        var vote = await GetOpenAsync(voteId);
        var me = vote.Participants.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            ?? throw new ValidationException("err.voteFirst");
        if (!me.HasVoted)
            throw new ValidationException("err.votePress");
        await _votes.SetFinishedAsync(me.Id, true);
        await TryCompleteAsync(voteId);
        await _notifier.NotifyAsync();
    }

    public async Task CancelAsync(long voteId)
    {
        _ = await GetOpenAsync(voteId);
        await _votes.CloseAsync(voteId, "cancelled", null);
        await _notifier.NotifyAsync();
    }

    /// <summary>Подсчёт только когда все участники завершили. Победитель попадает в календарь, ничья даёт новый раунд.</summary>
    private async Task TryCompleteAsync(long voteId)
    {
        var vote = await _votes.GetByIdAsync(voteId);
        if (vote is null || !vote.IsOpen) return;
        if (vote.Participants.Count == 0 || vote.Participants.Any(p => !p.HasVoted || !p.IsFinished)) return;

        var max = vote.Options.Max(o => o.Votes);
        var winners = vote.Options.Where(o => o.Votes == max).ToList();
        if (winners.Count == 1)
        {
            var winner = winners[0];
            await _votes.CloseAsync(voteId, "finished", winner.RestaurantId);
            await _meals.AssignMainAsync(DateOnly.ParseExact(vote.Date, "yyyy-MM-dd"), winner.RestaurantId);
        }
        else
        {
            var tieNames = string.Join(", ", winners.Select(w => w.RestaurantName));
            await _votes.StartNewRoundAsync(voteId, winners.Select(w => w.Id).ToArray(), tieNames);
        }
    }

    private async Task<Vote> GetOpenAsync(long voteId)
    {
        var vote = await _votes.GetByIdAsync(voteId) ?? throw new ValidationException("err.voteNotFound");
        if (!vote.IsOpen) throw new ValidationException("err.voteClosed");
        return vote;
    }

    private async Task EnsureActiveAsync(long restaurantId)
    {
        var r = await _restaurants.GetByIdAsync(restaurantId) ?? throw new ValidationException("err.restaurantNotFound");
        if (!r.IsActive) throw new ValidationException("err.voteRestaurantInactive", r.Name);
    }
}
