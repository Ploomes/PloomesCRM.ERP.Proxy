using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PloomesCRM.ERP.Proxy.Domain.Ploomes;
using PloomesCRM.ERP.Proxy.Entities;

namespace PloomesCRM.ERP.Proxy.Helpers
{
    static class PloomesWebRequest
    {
        public static string Request(string url, string method, string requestBody = null,
            Dictionary<string, string> headers = null, string accept = null,
            SecurityProtocolType type =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12,
            string contentType = "application/json", bool log = true, AccountIntegration logAccountIntegration = null,
            int timeout = 600000)
        {
            ServicePointManager.SecurityProtocol = type;
            HttpWebRequest wrq;
            byte[] byteArray;
            Stream dataStream;
            WebResponse wrs;
            string response = null;

            wrq = (HttpWebRequest)WebRequest.Create(url);
            wrq.Timeout = timeout;
            wrq.Method = method;
            wrq.ContentType = contentType;
            if (accept != null)
                wrq.Accept = accept;

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    wrq.Headers.Add(header.Key, header.Value);
                }
            }

            wrq.Headers.Add("User-Agent", "integrations.cb2");

            if (requestBody != null)
            {
                byteArray = Encoding.UTF8.GetBytes(requestBody);
                wrq.ContentLength = byteArray.Length;
                dataStream = wrq.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            try
            {
                try
                {
                    var path = new Uri(url).AbsolutePath;
                    if (log && Helper.isContainer && !"GET".Equals(method))
                    {
                        DataDogLog ddLog = new DataDogLog("outgoingRequest") { Message = $"Outgoing {method} {url}" };
                        ddLog.Method = method;
                        ddLog.Url = url;
                        var headersJson = new JObject();
                        foreach (var h in headers.Where(h => !h.Key.ToLower().EndsWith("key")))
                            headersJson.Add(h.Key, h.Value.ToString());
                        ddLog.Headers = JsonConvert.SerializeObject(headersJson, Formatting.Indented);
                        ddLog.QueryString = Helper.GetQueryString(url);
                        try
                        {
                            var bodyJObject = JObject.Parse(requestBody);
                            foreach (var prop in bodyJObject.DescendantsAndSelf().OfType<JProperty>().Where(p =>
                                         p.Value.Type == JTokenType.String && p.Value.ToString().Length > 5000))
                                prop.Value = "<ploomescrmcallbackhub2::propriedade minimizada>";
                            ddLog.RequestBody = JsonConvert.SerializeObject(bodyJObject, Formatting.Indented);
                        }
                        catch (Exception)
                        {
                            ddLog.RequestBody = requestBody;
                        }

                        ddLog.Endpoint = path;
                        ddLog.DateTime = DateTime.Now;
                        if (logAccountIntegration != null)
                        {
                            ddLog.AccountId = logAccountIntegration.AccountId;
                            ddLog.DataDogTags.Add("accountKey", logAccountIntegration?.Key);
                            ddLog.DataDogTags.Add("integration", logAccountIntegration?.Integration?.Key ?? "null");
                        }

                        LogWriter.SendToDataDog(ddLog);
                    }
                }
                catch
                {
                }

                wrs = wrq.GetResponse();
            }
            catch (Exception e)
            {
                if (log)
                {
                    e.Data["url"] = url;
                    e.Data["method"] = method;
                    if (headers != null)
                        e.Data["headers"] = headers.Where(h => !h.Key.ToLower().EndsWith("key")).Aggregate("", (acc, h) => acc += h.Key + ": " + h.Value + "\r\n", acc => acc);
                    if (requestBody != null)
                    {
                        try
                        {
                            var bodyJObject = JObject.Parse(requestBody);
                            if (e.Data["url"].ToString().Contains("app.omie.com.br"))
                            {
                                bodyJObject.Remove("app_key");
                                bodyJObject.Remove("app_secret");
                            }
                            foreach (var prop in bodyJObject.DescendantsAndSelf().OfType<JProperty>().Where(p =>
                                         p.Value.Type == JTokenType.String && p.Value.ToString().Length > 5000))
                                prop.Value = "<ploomescrmcallbackhub2::propriedade minimizada>";
                            e.Data["requestBody"] = JsonConvert.SerializeObject(bodyJObject, Formatting.Indented);
                        }
                        catch (Exception)
                        {
                            e.Data["requestBody"] = requestBody;
                        }
                    }

                    if (e is WebException webException)
                    {
                        e.Data["webExceptionResponseBody"] =
                            new StreamReader(webException.Response?.GetResponseStream() ?? Stream.Null).ReadToEnd();
                        if (((HttpWebResponse)webException.Response)?.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            LogWriter.Warning(logAccountIntegration, "Unathorized", e);
                        }
                        else
                        {
                            LogWriter.Error(logAccountIntegration, e);
                        }
                    }
                    else
                    {
                        LogWriter.Error(logAccountIntegration, e);
                    }
                }
                throw e;
            }

            response = new StreamReader(wrs.GetResponseStream()).ReadToEnd();
            wrs.Close();

            return response;
        }

