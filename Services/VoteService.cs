using FoodCalendar.Models;
using FoodCalendar.Repositories;

namespace FoodCalendar.Services;

/// <summary>Голосование за основное блюдо на сегодня.</summary>
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

    public Task<Vote?> GetTodayAsync() => _votes.GetLatestAsync(CalendarService.Today);

    public async Task<long> StartAsync(string rawName, IReadOnlyCollection<long> restaurantIds)
    {
        var name = PeopleService.NormalizeName(rawName);
        var date = CalendarService.Today;
        if (CalendarService.IsWeekend(date))
            throw new ValidationException("Сегодня выходной, голосование не нужно.");
        if (restaurantIds.Distinct().Count() < 2)
            throw new ValidationException("Выберите хотя бы два ресторана.");
        var existing = await _votes.GetLatestAsync(date);
        if (existing is { IsOpen: true })
            throw new ValidationException("Голосование на сегодня уже идёт, присоединяйтесь.");
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
            throw new ValidationException("Этот ресторан уже есть в списке.");
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
            throw new ValidationException("Отметьте хотя бы один вариант.");
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
            ?? throw new ValidationException("Сначала проголосуйте.");
        if (!me.HasVoted)
            throw new ValidationException("Сначала нажмите «Голосовать».");
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
        var vote = await _votes.GetByIdAsync(voteId) ?? throw new ValidationException("Голосование не найдено.");
        if (!vote.IsOpen) throw new ValidationException("Голосование уже завершено.");
        return vote;
    }

    private async Task EnsureActiveAsync(long restaurantId)
    {
        var r = await _restaurants.GetByIdAsync(restaurantId) ?? throw new ValidationException("Ресторан не найден.");
        if (!r.IsActive) throw new ValidationException($"Ресторан «{r.Name}» деактивирован.");
    }
}
