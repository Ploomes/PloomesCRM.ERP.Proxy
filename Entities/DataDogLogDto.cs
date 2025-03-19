using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PloomesCRM.ERP.Proxy.Entities
{
    public class DataDogLogDto
    {
        public DataDogLogDto(DataDogLog ddl)
        {
            DataDogSource = ddl.DataDogSource;
            DataDogTags = ddl.DataDogTags.Aggregate(new StringBuilder(), (acc, cur) => acc.Append($"{cur.Key}:{ cur.Value},"), sb => sb.ToString()[..^1]);
            Hostname = ddl.Hostname;
            Message = ddl.Message;
            Service = ddl.Service;

            Id = ddl.Id;
            AccountId = ddl.AccountId;
            DateTime = DateTime.Now;
            ElapsedMilliseconds = ddl.ElapsedMilliseconds;
            Level = ddl.Level;

            IPAddress = ddl.IPAddress;
            Origin = ddl.Origin;
            Url = ddl.Url;
            Endpoint = ddl.Endpoint;
            Method = ddl.Method;
            QueryString = ddl.QueryString;
            Headers = ddl.Headers;
            HttpStatusCode = ddl.HttpStatusCode;
            RequestBody = ddl.RequestBody;
            RequestBodyLength = ddl.RequestBody?.Length ?? 0;

            UserId = ddl.UserId;
            EntityType = ddl.EntityType;
            EntityId = ddl.EntityId;
        }

        [JsonProperty("ddsource")]
        public string DataDogSource { get; set; }

        [JsonProperty("ddtags")]
        public string DataDogTags { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        // fields

        [JsonProperty("Id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }
        
        [JsonProperty("AccountId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AccountId { get; set; }
        
        [JsonProperty("DateTime")]
        public DateTime DateTime { get; set; }
        
        [JsonProperty("ElapsedMilliseconds")]
        public int ElapsedMilliseconds { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }
        
        // http

        [JsonProperty("IPAddress", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string IPAddress { get; set; }

        [JsonProperty("Origin", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Origin { get; set; }
        
        [JsonProperty("Method", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Method { get; set; }
        
        [JsonProperty("Url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url { get; set; }
        
        [JsonProperty("Endpoint", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Endpoint { get; set; }
        
        [JsonProperty("QueryString", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string QueryString { get; set; }

        [JsonProperty("Headers", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Headers { get; set; }

        [JsonProperty("HttpStatusCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int HttpStatusCode { get; set; }
        
        [JsonProperty("RequestBody", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string RequestBody { get; set; }

        [JsonProperty("RequestBodyLength", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int RequestBodyLength { get; set; }

        // automations
        
        [JsonProperty("UserId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int UserId { get; set; }
        
        [JsonProperty("EntityType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string EntityType { get; set; }
        
        [JsonProperty("EntityId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long EntityId { get; set; }
    }
}
