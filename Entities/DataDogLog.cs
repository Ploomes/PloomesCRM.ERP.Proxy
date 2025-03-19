
using PloomesCRM.ERP.Proxy.Helpers;

namespace PloomesCRM.ERP.Proxy.Entities
{
    public class DataDogLog
    {
        public DataDogLog(string type)
        {
            DataDogTags["type"] = type;
            DataDogTags["env"] = Helper.isDevelopment ? "dev" : "prod";
            if (!Helper.isContainer) DataDogTags["isLocal"] = "true";
            DataDogTags["pod_name"] = Helper.hostname;
            DataDogTags["service"] = "kubernetes";
            DataDogTags["cluster"] = Helper.cluster;
            DataDogTags["gitCommit"] = Helper.gitCommit;
            Id = Guid.NewGuid().ToString("N");
        }

        public string DataDogSource { get; } = Helper.source;
        public string Hostname { get; } = Helper.node + "-" + Helper.cluster;
        public string Service { get; } = Helper.service;
        public Dictionary<string, string> DataDogTags { get; } = new Dictionary<string, string>();
        public string Message { get; set; }
       
        // fields
        public string Id { get; set; }
        public int AccountId { get; set; }
        public DateTime DateTime { get; set; }
        public int ElapsedMilliseconds { get; set; }
        public string Level { get; set; }

        // http
        public string IPAddress { get; set; }
        public string Origin { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Endpoint { get; set; }
        public string QueryString { get; set; }
        public string Headers { get; set; }
        public int HttpStatusCode { get; set; }
        public string RequestBody { get; set; }

        // automations
        public int UserId { get; set; }
        public string EntityType { get; set; }
        public long EntityId { get; set; }
    }
}
