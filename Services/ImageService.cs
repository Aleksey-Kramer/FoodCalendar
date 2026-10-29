using System.Text;
using FoodCalendar.Data;
using Microsoft.AspNetCore.Components.Forms;

namespace FoodCalendar.Services;

/// <summary>Сохранение картинок ресторанов: проверка сигнатуры, лимит размера, GUID-имена.</summary>
public sealed class ImageService
{
    public const long MaxBytes = 2 * 1024 * 1024;
    public const string Accept = "image/jpeg,image/png,image/webp,image/svg+xml";
    private const string TypeError = "Допустимы только изображения JPEG, PNG, WebP или SVG.";

    private readonly StorageOptions _storage;

    public ImageService(StorageOptions storage) => _storage = storage;

    public static string? Url(string? fileName) =>
        string.IsNullOrEmpty(fileName) ? null : "/images/" + fileName;

    /// <summary>Читает загруженный файл в память и проверяет размер и сигнатуру.</summary>
    public async Task<byte[]> ReadAndValidateAsync(IBrowserFile file)
    {
        if (file.Size > MaxBytes)
            throw new ValidationException("Файл больше 2 МБ.");
        if (file.Size == 0)
            throw new ValidationException("Файл пустой.");

        using var ms = new MemoryStream((int)file.Size);
        await using (var stream = file.OpenReadStream(MaxBytes))
        {
            await stream.CopyToAsync(ms);
        }

        var bytes = ms.ToArray();
        Validate(bytes);
        return bytes;
    }

    /// <summary>Сохраняет проверенные байты под GUID-именем и возвращает имя файла на диске.</summary>
    public async Task<string> SaveAsync(byte[] bytes)
    {
        if (bytes.Length == 0 || bytes.Length > MaxBytes)
            throw new ValidationException("Файл пустой или больше 2 МБ.");
        Validate(bytes);
        var ext = DetectExtension(bytes)!;

        Directory.CreateDirectory(_storage.ImagesPath);
        var fileName = Guid.NewGuid().ToString("N") + ext;
        await File.WriteAllBytesAsync(Path.Combine(_storage.ImagesPath, fileName), bytes);
        return fileName;
    }

    public async Task<string> SaveAsync(IBrowserFile file) => await SaveAsync(await ReadAndValidateAsync(file));

    /// <summary>data:-URL для предпросмотра в браузере до сохранения.</summary>
    public static string ToDataUrl(byte[] bytes)
    {
        var mime = DetectExtension(bytes) switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    /// <summary>Удаляет файл картинки (если имя безопасно и файл существует).</summary>
    public void Delete(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName) || Path.GetFileName(fileName) != fileName)
            return;
        var path = Path.Combine(_storage.ImagesPath, fileName);
        if (File.Exists(path))
            File.Delete(path);
    }

    private static void Validate(byte[] bytes)
    {
        var ext = DetectExtension(bytes) ?? throw new ValidationException(TypeError);
        if (ext == ".svg" && SvgHasScript(bytes))
            throw new ValidationException("SVG со скриптами или обработчиками событий не принимается.");
    }

    /// <summary>Определяет тип по сигнатуре (magic bytes), а не по расширению или Content-Type.</summary>
    public static string? DetectExtension(ReadOnlySpan<byte> b)
    {
        // JPEG: FF D8 FF
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ".jpg";
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
                          && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return ".png";
        // WebP: "RIFF" .... "WEBP"
        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                           && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50)
            return ".webp";
        // SVG: текст, начинающийся с <?xml, <!DOCTYPE svg или <svg (после BOM/пробелов/комментариев)
        if (IsSvg(b))
            return ".svg";
        return null;
    }

    private static bool IsSvg(ReadOnlySpan<byte> b)
    {
        if (b.Length < 5) return false;
        var head = Encoding.UTF8.GetString(b[..Math.Min(b.Length, 2048)]).TrimStart('﻿', ' ', '\t', '\r', '\n');
        if (!(head.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
              || head.StartsWith("<svg", StringComparison.OrdinalIgnoreCase)
              || head.StartsWith("<!DOCTYPE svg", StringComparison.OrdinalIgnoreCase)
              || head.StartsWith("<!--", StringComparison.Ordinal)))
            return false;
        return head.Contains("<svg", StringComparison.OrdinalIgnoreCase);
    }

    private static bool SvgHasScript(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        return text.Contains("<script", StringComparison.OrdinalIgnoreCase)
            || text.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
            || System.Text.RegularExpressions.Regex.IsMatch(text, @"\son[a-z]+\s*=", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