        public static string MultipartRequest(string url, string method, Dictionary<string, string> textInputs = null,
            Dictionary<string, PloomesWebRequestFile> fileInputs = null, Dictionary<string, string> headers = null)
        {
            WebRequest wrq;
            Stream dataStream;
            WebResponse wrs;
            Stream s;
            StreamReader rd;
            string response = null;

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] boundaryFooterBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

            wrq = WebRequest.Create(url);
            wrq.Method = method;
            wrq.ContentType = "multipart/form-data; boundary=" + boundary;

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    wrq.Headers.Add(header.Key, header.Value);
                }
            }

            dataStream = wrq.GetRequestStream();

            if (textInputs != null)
            {
                string contentDispositionTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (KeyValuePair<string, string> textInput in textInputs)
                {
                    string contentDisposition =
                        string.Format(contentDispositionTemplate, textInput.Key, textInput.Value);
                    byte[] contentDispositionBytes = Encoding.UTF8.GetBytes(contentDisposition);

                    dataStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    dataStream.Write(contentDispositionBytes, 0, contentDispositionBytes.Length);
                }
            }

            if (fileInputs != null)
            {
                string contentDispositionTemplate =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                ;
                foreach (KeyValuePair<string, PloomesWebRequestFile> fileInput in fileInputs)
                {
                    string contentDisposition = string.Format(contentDispositionTemplate, fileInput.Key,
                        fileInput.Value.FileName, fileInput.Value.ContentType);
                    byte[] contentDispositionBytes = Encoding.UTF8.GetBytes(contentDisposition);

                    dataStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    dataStream.Write(contentDispositionBytes, 0, contentDispositionBytes.Length);

                    Stream contentStream = fileInput.Value.ContentStream;
                    byte[] contentBytes = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = contentStream.Read(contentBytes, 0, contentBytes.Length)) != 0)
                    {
                        dataStream.Write(contentBytes, 0, bytesRead);
                    }
                }
            }

            dataStream.Write(boundaryFooterBytes, 0, boundaryFooterBytes.Length);

            dataStream.Close();

            wrs = wrq.GetResponse();
            s = wrs.GetResponseStream();
            rd = new StreamReader(s);

            response = rd.ReadToEnd();

            rd.Close();
            s.Close();
            wrs.Close();

            return response;
        }

        public static bool TryRequest(out JObject objRequest, string url, string method, string requestBody = null,
            Dictionary<string, string> headers = null, string accept = null,
            SecurityProtocolType type =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12,
            string contentType = "application/json", bool log = true)
        {
            objRequest = null;

            string request = Request(url, method, requestBody, headers, accept, type, contentType, log);

            if (!String.IsNullOrEmpty(request))
            {
                objRequest = JObject.Parse(request);
                return true;
            }

            return false;
        }
    }
}