using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourierDTS.Auth;
using CourierDTS.Data;
using CourierDTS.Dtos;
using CourierDTS.Models;

namespace CourierDTS.Controllers
{
    // Mühendisin isteği: katmanlara/ayrı controller'lara bölmeden, tüm endpoint'ler tek yerde.
    // TODO: Azure AD gerçek değerlerle kurulunca [Authorize] geri açılmalı
    // (Program.cs'teki geçici X-Demo-Key middleware'i de kaldırılmalı).
    [ApiController]
    [Route("api")]
    // [Authorize]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ApiController> _logger;

        public ApiController(AppDbContext db, ILogger<ApiController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // İlkel giriş kontrolü - gerçek auth (Azure AD) gelince kaldırılacak.
        [HttpPost("admin/login")]
        public async Task<IActionResult> AdminLogin(AdminLoginRequest request)
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Name == request.Name);
            if (admin == null || !PasswordHasher.Verify(request.Password, admin.PasswordHash))
            {
                _logger.LogWarning("Admin login failed for name {Name}", request.Name);
                return Unauthorized();
            }

            var token = SessionStore.CreateSession("admin", admin.Id);

            _logger.LogInformation("Admin {Name} logged in", admin.Name);
            return Ok(new { adminId = admin.Id, name = admin.Name, token });
        }

        // İlkel giriş kontrolü - gerçek auth (Azure AD/cihaz bazlı) gelince kaldırılacak.
        [HttpPost("courier/login")]
        public async Task<IActionResult> CourierLogin(CourierLoginRequest request)
        {
            var courier = await _db.Couriers.FirstOrDefaultAsync(c => c.Name == request.Name);
            if (courier == null || !PasswordHasher.Verify(request.Password, courier.PasswordHash))
            {
                _logger.LogWarning("Courier login failed for name {Name}", request.Name);
                return Unauthorized();
            }

            var token = SessionStore.CreateSession("courier", courier.Id);

            _logger.LogInformation("Courier {Name} logged in", courier.Name);
            return Ok(new { courierId = courier.Id, name = courier.Name, token });
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _db.Locations.ToListAsync();
            return Ok(locations);
        }

        [HttpPost("locations")]
        public async Task<IActionResult> CreateLocation(LocationRequest request)
        {
            var location = new Location
            {
                Name = request.Name,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ContactPerson = request.ContactPerson,
                ContactPhone = request.ContactPhone
            };

            _db.Locations.Add(location);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Location {LocationId} created ({Name})", location.Id, location.Name);

            return StatusCode(201, location);
        }

        [HttpPut("locations/{id}")]
        public async Task<IActionResult> UpdateLocation(int id, LocationRequest request)
        {
            var location = await _db.Locations.FindAsync(id);
            if (location == null)
                return NotFound();

            location.Name = request.Name;
            location.Latitude = request.Latitude;
            location.Longitude = request.Longitude;
            location.ContactPerson = request.ContactPerson;
            location.ContactPhone = request.ContactPhone;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Location {LocationId} updated", location.Id);

            return Ok(location);
        }

        [HttpDelete("locations/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _db.Locations.FindAsync(id);
            if (location == null)
                return NotFound();

            _db.Locations.Remove(location);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Location {LocationId} deleted", id);

            return NoContent();
        }

        // Admin görünümü: sistemdeki tüm kuryeleri (boşta olanlar dahil) listeler.
        // PasswordHash dışarı hiç verilmiyor (hash olsa bile gereksiz sızıntı).
        [HttpGet("couriers")]
        public async Task<IActionResult> GetCouriers()
        {
            var couriers = await _db.Couriers
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Surname,
                    c.Sex,
                    c.DateOfBirth,
                    c.Phone,
                    c.IsActive,
                    c.LastLat,
                    c.LastLng,
                    c.ActiveVehicleId
                })
                .ToListAsync();

            return Ok(couriers);
        }

        [HttpPost("couriers")]
        public async Task<IActionResult> CreateCourier(CreateCourierRequest request)
        {
            var courier = new Courier
            {
                Name = request.Name,
                Surname = request.Surname,
                Sex = request.Sex,
                DateOfBirth = request.DateOfBirth,
                Phone = request.Phone,
                ActiveVehicleId = request.ActiveVehicleId,
                IsActive = false,
                PasswordHash = PasswordHasher.Hash(request.Password)
            };

            _db.Couriers.Add(courier);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Courier {CourierId} created ({Name} {Surname})", courier.Id, courier.Name, courier.Surname);

            return StatusCode(201, new
            {
                courier.Id,
                courier.Name,
                courier.Surname,
                courier.Sex,
                courier.DateOfBirth,
                courier.Phone,
                courier.IsActive,
                courier.ActiveVehicleId
            });
        }

        [HttpPut("couriers/{id}")]
        public async Task<IActionResult> UpdateCourier(int id, UpdateCourierRequest request)
        {
            var courier = await _db.Couriers.FindAsync(id);
            if (courier == null)
                return NotFound();

            courier.Name = request.Name;
            courier.Surname = request.Surname;
            courier.Sex = request.Sex;
            courier.DateOfBirth = request.DateOfBirth;
            courier.Phone = request.Phone;
            courier.ActiveVehicleId = request.ActiveVehicleId;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Courier {CourierId} updated", courier.Id);

            return Ok(new
            {
                courier.Id,
                courier.Name,
                courier.Surname,
                courier.Sex,
                courier.DateOfBirth,
                courier.Phone,
                courier.IsActive,
                courier.ActiveVehicleId
            });
        }

        [HttpDelete("couriers/{id}")]
        public async Task<IActionResult> DeleteCourier(int id)
        {
            var courier = await _db.Couriers.FindAsync(id);
            if (courier == null)
                return NotFound();

            try
            {
                _db.Couriers.Remove(courier);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // İlişkili kayıtları varsa (paket, sefer geçmişi vb.) silme reddedilir - 500 yerine net cevap.
                return BadRequest("Bu kuryenin ilişkili kayıtları (paket/sefer) olduğu için silinemiyor.");
            }

            _logger.LogInformation("Courier {CourierId} deleted", id);

            return NoContent();
        }

        // Paket burada kuryeye atanmadan, havuza (Pending, AssignedCourierId: null)
        // düşecek şekilde oluşturuluyor - atama ayrı bir işlem.
        [HttpPost("packages")]
        public async Task<IActionResult> CreatePackage(CreatePackageRequest request)
        {
            var package = new Package
            {
                Barcode = request.Barcode,
                Description = request.Description,
                Priority = request.Priority,
                PickupLocId = request.PickupLocationId,
                DropoffLocId = request.DropoffLocationId,
                Status = PackageStatus.Pending
            };

            _db.Packages.Add(package);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Package {PackageId} created (havuzda, atanmamış)", package.Id);

            return StatusCode(201, package);
        }

        // Admin görünümü: tek bir kuryeyle sınırlı değil, tüm paketleri gösterir -
        // Delivered/Cancelled dahil, kuryenin aktif görev listesinden (mypackages) farklı.
        // AssignedCourierId: null olanlar "havuzdaki" paketleri temsil ediyor.
        [HttpGet("packages")]
        public async Task<IActionResult> GetAllPackages()
        {
            var packages = await _db.Packages.ToListAsync();
            return Ok(packages);
        }

        [HttpPut("packages/{id}")]
        public async Task<IActionResult> UpdatePackage(int id, UpdatePackageRequest request)
        {
            var package = await _db.Packages.FindAsync(id);
            if (package == null)
                return NotFound();

            package.Barcode = request.Barcode;
            package.Description = request.Description;
            package.Priority = request.Priority;
            package.PickupLocId = request.PickupLocationId;
            package.DropoffLocId = request.DropoffLocationId;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Package {PackageId} updated", package.Id);

            return Ok(package);
        }

        // Paketi bir kuryeye atar ya da (CourierId: null gönderilirse) havuza geri döndürür.
        [HttpPut("packages/{id}/assign")]
        public async Task<IActionResult> AssignCourier(int id, AssignCourierRequest request)
        {
            var package = await _db.Packages.FindAsync(id);
            if (package == null)
                return NotFound();

            package.AssignedCourierId = request.CourierId;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Package {PackageId} assigned to courier {CourierId}", package.Id, request.CourierId);

            return Ok(package);
        }

        [HttpDelete("packages/{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var package = await _db.Packages.FindAsync(id);
            if (package == null)
                return NotFound();

            try
            {
                _db.Packages.Remove(package);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // PackageHistories -> Package FK'si Restrict, yani geçmişi olan bir
                // paket silinemiyor (chain-of-custody). Kullanıcıya 500 yerine net bir cevap.
                return BadRequest("Bu paketin geçmiş kaydı olduğu için silinemiyor.");
            }

            _logger.LogInformation("Package {PackageId} deleted", id);

            return NoContent();
        }

        [HttpGet("packages/mypackages")]
        public async Task<IActionResult> GetMyPackages([FromQuery] int courierId)
        {
            var packages = await _db.Packages
                .Where(p => p.AssignedCourierId == courierId
                            && p.Status != PackageStatus.Delivered
                            && p.Status != PackageStatus.Cancelled)
                .ToListAsync();

            return Ok(packages);
        }

        // Admin görünümü: chain-of-custody geçmişi - hangi paket, ne zaman,
        // hangi eylemle güncellendi (asla silinmeyen kayıt).
        [HttpGet("packagehistories")]
        public async Task<IActionResult> GetPackageHistories()
        {
            var histories = await _db.PackageHistories.ToListAsync();
            return Ok(histories);
        }

        [HttpPost("packages/syncactions")]
        public async Task<IActionResult> SyncActions(SyncActionsRequest request)
        {
            // Çevrimdışı kuyruktan gelen eylemler varış sırasına göre değil,
            // gerçek zamanına (ActionTime) göre uygulanmalı.
            var orderedActions = request.Actions.OrderBy(a => a.ActionTime).ToList();

            foreach (var action in orderedActions)
            {
                var package = await _db.Packages.FindAsync(action.PackageId);
                if (package == null)
                {
                    _logger.LogWarning("Sync-actions: Package {PackageId} not found, skipped", action.PackageId);
                    continue;
                }

                try
                {
                    if (action.ActionType == PackageActionType.Cancelled)
                        package.Cancel();
                    else
                        package.Status = MapActionToStatus(action.ActionType);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Sync-actions: invalid transition for package {PackageId}", action.PackageId);
                    continue;
                }

                _db.PackageHistories.Add(new PackageHistory
                {
                    JourneyId = request.JourneyId,
                    PackageId = action.PackageId,
                    ActionType = action.ActionType,
                    ActionTime = action.ActionTime,
                    Notes = action.Notes
                });
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Synced {Count} actions for journey {JourneyId}", orderedActions.Count, request.JourneyId);

            return Ok();
        }

        [HttpPost("journeys/start")]
        public async Task<IActionResult> StartJourney(StartJourneyRequest request)
        {
            var journey = new Journey
            {
                CourierId = request.CourierId,
                StartLocId = request.StartLocationId,
                EndLocId = request.EndLocationId,
                StartTime = DateTime.UtcNow,
                Status = JourneyStatus.InProgress
            };

            _db.Journeys.Add(journey);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Journey {JourneyId} started for courier {CourierId}", journey.Id, journey.CourierId);

            return Ok(new { journeyId = journey.Id });
        }

        // Admin görünümü: bir seferin GPS geçmişini, zaman sırasına göre döner
        // (haritada rota çizmek için).
        [HttpGet("telemetry")]
        public async Task<IActionResult> GetTelemetry([FromQuery] int journeyId)
        {
            var points = await _db.TelemetryLogs
                .Where(t => t.JourneyId == journeyId)
                .OrderBy(t => t.Timestamp)
                .ToListAsync();

            return Ok(points);
        }

        [HttpPost("telemetry/batch")]
        public async Task<IActionResult> TelemetryBatch(TelemetryBatchRequest request)
        {
            var journeyId = request.Context.JourneyId;

            // Tünel/offline kuyruktan gelen noktalar varış sırasına göre değil,
            // cihazdaki gerçek zamanına göre kaydedilmeli.
            var points = request.Payload.ActualPathSegment
                .OrderBy(p => p.Timestamp)
                .ToList();

            foreach (var point in points)
            {
                _db.TelemetryLogs.Add(new TelemetryLog
                {
                    JourneyId = journeyId,
                    Latitude = point.Lat,
                    Longitude = point.Lng,
                    Timestamp = point.Timestamp
                });
            }

            // Kuryenin son bilinen konumunu güncelle (canlı haritada hızlı gösterim için).
            var lastPoint = points.LastOrDefault();
            if (lastPoint != null)
            {
                var courier = await _db.Couriers.FindAsync(request.Context.CourierId);
                if (courier != null)
                {
                    courier.LastLat = lastPoint.Lat;
                    courier.LastLng = lastPoint.Lng;
                }
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Stored {Count} telemetry points for journey {JourneyId}", points.Count, journeyId);

            return Ok();
        }

        private static PackageStatus MapActionToStatus(PackageActionType actionType) => actionType switch
        {
            PackageActionType.PickedUp => PackageStatus.PickedUp,
            PackageActionType.InTransit => PackageStatus.InTransit,
            PackageActionType.Delivered => PackageStatus.Delivered,
            PackageActionType.Cancelled => PackageStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(actionType))
        };
    }
}
