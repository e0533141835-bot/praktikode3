// using Microsoft.EntityFrameworkCore;
// using TodoApi;

// // var builder = WebApplication.CreateBuilder(args);
// var builder = WebApplication.CreateBuilder(args);

// builder.Configuration.AddEnvironmentVariables();

// builder.Services.AddDbContext<ToDoDbContext>(options =>
//     options.UseMySql(
//         builder.Configuration.GetConnectionString("ToDoDB"),
//         new MySqlServerVersion(new Version(8, 0, 33))
//     )
// );
// // ✅ רישום ה־DbContext
// // builder.Services.AddDbContext<ToDoDbContext>(options =>
// //     options.UseMySql(
// //         builder.Configuration.GetConnectionString("ToDoDB"),
// //         new MySqlServerVersion(new Version(8, 0, 33))
// //     )
// // );

// // ✅ הוספת CORS לאפליקציה (מאפשר ל-frontend לגשת לשרת)
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader());
// });

// var app = builder.Build();

// // ✅ הפעלת CORS
// app.UseCors("AllowAll");

// // ✅ הוספת התמיכה בקבצי HTML, CSS, JS (כדי לראות את הדף הורדרד שלך)
// app.UseDefaultFiles();
// app.UseStaticFiles();

// // ✅ נתיב ברירת מחדל כדי שלא תקבל 404
// app.MapGet("/", () => "✅ Todo API is running...");

// // ✅ שליפת כל המשימות
// app.MapGet("/items", async (ToDoDbContext db) =>
//     await db.Items.ToListAsync()
// );

// // ✅ הוספת משימה חדשה
// app.MapPost("/items", async (ToDoDbContext db, Item item) =>
// {
//     db.Items.Add(item);
//     await db.SaveChangesAsync();
//     return Results.Created($"/items/{item.Id}", item);
// });

// // ✅ עדכון משימה קיימת
// app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) =>
// {
//     var item = await db.Items.FindAsync(id);
//     if (item == null) return Results.NotFound();

//     item.Name = updatedItem.Name;
//     item.IsComplete = updatedItem.IsComplete;
//     await db.SaveChangesAsync();

//     return Results.Ok(item);
// });

// // ✅ מחיקת משימה לפי מזהה
// app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
// {
//     var item = await db.Items.FindAsync(id);
//     if (item == null) return Results.NotFound();

//     db.Items.Remove(item);
//     await db.SaveChangesAsync();
//     return Results.Ok();
// });

// app.Run();



using Microsoft.EntityFrameworkCore;
using TodoApi;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// ✅ קריאה למשתני סביבה
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbName = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "ToDoDb";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

// ✅ בניית מחרוזת החיבור דינמית
var connectionString = $"Server={dbServer};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";

// ✅ רישום DbContext עם MySQL
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33)))
);

// ✅ הוספת CORS כדי שה-frontend יוכל לגשת לשרת
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ✅ הפעלת CORS
app.UseCors("AllowAll");

// ✅ תמיכה בקבצי סטטיים
app.UseDefaultFiles();
app.UseStaticFiles();

// ✅ נתיב ברירת מחדל
app.MapGet("/", () => "✅ Todo API is running...");

// ✅ CRUD עבור המשימות
app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync());

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
    return Results.Ok();
});

app.Run();
