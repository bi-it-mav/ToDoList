using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using ToDoList.Components;
using ToDoList.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

(string connectionString, ServerVersion serverVersion) credentials;
try
{
    var maybeConnectionString = builder.Configuration.GetConnectionString("Default") ?? "";
    credentials = (maybeConnectionString, ServerVersion.AutoDetect(maybeConnectionString));
} catch (MySqlException)
{
    // Used for defining migrations; make sure this matches `docker-compose.yaml` before you run `Add-Migration`.
    credentials = (
        // Partially fake.
        "Server=localhost;Port=3306;Database=todolistdb;User=fakeuser;Password=fakepassword;",
        new MariaDbServerVersion(new Version(12, 0))
    );
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(credentials.connectionString, credentials.serverVersion));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
