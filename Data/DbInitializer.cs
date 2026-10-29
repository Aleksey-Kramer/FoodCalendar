using System.Reflection;
using System.Text;
using Dapper;
using FoodCalendar.Repositories;
using FoodCalendar.Services;

namespace FoodCalendar.Data;

/// <summary>Создаёт схему (schema.sql), выполняет миграции и при пустой БД засевает начальные данные.</summary>
public sealed class DbInitializer
{
    /// <summary>Название ресторана и имя встроенной PNG-картинки из Data/SeedImages.</summary>
    private static readonly (string Name, string Resource)[] SeedRestaurants =
    {
        ("Oshi Amiri",  "oshi-amiri.png"),
        ("Evos",        "evos.png"),
        ("Burger King", "burger-king.png"),
        ("Mazzali",     "mazzali.png"),
        ("Плов",        "plov.png"),
        ("PastaPasta",  "pasta-pasta.png"),
        ("Ris",         "ris.png"),
        ("Yapona Mama", "yapona-mama.png"),
        ("KFC",         "kfc.png"),
        ("Bon!",        "bon.png"),
        ("Safia",       "safia.png"),
    };

    private readonly SqliteConnectionFactory _factory;
    private readonly StorageOptions _storage;
    private readonly RestaurantRepository _restaurants;
    private readonly MealRepository _meals;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        SqliteConnectionFactory factory,
        StorageOptions storage,
        RestaurantRepository restaurants,
        MealRepository meals,
        ILogger<DbInitializer> logger)
    {
        _factory = factory;
        _storage = storage;
        _restaurants = restaurants;
        _meals = meals;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        CreateSchema();
        Migrate();
        Directory.CreateDirectory(_storage.ImagesPath);
        Directory.CreateDirectory(_storage.ReceiptsPath);

        if (await _restaurants.CountAsync() > 0)
            return;

        _logger.LogInformation("Ресторанов нет, засеваем начальные данные");

        var ids = new List<long>();
        foreach (var (name, resource) in SeedRestaurants)
        {
            var file = CopySeedImage(resource);
            ids.Add(await _restaurants.InsertAsync(name, file));
        }

        // Перемешанная ротация: все рестораны по кругу, без повторов подряд.
        var today = DateTime.Today;
        var seeded = 0;
        var queue = new List<long>();
        long? last = null;
        for (var d = 1; d <= DateTime.DaysInMonth(today.Year, today.Month); d++)
        {
            var date = new DateOnly(today.Year, today.Month, d);
            if (CalendarService.IsWeekend(date))
                continue;

            if (queue.Count == 0)
            {
                queue = ids.OrderBy(_ => Random.Shared.Next()).ToList();
                if (queue.Count > 1 && queue[0] == last)
                    (queue[0], queue[^1]) = (queue[^1], queue[0]);
            }

            var id = queue[0];
            queue.RemoveAt(0);
            await _meals.InsertAsync(date, id, isMain: true, label: null);
            last = id;
            seeded++;
        }

        _logger.LogInformation("Засеяно ресторанов: {Restaurants}, назначений на текущий месяц: {Days}", ids.Count, seeded);
    }

    private void CreateSchema()
    {
        using var stream = OpenResource("schema.sql");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var sql = reader.ReadToEnd();

        using var conn = _factory.Open();
        conn.Execute(sql);
    }

    /// <summary>Старая таблица day_assignments переносится в meals как основные блюда.</summary>
    private void Migrate()
    {
        using var conn = _factory.Open();
        // meals.is_hidden: ресторан убран из дня, чек сохранён и скрыт до повторного выбора
        var hasHidden = conn.Query<string>("SELECT name FROM pragma_table_info('meals')").Contains("is_hidden");
        if (!hasHidden)
        {
            conn.Execute("ALTER TABLE meals ADD COLUMN is_hidden INTEGER NOT NULL DEFAULT 0 CHECK (is_hidden IN (0, 1))");
            _logger.LogInformation("Миграция: добавлена колонка meals.is_hidden");
        }
        // meals.mode: доставка (delivery) или еда на месте (dinein) — определяет набор сборов чека
        var hasMode = conn.Query<string>("SELECT name FROM pragma_table_info('meals')").Contains("mode");
        if (!hasMode)
        {
            conn.Execute("ALTER TABLE meals ADD COLUMN mode TEXT NOT NULL DEFAULT 'delivery' CHECK (mode IN ('delivery', 'dinein'))");
            _logger.LogInformation("Миграция: добавлена колонка meals.mode");
        }

        // meal_participants.is_paid: отметка «оплатил» (по умолчанию — ожидает оплаты)
        if (!conn.Query<string>("SELECT name FROM pragma_table_info('meal_participants')").Contains("is_paid"))
        {
            conn.Execute("ALTER TABLE meal_participants ADD COLUMN is_paid INTEGER NOT NULL DEFAULT 0 CHECK (is_paid IN (0, 1))");
            _logger.LogInformation("Миграция: добавлена колонка meal_participants.is_paid");
        }
        // ux_meals_main: уникальность только среди видимых основных — скрытые чеки разных ресторанов сосуществуют
        var mainIndexSql = conn.ExecuteScalar<string>("SELECT sql FROM sqlite_master WHERE type = 'index' AND name = 'ux_meals_main'") ?? "";
        if (mainIndexSql.Length > 0 && !mainIndexSql.Contains("is_hidden"))
        {
            conn.Execute("DROP INDEX ux_meals_main");
            conn.Execute("CREATE UNIQUE INDEX ux_meals_main ON meals(date) WHERE is_main = 1 AND is_hidden = 0");
            _logger.LogInformation("Миграция: индекс ux_meals_main учитывает скрытые приёмы");
        }
        // meal_shared_items.kind: новые типы сборов (обслуживание, чаевые). SQLite не умеет менять CHECK —
        // таблица пересоздаётся; внешние ключи на время выключены, иначе DROP каскадно удалит привязки участников.
        var sharedSql = conn.ExecuteScalar<string>("SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'meal_shared_items'") ?? "";
        if (sharedSql.Length > 0 && !sharedSql.Contains("'tips'"))
        {
            conn.Execute("PRAGMA foreign_keys = OFF");
            using (var tx = conn.BeginTransaction())
            {
                conn.Execute(
                    "CREATE TABLE meal_shared_items_new (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "meal_id INTEGER NOT NULL REFERENCES meals(id) ON DELETE CASCADE, " +
                    "name TEXT NOT NULL CHECK (length(trim(name)) > 0), " +
                    "kind TEXT NOT NULL DEFAULT 'item' CHECK (kind IN ('item', 'delivery', 'service', 'servicecharge', 'tips')), " +
                    "price REAL NULL, qty INTEGER NOT NULL DEFAULT 1 CHECK (qty > 0), " +
                    "split_all INTEGER NOT NULL DEFAULT 1 CHECK (split_all IN (0, 1)))", transaction: tx);
                conn.Execute("INSERT INTO meal_shared_items_new (id, meal_id, name, kind, price, qty, split_all) " +
                             "SELECT id, meal_id, name, kind, price, qty, split_all FROM meal_shared_items", transaction: tx);
                conn.Execute("DROP TABLE meal_shared_items", transaction: tx);
                conn.Execute("ALTER TABLE meal_shared_items_new RENAME TO meal_shared_items", transaction: tx);
                conn.Execute("CREATE INDEX IF NOT EXISTS ix_meal_shared_items_meal ON meal_shared_items(meal_id)", transaction: tx);
                tx.Commit();
            }
            conn.Execute("PRAGMA foreign_keys = ON");
            _logger.LogInformation("Миграция: расширены типы сборов в meal_shared_items");
        }

        var hasOld = conn.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'day_assignments'") > 0;
        if (!hasOld) return;

        var moved = conn.Execute(
            "INSERT INTO meals (date, restaurant_id, is_main, label, created_at, updated_at) " +
            "SELECT date, restaurant_id, 1, NULL, updated_at, updated_at FROM day_assignments " +
            "WHERE date NOT IN (SELECT date FROM meals WHERE is_main = 1)");
        conn.Execute("DROP TABLE day_assignments");
        _logger.LogInformation("Миграция: перенесено назначений из day_assignments в meals: {Count}", moved);
    }

    /// <summary>Копирует встроенную картинку в папку картинок под её читаемым именем (например, plov.png).</summary>
    private string CopySeedImage(string resource)
    {
        var fileName = resource;
        using var src = OpenResource("seed/" + resource);
        using var dst = File.Create(Path.Combine(_storage.ImagesPath, fileName));
        src.CopyTo(dst);
        return fileName;
    }

    private static Stream OpenResource(string logicalName) =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream(logicalName)
        ?? throw new InvalidOperationException($"Не найден встроенный ресурс {logicalName}");
}
