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
    // devre dışı bırakıldı, onun yerine ilkel bir oturum tokeni kontrolü var.
    // TODO: Baş mühendis Azure AD'yi kurunca bu middleware kaldırılıp
    // ApiController'daki [Authorize] satırı geri açılmalı.
    //
    // Sadece ADMIN paneline özel endpoint'ler token istiyor (kurye yönetimi,
    // paket oluşturma/listeleme). Kurye mobil uygulaması ve simulator-bot'un
    // henüz kendi bir girişi/şifresi yok - bu yüzden journeys/start,
    // syncactions, telemetry/batch, mypackages şimdilik açık bırakıldı.
    // Kurye/bot tarafı için ayrı bir kimlik doğrulama gerekiyorsa, bu ayrıca ele alınmalı.
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        var isAdminOnlyEndpoint =
            path == "/api/couriers" ||
            (path == "/api/packages" && (method == "GET" || method == "POST"));

        if (isAdminOnlyEndpoint)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            var token = authHeader != null && authHeader.StartsWith("Bearer ")
                ? authHeader["Bearer ".Length..]
                : null;

            if (!SessionStore.IsValid(token))
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
