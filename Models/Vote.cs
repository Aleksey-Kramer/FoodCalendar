namespace FoodCalendar.Models;

public sealed class Vote
{
    public long Id { get; set; }
    public string Date { get; set; } = "";
    /// <summary>open, finished, cancelled</summary>
    public string Status { get; set; } = "open";
    public int Round { get; set; } = 1;
    /// <summary>Названия ресторанов, между которыми была ничья в прошлом раунде.</summary>
    public string? TieNames { get; set; }
    public long? WinnerRestaurantId { get; set; }
    public string CreatedBy { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string? FinishedAt { get; set; }

    public DateOnly DateValue => DateOnly.ParseExact(Date, "yyyy-MM-dd");
    public bool IsOpen => Status == "open";
    public bool IsFinished => Status == "finished";

    // Из JOIN с restaurants (победитель)
    public string? WinnerName { get; set; }
    public string? WinnerImageFile { get; set; }

    public List<VoteOption> Options { get; set; } = new();
    public List<VoteParticipant> Participants { get; set; } = new();

    public int VotedCount => Participants.Count(p => p.HasVoted);
    public int FinishedCount => Participants.Count(p => p.IsFinished);
}

public sealed class VoteOption
{
    public long Id { get; set; }
    public long VoteId { get; set; }
    public long RestaurantId { get; set; }
    public string AddedBy { get; set; } = "";

    // Из JOIN с restaurants
    public string RestaurantName { get; set; } = "";
    public string? ImageFile { get; set; }

    /// <summary>Сколько участников отметили этот вариант.</summary>
    public int Votes { get; set; }
}

public sealed class VoteParticipant
{
    public long Id { get; set; }
    public long VoteId { get; set; }
    public string Name { get; set; } = "";
    public bool HasVoted { get; set; }
    public bool IsFinished { get; set; }
    public string JoinedAt { get; set; } = "";
    public List<long> OptionIds { get; set; } = new();
}
