using Microsoft.EntityFrameworkCore;
using PureFitness.Context;
using PureFitness.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ✅ Read connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ✅ Add MVC
builder.Services.AddControllersWithViews();

// ✅ Register DbContext
builder.Services.AddDbContext<MyDBContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
           .EnableSensitiveDataLogging()
);

// ✅ Enable Session (30 minutes)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Cookie Authentication (consistent naming)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/LoginView";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login/LoginView";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// ✅ Antiforgery
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

// ✅ Register Email Service
builder.Services.AddScoped<PureFitness.Services.EmailService>();

var app = builder.Build();

// ✅ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Must be before Authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LandingPage}/{action=LandingView}/{id?}"
);

app.Run();
