using System.Text.Json.Serialization;

namespace PloomesCRM.ERP.Proxy.Domain
{
    public class DestinationAPIRequest
    {
        [JsonPropertyName("body")]
        public string Body {  get; set; }
    }
}
