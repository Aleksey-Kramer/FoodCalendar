using FoodCalendar.Data;
using Microsoft.AspNetCore.Components.Forms;

namespace FoodCalendar.Services;

/// <summary>Файлы чеков: картинка (JPG/PNG/WebP) или PDF, до 10 МБ, GUID-имена.</summary>
public sealed class ReceiptService
{
    public const long MaxBytes = 10 * 1024 * 1024;
    public const string Accept = "image/jpeg,image/png,image/webp,application/pdf";

    private readonly StorageOptions _storage;

    public ReceiptService(StorageOptions storage) => _storage = storage;

    public static string? Url(string? fileName) =>
        string.IsNullOrEmpty(fileName) ? null : "/receipts/" + fileName;

    public static bool IsPdf(string? fileName) =>
        fileName is not null && fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    public async Task<string> SaveAsync(IBrowserFile file)
    {
        if (file.Size > MaxBytes)
            throw new ValidationException("err.receiptTooBig");
        if (file.Size == 0)
            throw new ValidationException("err.fileEmpty");

        using var ms = new MemoryStream((int)file.Size);
        await using (var stream = file.OpenReadStream(MaxBytes))
        {
            await stream.CopyToAsync(ms);
        }
        var bytes = ms.ToArray();

        var ext = DetectExtension(bytes)
            ?? throw new ValidationException("err.receiptType");

        Directory.CreateDirectory(_storage.ReceiptsPath);
        var fileName = Guid.NewGuid().ToString("N") + ext;
        await File.WriteAllBytesAsync(Path.Combine(_storage.ReceiptsPath, fileName), bytes);
        return fileName;
    }

    public void Delete(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName) || Path.GetFileName(fileName) != fileName)
            return;
        var path = Path.Combine(_storage.ReceiptsPath, fileName);
        if (File.Exists(path))
            File.Delete(path);
    }

    public static string? DetectExtension(ReadOnlySpan<byte> b)
    {
        // PDF: "%PDF"
        if (b.Length >= 4 && b[0] == 0x25 && b[1] == 0x50 && b[2] == 0x44 && b[3] == 0x46)
            return ".pdf";
        var img = ImageService.DetectExtension(b);
        return img is ".jpg" or ".png" or ".webp" ? img : null;
    }
}
