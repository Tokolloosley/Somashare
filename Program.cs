using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SomaShare.Data;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using SomaShare.Models;

var builder = WebApplication.CreateBuilder(args);

// DATABASE

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY & SECURITY
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password Requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Lockout
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;

    // User Settings
    options.User.RequireUniqueEmail = true;

    // Sign In
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// MVC + RAZOR

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// SESSION
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("af"),
        new CultureInfo("zu"),
        new CultureInfo("xh"),
        new CultureInfo("st")
    };

    options.DefaultRequestCulture =
        new RequestCulture("en");

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
// BUILD APP
var app = builder.Build();

// ERROR HANDLING
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// MIDDLEWARE
app.UseHttpsRedirection();

app.UseStaticFiles();

var localizationOptions =
    app.Services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();

app.UseRequestLocalization(
    localizationOptions.Value);

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// DATABASE INITIALIZATION
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context =
            services.GetRequiredService<ApplicationDbContext>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.InitializeAsync(
            context,
            userManager,
            roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

// ROUTING

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// RUN

app.Run();