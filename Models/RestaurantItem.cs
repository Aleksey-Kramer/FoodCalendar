namespace FoodCalendar.Models;

/// <summary>Позиция меню ресторана с ценой (для подсказок при вводе).</summary>
public sealed class RestaurantItem
{
    public long Id { get; set; }
    public long RestaurantId { get; set; }
    public string Name { get; set; } = "";
    public decimal? Price { get; set; }
}
