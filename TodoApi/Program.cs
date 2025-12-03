using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 📌 קריאת Connection String
// ===============================
var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionString")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("CONNECTIONSTRING");

if (string.IsNullOrEmpty(connectionString))
{
    var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
    var dbName = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "ToDoDb";
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

    connectionString =
        $"Server={dbServer};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";
}

Console.WriteLine("📌 Using Connection String:");
Console.WriteLine(connectionString);

// ===============================
// 📌 Register DB Context
// ===============================
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString,
        new MySqlServerVersion(new Version(8, 0, 33)),
        mysql => mysql.EnableRetryOnFailure(3)));

// ===============================
// 📌 CORS תקין
// ===============================
var corsPolicy = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
        policy.WithOrigins("https://todolist-frontend-zrkx.onrender.com")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ===============================
// 📌 הפעלת CORS לפני הכול
// ===============================
app.UseCors(corsPolicy);

// ===============================
// 📌 Static Files (React build)
// ===============================
var staticFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build");

if (Directory.Exists(staticFilesPath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(staticFilesPath),
        RequestPath = ""
    });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// ===============================
// 📌 Health Check
// ===============================
app.MapGet("/", () => "🚀 Todo API is running!");

app.MapGet("/health", async (ToDoDbContext db) =>
{
    try
    {
        await db.Items.FirstOrDefaultAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch
    {
        return Results.StatusCode(500);
    }
});

// ===============================
// 📌 CRUD
// ===============================

// GET
app.MapGet("/items", async (ToDoDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items);
});

// POST
app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

// PUT
app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound(new { message = "Item not found" });

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// DELETE
app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound(new { message = "Item not found" });

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Item deleted successfully" });
});

// ===============================
// 📌 Render PORT חובה
// ===============================
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

// ===============================
// 📌 הפעלת השרת
// ===============================
app.Run();
