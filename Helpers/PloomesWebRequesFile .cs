using System.IO;

namespace PloomesCRM.ERP.Proxy.Helpers
{
    public class PloomesWebRequestFile
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream ContentStream { get; set; }

        public PloomesWebRequestFile(string fileName, string contentType, Stream contentStream)
        {
            FileName = fileName;
            ContentType = contentType;
            ContentStream = contentStream;
        }
    }
}
