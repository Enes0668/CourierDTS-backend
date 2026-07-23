using System.Text.Json.Serialization;

namespace CourierDTS.Dtos
{
    // POST /api/telemetry/batch - mobil taraf snake_case JSON gönderiyor.
    public class TelemetryBatchRequest
    {
        [JsonPropertyName("event_name")]
        public string EventName { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public TelemetryContext Context { get; set; } = null!;

        public TelemetryPayload Payload { get; set; } = null!;
    }

    public class TelemetryContext
    {
        [JsonPropertyName("courier_id")]
        public int CourierId { get; set; }

        [JsonPropertyName("journey_id")]
        public int JourneyId { get; set; }
    }

    public class TelemetryPayload
    {
        [JsonPropertyName("actual_path_segment")]
        public List<TelemetryPoint> ActualPathSegment { get; set; } = new();
    }

    public class TelemetryPoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
