namespace FoodCalendar.Models;

/// <summary>Приём еды за день (основное блюдо или дополнительное с меткой) вместе с данными ресторана.</summary>
public sealed class Meal
{
    public long Id { get; set; }
    /// <summary>Дата в формате YYYY-MM-DD.</summary>
    public string Date { get; set; } = "";
    public long RestaurantId { get; set; }
    public bool IsMain { get; set; }
    /// <summary>Метка дополнительного приёма: «Завтрак», «Перекус», «Кофе» и т.п.</summary>
    public string? Label { get; set; }
    public string? ReceiptFile { get; set; }
    /// <summary>Итог по чеку, введённый вручную.</summary>
    public decimal? TotalAmount { get; set; }
    public string UpdatedAt { get; set; } = "";

    // Из JOIN с restaurants
    public string RestaurantName { get; set; } = "";
    public string? ImageFile { get; set; }
    public bool IsActive { get; set; }

    public List<MealParticipant> Participants { get; set; } = new();
    public List<SharedItem> SharedItems { get; set; } = new();

    /// <summary>Сумма личных позиций всех участников.</summary>
    public decimal OwnItemsTotal => Participants.Sum(p => p.Subtotal);

    /// <summary>Сумма общих позиций и сборов.</summary>
    public decimal SharedTotal => SharedItems.Sum(s => s.Amount);

    /// <summary>Сумма всех позиций: личные + общие.</summary>
    public decimal ItemsTotal => OwnItemsTotal + SharedTotal;

    /// <summary>Доля участника в общих позициях и сборах.</summary>
    public decimal ShareFor(long participantId)
    {
        decimal share = 0;
        foreach (var s in SharedItems)
        {
            var members = s.SplitAll ? Participants.Select(p => p.Id).ToList() : s.ParticipantIds;
            if (members.Count == 0 || !members.Contains(participantId)) continue;
            share += s.Amount / members.Count;
        }
        return Math.Round(share, 2);
    }

    /// <summary>Итого для участника: свои позиции + доля общих.</summary>
    public decimal TotalFor(MealParticipant p) => p.Subtotal + ShareFor(p.Id);
}

public sealed class MealParticipant
{
    public long Id { get; set; }
    public long MealId { get; set; }
    public long? PersonId { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public List<MealItem> Items { get; set; } = new();

    public decimal Subtotal => Items.Sum(i => (i.Price ?? 0) * i.Qty);
}

public sealed class MealItem
{
    public long Id { get; set; }
    public long ParticipantId { get; set; }
    public string Name { get; set; } = "";
    public decimal? Price { get; set; }
    public int Qty { get; set; } = 1;
}

/// <summary>Общая позиция чека (соус, упаковка) или сбор (доставка, работа сервиса).</summary>
public sealed class SharedItem
{
    public long Id { get; set; }
    public long MealId { get; set; }
    public string Name { get; set; } = "";
    /// <summary>item, delivery, service</summary>
    public string Kind { get; set; } = "item";
    public decimal? Price { get; set; }
    public int Qty { get; set; } = 1;
    /// <summary>true: делится на всех участников; false: только на ParticipantIds.</summary>
    public bool SplitAll { get; set; } = true;
    public List<long> ParticipantIds { get; set; } = new();

    public decimal Amount => (Price ?? 0) * Qty;
}
