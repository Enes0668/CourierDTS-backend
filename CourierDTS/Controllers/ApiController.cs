using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourierDTS.Data;
using CourierDTS.Dtos;
using CourierDTS.Models;

namespace CourierDTS.Controllers
{
    // Mühendisin isteği: katmanlara/ayrı controller'lara bölmeden, tüm endpoint'ler tek yerde.
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ApiController> _logger;

        public ApiController(AppDbContext db, ILogger<ApiController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _db.Locations.ToListAsync();
            return Ok(locations);
        }

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
                AssignedCourierId = request.AssignedCourierId,
                Status = PackageStatus.Pending
            };

            _db.Packages.Add(package);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Package {PackageId} created (assigned courier: {CourierId})", package.Id, package.AssignedCourierId);

            return StatusCode(201, package);
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
