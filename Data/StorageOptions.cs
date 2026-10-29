namespace FoodCalendar.Data;

/// <summary>Пути к БД, папке картинок и папке чеков из appsettings.json (секция Storage).</summary>
public sealed class StorageOptions
{
    public string DbPath { get; set; } = "storage/foodcalendar.db";
    public string ImagesPath { get; set; } = "storage/images";
    public string ReceiptsPath { get; set; } = "storage/receipts";

    /// <summary>Относительные пути превращаются в абсолютные относительно корня приложения.</summary>
    public void Resolve(string contentRoot)
    {
        DbPath = Path.GetFullPath(Path.Combine(contentRoot, DbPath));
        ImagesPath = Path.GetFullPath(Path.Combine(contentRoot, ImagesPath));
        ReceiptsPath = Path.GetFullPath(Path.Combine(contentRoot, ReceiptsPath));
    }
}
