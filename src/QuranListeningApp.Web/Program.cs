using Microsoft.AspNetCore.Authentication;
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

// Configure Database with Azure SQL retry policy
var dbSettings = builder.Configuration.GetSection("DatabaseSettings");
var isAzureSql = dbSettings.GetValue<bool>("IsAzureSql");
var enableRetry = dbSettings.GetValue<bool>("EnableRetryOnFailure");
var maxRetryCount = dbSettings.GetValue<int>("MaxRetryCount", 3);
var maxRetryDelay = dbSettings.GetValue<int>("MaxRetryDelaySeconds", 2);
var commandTimeout = dbSettings.GetValue<int>("CommandTimeoutSeconds", 30);

// Configure DbContextFactory for Blazor Server threading support
builder.Services.AddDbContextFactory<QuranAppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("QuranListeningApp.Infrastructure");
            
            // Enable retry on failure for Azure SQL transient errors
            if (enableRetry)
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: maxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                    errorNumbersToAdd: new[] { 40197, 40501, 40613 } // Azure SQL cold start & transient errors
                );
            }
            
            // Set command timeout (higher for Azure SQL cold starts)
            sqlOptions.CommandTimeout(commandTimeout);
        }));

// Also register a scoped DbContext for dependency injection compatibility
builder.Services.AddScoped<QuranAppDbContext>(provider =>
{
    var factory = provider.GetRequiredService<IDbContextFactory<QuranAppDbContext>>();
    return factory.CreateDbContext();
});

// Register Repositories with DbContextFactory
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

// Add logout handler endpoint
app.MapGet("/logout-handler", async (HttpContext context) =>
{
    // Clear the authentication cookie
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    // Set cache control headers to prevent back button access
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0, private";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    
    // Redirect to login page
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
