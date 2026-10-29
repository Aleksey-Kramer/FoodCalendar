namespace FoodCalendar.Models;

public sealed class Restaurant
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string? ImageFile { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = "";
}
