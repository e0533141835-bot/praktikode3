
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ ERROR: Connection String 'DefaultConnection' not found in configuration.");
    throw new InvalidOperationException("Connection String 'DefaultConnection' not found. Check appsettings.json or environment variables (e.g., ConnectionStrings__DefaultConnection).");
}

Console.WriteLine("📌 Using Connection String:");
Console.WriteLine(connectionString);

builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseMySql(connectionString,
        new MySqlServerVersion(new Version(8, 0, 33)),
        mysql => mysql.EnableRetryOnFailure(3)
    );
});

var corsPolicy = "AllowAll";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
        policy .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();


app.UseCors(corsPolicy);

app.MapGet("/", () => "✅ Todo API is running...");

app.MapGet("/health", async (ToDoDbContext db) =>
{
    try
    {
        await db.Items.FirstOrDefaultAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch (MySqlException ex)
    {
        Console.WriteLine($"❌ Database connection failed: {ex.Message}");
        return Results.StatusCode(500);
    }
    catch
    {
        return Results.StatusCode(500);
    }
});


app.MapGet("/items", async (ToDoDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items);
});

app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;
    await db.SaveChangesAsync();

    return Results.Ok(item);
});

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Item deleted" });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();
