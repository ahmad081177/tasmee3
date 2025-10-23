using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Application.Services;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;
using QuranListeningApp.Infrastructure.Repositories;
using QuranListeningApp.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Antiforgery
builder.Services.AddAntiforgery();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Database
builder.Services.AddDbContext<QuranAppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("QuranListeningApp.Infrastructure")));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListeningSessionRepository, ListeningSessionRepository>();
builder.Services.AddScoped<ISurahReferenceRepository, SurahReferenceRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();

// Register Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ListeningSessionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<AppSettingsService>();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Initialize database and seed data
await DbInitializer.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add security headers and cache control
app.Use(async (context, next) =>
{
    // Add cache control headers for authenticated pages
    if (context.Request.Path.StartsWithSegments("/admin") || 
        context.Request.Path.StartsWithSegments("/teacher") || 
        context.Request.Path.StartsWithSegments("/student"))
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }
    
    // Add security headers
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    await next();
});

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
