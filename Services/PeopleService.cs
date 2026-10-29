using System.Text.RegularExpressions;
using FoodCalendar.Models;
using FoodCalendar.Repositories;

namespace FoodCalendar.Services;

public sealed class PeopleService
{
    public const int MaxNameLength = 60;

    private readonly PeopleRepository _repo;

    public PeopleService(PeopleRepository repo) => _repo = repo;

    public Task<List<Person>> GetAllAsync() => _repo.GetAllAsync();

    public static string NormalizeName(string? raw)
    {
        var name = Regex.Replace((raw ?? "").Trim(), @"\s+", " ");
        if (name.Length == 0)
            throw new ValidationException("Введите имя.");
        if (name.Length > MaxNameLength)
            throw new ValidationException($"Имя не должно быть длиннее {MaxNameLength} символов.");
        return name;
    }

    /// <summary>Возвращает существующего человека по имени или создаёт нового.</summary>
    public async Task<Person> GetOrCreateAsync(string rawName)
    {
        var name = NormalizeName(rawName);
        var existing = await _repo.FindByNameAsync(name);
        if (existing is not null) return existing;
        var id = await _repo.InsertAsync(name);
        return new Person { Id = id, Name = name };
    }

    public Task<Person?> FindAsync(string rawName) => _repo.FindByNameAsync(NormalizeName(rawName));

    public Task DeleteAsync(long id) => _repo.DeleteAsync(id);
}
