using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Services.Interfaces;
using MoodPlaylistGenerator.Services.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SQLite database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=MoodPlaylist.db"));

// Add services
// OPTION 1: Use SQLite implementation (Code-First with Entity Framework)
builder.Services.AddScoped<IAuthService, SQLiteAuthService>();

// OPTION 2: Use In-Memory implementation (List-based for learning/testing)
// Uncomment the line below and comment out the line above to switch
// builder.Services.AddSingleton<IAuthService, InMemoryAuthService>();

builder.Services.AddScoped<SongService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<MoodService>();
builder.Services.AddScoped<AnalyticsService>();

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Add static files middleware
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database
await InitializeDatabaseAsync(app.Services);

async Task InitializeDatabaseAsync(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        
        context.Database.EnsureCreated();
        
        // Create a test user if no users exist
        if (!context.Users.Any())
        {
            await authService.RegisterAsync("test@example.com", "testuser", "password123");
            Console.WriteLine("Test user created - Email: test@example.com, Password: password123");
        }
    }
}

app.Run();
