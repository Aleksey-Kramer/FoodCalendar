using Dapper;
using FoodCalendar.Components;
using FoodCalendar.Data;
using FoodCalendar.Repositories;
using FoodCalendar.Services;
using Microsoft.Extensions.FileProviders;

// Колонки snake_case -> свойства PascalCase
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

var storage = builder.Configuration.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();
storage.Resolve(builder.Environment.ContentRootPath);
Directory.CreateDirectory(storage.ImagesPath);
Directory.CreateDirectory(storage.ReceiptsPath);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(storage);
builder.Services.AddSingleton<SqliteConnectionFactory>();

builder.Services.AddScoped<RestaurantRepository>();
builder.Services.AddScoped<RestaurantItemRepository>();
builder.Services.AddScoped<MealRepository>();
builder.Services.AddScoped<PeopleRepository>();
builder.Services.AddScoped<VoteRepository>();

builder.Services.AddScoped<I18n>();
builder.Services.AddSingleton(builder.Configuration.GetSection("Admin").Get<AdminOptions>() ?? new AdminOptions());
builder.Services.AddSingleton<AdminAuth>();
builder.Services.AddScoped<AdminSession>();
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<MealService>();
builder.Services.AddScoped<PeopleService>();
builder.Services.AddScoped<VoteService>();
builder.Services.AddSingleton<ChangeNotifier>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<ReceiptService>();
builder.Services.AddScoped<DbInitializer>();

var app = builder.Build();

// Схема, миграции и сиды при старте
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DbInitializer>().InitializeAsync();
}

app.Logger.LogInformation("База данных: {Db}", storage.DbPath);
app.Logger.LogInformation("Папка картинок: {Images}", storage.ImagesPath);
app.Logger.LogInformation("Папка чеков: {Receipts}", storage.ReceiptsPath);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

// Картинки ресторанов и чеки из папок, заданных в конфиге
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storage.ImagesPath),
    RequestPath = "/images",
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=86400"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storage.ReceiptsPath),
    RequestPath = "/receipts"
});

app.UseAntiforgery();

// Вход администратора: обычная HTML-форма, cookie с подписанным токеном на 30 дней
app.MapPost("/auth/login", (HttpContext ctx, AdminAuth auth) =>
{
    var form = ctx.Request.Form;
    if (!auth.Verify(form["login"], form["password"]))
        return Results.Redirect("/login?failed=1");
    ctx.Response.Cookies.Append(AdminAuth.CookieName, auth.CreateToken(), new CookieOptions
    {
        HttpOnly = true, SameSite = SameSiteMode.Lax, MaxAge = TimeSpan.FromDays(30), Path = "/"
    });
    var returnUrl = form["returnUrl"].ToString();
    return Results.Redirect(returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") ? returnUrl : "/");
}).DisableAntiforgery();

app.MapPost("/auth/logout", (HttpContext ctx) =>
{
    ctx.Response.Cookies.Delete(AdminAuth.CookieName, new CookieOptions { Path = "/" });
    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
