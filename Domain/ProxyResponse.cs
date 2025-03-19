using System.Net;
using System.Text.Json.Serialization;

namespace PloomesCRM.ERP.Proxy.Domain
{
    public class ProxyResponse
    {
        [JsonPropertyName("destination_status_code")]
        public HttpStatusCode DestinationStatusCode { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
