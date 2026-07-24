using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using CourierDTS.Auth;
using CourierDTS.Data;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Starting up CourierDTS");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Frontend henüz hangi adreste çalışacak netleşmedi - şimdilik her origin'e izin
    // veriyoruz, üretime geçerken belirli adres(ler)e kısıtlanmalı.
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.

    app.UseHttpsRedirection();

    app.UseCors("AllowFrontend");

    // GEÇİCİ: Azure AD henüz gerçek değerlerle kurulmadığı için [Authorize] (JWT)
    // devre dışı bırakıldı, onun yerine ilkel, rol bazlı bir oturum tokeni kontrolü var.
    // TODO: Baş mühendis Azure AD'yi kurunca bu middleware kaldırılıp
    // ApiController'daki [Authorize] satırı geri açılmalı.
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        // Admin paneline özel: kurye yönetimi, paket oluşturma/listeleme,
        // konum ekleme/güncelleme/silme (okuma - GET /api/locations - herkese açık kalıyor).
        var requiresAdmin =
            path == "/api/couriers" ||
            (path == "/api/packages" && (method == "GET" || method == "POST")) ||
            (path.StartsWith("/api/packages/") && !path.StartsWith("/api/packages/mypackages") && !path.StartsWith("/api/packages/syncactions")) ||
            (path == "/api/locations" && method == "POST") ||
            (path.StartsWith("/api/locations/") && (method == "PUT" || method == "DELETE"));

        // Kurye mobil uygulamasına/simulator-bot'a özel.
        var requiresCourier =
            path == "/api/packages/mypackages" ||
            path == "/api/packages/syncactions" ||
            path == "/api/journeys/start" ||
            path == "/api/telemetry/batch";

        if (requiresAdmin || requiresCourier)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            var token = authHeader != null && authHeader.StartsWith("Bearer ")
                ? authHeader["Bearer ".Length..]
                : null;

            var requiredRole = requiresAdmin ? "admin" : "courier";

            if (!SessionStore.IsValidForRole(token, requiredRole))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Giriş yapmanız gerekiyor.");
                return;
            }
        }

        await next();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Program stopped due to exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
