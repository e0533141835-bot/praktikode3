// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.FileProviders;
// using TodoApi;

// var builder = WebApplication.CreateBuilder(args);

// // ===============================
// // 📌 קריאת ConnectionString מהשרת
// // ===============================
// var connectionString =
//     Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
//     Environment.GetEnvironmentVariable("ConnectionString") ??
//     Environment.GetEnvironmentVariable("DATABASE_URL") ??
//     Environment.GetEnvironmentVariable("CONNECTIONSTRING");

// if (string.IsNullOrEmpty(connectionString))
// {
//     var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
//     var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
//     var dbName = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "ToDoDb";
//     var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
//     var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

//     connectionString = $"Server={dbServer};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";
// }

// Console.WriteLine("📌 Using Connection String:");
// Console.WriteLine(connectionString);

// // ===============================
// // 📌 Register DbContext
// // ===============================
// builder.Services.AddDbContext<ToDoDbContext>(options =>
// {
//     options.UseMySql(connectionString,
//         new MySqlServerVersion(new Version(8, 0, 33)),
//         mysql => mysql.EnableRetryOnFailure(3)
//     );
// });

// // ===============================
// // 📌 CORS – שם אחד קבוע!
// // ===============================
// var corsPolicy = "AllowFrontend";

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(corsPolicy, policy =>
//         policy.WithOrigins("https://todolist-frontend-zrkx.onrender.com")
//               .AllowAnyMethod()
//               .AllowAnyHeader());
// });

// // ===============================
// var app = builder.Build();

// // ===============================
// // 📌 הפעלת CORS
// // ===============================
// app.UseCors(corsPolicy);

// // ===============================
// // 📌 Static Files (אם יש React build)
// // ===============================
// var staticFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build");

// if (Directory.Exists(staticFilesPath))
// {
//     app.UseDefaultFiles();
//     app.UseStaticFiles(new StaticFileOptions
//     {
//         FileProvider = new PhysicalFileProvider(staticFilesPath),
//         RequestPath = ""
//     });
// }
// else
// {
//     app.UseDefaultFiles();
//     app.UseStaticFiles();
// }

// // ===============================
// // 📌 Health Check
// // ===============================
// app.MapGet("/", () => "✅ Todo API is running...");

// app.MapGet("/health", async (ToDoDbContext db) =>
// {
//     try
//     {
//         await db.Items.FirstOrDefaultAsync();
//         return Results.Ok(new { status = "healthy", database = "connected" });
//     }
//     catch
//     {
//         return Results.StatusCode(500);
//     }
// });

// // ===============================
// // 📌 CRUD
// // ===============================

// app.MapGet("/items", async (ToDoDbContext db) =>
// {
//     var items = await db.Items.ToListAsync();
//     return Results.Ok(items);
// });

// app.MapPost("/items", async (ToDoDbContext db, Item item) =>
// {
//     db.Items.Add(item);
//     await db.SaveChangesAsync();
//     return Results.Created($"/items/{item.Id}", item);
// });

// app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) =>
// {
//     var item = await db.Items.FindAsync(id);
//     if (item == null) return Results.NotFound();

//     item.Name = updatedItem.Name;
//     item.IsComplete = updatedItem.IsComplete;
//     await db.SaveChangesAsync();

//     return Results.Ok(item);
// });

// app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
// {
//     var item = await db.Items.FindAsync(id);
//     if (item == null) return Results.NotFound();

//     db.Items.Remove(item);
//     await db.SaveChangesAsync();
//     return Results.Ok(new { message = "Item deleted" });
// });

// // ===============================
// // 📌 PORT – חובה ב־Render
// // ===============================
// var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
// app.Urls.Add($"http://*:{port}");

// // ===============================
// app.Run();



using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MySql.Data.MySqlClient; // הוספת Using זה לוודא ש-MySqlException נגיש במקרה הצורך
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 📌 קריאת ConnectionString מהקונפיגורציה
// ===============================
// קורא מ: 1. ConnectionStrings:DefaultConnection ב-appsettings.json
//       2. משתנה סביבה: ConnectionStrings__DefaultConnection (ב-Render)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ ERROR: Connection String 'DefaultConnection' not found in configuration.");
    // זריקת חריגה מונעת הרצה במצב לא מוגדר
    throw new InvalidOperationException("Connection String 'DefaultConnection' not found. Check appsettings.json or environment variables (e.g., ConnectionStrings__DefaultConnection).");
}

Console.WriteLine("📌 Using Connection String:");
Console.WriteLine(connectionString);

// ===============================
// 📌 Register DbContext
// ===============================
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    // ודא שגרסת ה-MySQL תואמת למה שמוגדר ב-Clever Cloud.
    // אם לא יודעים, 8.0.33 היא הנחה סבירה.
    options.UseMySql(connectionString,
        new MySqlServerVersion(new Version(8, 0, 33)), 
        mysql => mysql.EnableRetryOnFailure(3)
    );
});

// ===============================
// 📌 CORS – שם אחד קבוע!
// ===============================
var corsPolicy = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
        policy.WithOrigins("https://todolist-frontend-zrkx.onrender.com")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ===============================
// 📌 הוספת תמיכה במינימל API (אם צריך)
// builder.Services.AddEndpointsApiExplorer();
// ===============================

var app = builder.Build();

// ===============================
// 📌 הפעלת CORS
// ===============================
app.UseCors(corsPolicy);

// ===============================
// 📌 Static Files (אם יש React build) - **נשאר כפי שהיה, אך מומלץ לוודא את נתיב ה-Build**
// ===============================
// var staticFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build");

// if (Directory.Exists(staticFilesPath))
// {
//     app.UseDefaultFiles();
//     app.UseStaticFiles(new StaticFileOptions
//     {
//         FileProvider = new PhysicalFileProvider(staticFilesPath),
//         RequestPath = ""
//     });
// }
// else
// {
//     // אם ה-Frontend מופרד (ואין תיקיית build), אנו עדיין רוצים קבצים סטטיים מה-wwwroot הסטנדרטי
//     app.UseDefaultFiles();
//     app.UseStaticFiles();
// }

// ===============================
// 📌 Health Check
// ===============================
app.MapGet("/", () => "✅ Todo API is running...");

app.MapGet("/health", async (ToDoDbContext db) =>
{
    try
    {
        // בדיקה אמיתית ע"י ניסיון קריאה לבסיס הנתונים
        await db.Items.FirstOrDefaultAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch (MySqlException ex)
    {
        // הצגת שגיאה מפורטת יותר במקרה של כשל בחיבור לבסיס הנתונים
        Console.WriteLine($"❌ Database connection failed: {ex.Message}");
        return Results.StatusCode(500);
    }
    catch
    {
        Console.WriteLine("wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
        return Results.StatusCode(500);

    }
});

// ===============================
// 📌 CRUD
// ===============================

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

// ===============================
// 📌 PORT – חובה ב־Render
// ===============================
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

// ===============================
app.Run();
