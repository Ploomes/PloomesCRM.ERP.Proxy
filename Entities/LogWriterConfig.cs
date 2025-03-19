namespace PloomesCRM.ERP.Proxy.Entities
{
    public class LogWriterConfig
    {
        public string LogLevel { get; set; }
        public DataDogConfig DataDog { get; set; }
    }

    public class DataDogConfig
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }
        public string ApplicationKey { get; set; }
    }
}
