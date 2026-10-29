using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace FoodCalendar.Services;

/// <summary>Логин и SHA-256-хеш пароля администратора. Значения по умолчанию вшиты, можно переопределить в appsettings («Admin»).</summary>
public sealed class AdminOptions
{
    public string Login { get; set; } = "admin";
    public string PasswordHash { get; set; } = "a541986a7370273acc2c8ee72e8c877d2568f88151aad2668f7f4c22f12fbd53";
}

/// <summary>
/// Вход администратора: проверка пароля по хешу и подписанный cookie-токен (Data Protection), 30 дней.
/// Singleton. Сам пароль нигде не хранится.
/// </summary>
public sealed class AdminAuth
{
    public const string CookieName = "fc.admin";
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    private readonly AdminOptions _options;
    private readonly IDataProtector _protector;

    public AdminAuth(AdminOptions options, IDataProtectionProvider provider)
    {
        _options = options;
        _protector = provider.CreateProtector("FoodCalendar.Admin");
    }

    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool Verify(string? login, string? password)
    {
        if (login is null || password is null) return false;
        var loginOk = string.Equals(login.Trim(), _options.Login, StringComparison.OrdinalIgnoreCase);
        var expected = Encoding.ASCII.GetBytes(_options.PasswordHash.ToLowerInvariant());
        var actual = Encoding.ASCII.GetBytes(Hash(password));
        // сравнение за постоянное время, логин проверяется после хеша, чтобы не отличать ошибки по времени
        var hashOk = expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
        return loginOk && hashOk;
    }

    /// <summary>Токен для cookie: подписанная и зашифрованная метка срока действия.</summary>
    public string CreateToken() =>
        _protector.Protect("admin|" + DateTimeOffset.UtcNow.Add(Lifetime).ToUnixTimeSeconds());

    public bool IsValid(string? token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        try
        {
            var parts = _protector.Unprotect(token).Split('|');
            return parts.Length == 2 && parts[0] == "admin"
                && long.TryParse(parts[1], out var exp)
                && DateTimeOffset.FromUnixTimeSeconds(exp) > DateTimeOffset.UtcNow;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}

/// <summary>Кто сейчас работает в цепи Blazor: администратор или обычный пользователь. Scoped; выставляется в Routes.razor.</summary>
public sealed class AdminSession
{
    /// <summary>Для тестов и служебных скриптов, где проверок прав нет.</summary>
    public static readonly AdminSession Trusted = new() { IsAdmin = true };

    public bool IsAdmin { get; set; }

    /// <summary>Бросает ValidationException, если текущий пользователь не администратор.</summary>
    public void Require()
    {
        if (!IsAdmin) throw new ValidationException("err.adminOnly");
    }
}
