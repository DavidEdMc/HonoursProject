using HonoursProject.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new NoCacheAttribute());
});

builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LessonService>();
builder.Services.AddScoped<AchievementService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Force the app to run on localhost:5000
app.Urls.Add("http://localhost:5000");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LoginPage}/{id?}");

// Automatically open the browser when the EXE starts
try
{
    Process.Start(new ProcessStartInfo
    {
        FileName = "http://localhost:5000",
        UseShellExecute = true
    });
}
catch
{
    // Ignore failures
}

app.Run();
