using System.Collections.Generic;

namespace PloomesCRM.ERP.Proxy.Domain.Ploomes
{
    public class Integration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string Key { get; set; }
        public int TypeId { get; set; }
    }
}