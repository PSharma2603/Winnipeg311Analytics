using System.Text.Json.Serialization;

namespace Winnipeg311Analytics.Models
{
    public class ServiceRequestRecord
    {
        [JsonPropertyName("sr_date")]
        public DateTime? SrDate { get; set; }

        [JsonPropertyName("service_area")]
        public string? ServiceArea { get; set; }

        [JsonPropertyName("service_request")]
        public string? ServiceRequest { get; set; }

        [JsonPropertyName("ward")]
        public string? Ward { get; set; }

        [JsonPropertyName("neighbourhood")]
        public string? Neighbourhood { get; set; }
    }
}