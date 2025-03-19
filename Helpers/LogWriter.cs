using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ploomes.ERP.Proxy;
using PloomesCRM.ERP.Proxy.Domain.Ploomes;
using PloomesCRM.ERP.Proxy.Entities;

namespace PloomesCRM.ERP.Proxy.Helpers
{
    public static class LogWriter
    {
        //app settings LogWriter object
        public static LogWriterConfig LogWriterConfig { get; set; }

        #region Constants
        //Validate Environment
        public readonly static bool isDevelopment = String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PLOOMESCRMCALLBACKHUB2_MASTER_DEFAULT_SERVICE_HOST"));


        //current log level
        private static readonly LogLevel currentLogLevel =
            Enum.TryParse(Program.LogWriterSection.GetValue<string>("LogLevel") ?? "ERROR", out LogLevel logLevel)
                ? logLevel
                : LogLevel.ERROR;

        //LogWriter datadog app settings
        private static readonly string dataDogUrl = Program.LogWriterSection.GetSection("DataDog").GetValue<string>("Url");
        private static readonly Dictionary<string, string> dataDogHeaders =
            new Dictionary<string, string>()
                {
                    { "DD-API-KEY", Program.LogWriterSection.GetSection("DataDog").GetValue<string>("ApiKey") },
                    { "DD-APPLICATION-KEY", Program.LogWriterSection.GetSection("DataDog").GetValue<string>("ApplicationKey") }
                };

        #endregion

        #region DataDog

        /// <summary>
        /// Sends a request to Ploomes DataDog service. <paramref name="dataDogLog"/>. Message must be not null.
        /// </summary>
        /// <param name="dataDogLog"></param>
        public static async Task SendToDataDog(DataDogLog dataDogLog)
            => await Task.Run(() =>
            {
                if (!String.IsNullOrEmpty(dataDogLog?.Message))
                {
                    PloomesWebRequest.Request(
                        dataDogUrl,
                        "POST",
                        JsonConvert.SerializeObject(new DataDogLogDto(dataDogLog)),
                        dataDogHeaders,
                        log: false
                    );
                }
            });

        #endregion

        #region LogWriter Public Api

        public static async Task Debug(AccountIntegration accountIntegration, string message, string payload = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, message: message, payload: payload), LogLevel.DEBUG, type: type);
        public static async Task Debug(AccountIntegration accountIntegration, HttpContext httpCtx, string message, string payload = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, httpCtx, message: message, payload: payload), LogLevel.DEBUG, type: type);

        public static async Task Info(AccountIntegration accountIntegration, string message, [CallerMemberName] string identifier = null, string type = "log", string accountKey = null, string integrationKey = null)
            => await HandleLogAsync(new Log(accountIntegration, message: message, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.INFO, type: type, accountKey: accountKey, integrationKey: integrationKey);
        public static async Task Info(AccountIntegration accountIntegration, HttpContext httpCtx, string message, [CallerMemberName] string identifier = null, string type = "log", string accountKey = null, string integrationKey = null)
            => await HandleLogAsync(new Log(accountIntegration, httpCtx, message: message, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.INFO, type: type, accountKey: accountKey, integrationKey: integrationKey);

        public static async Task Warning(AccountIntegration accountIntegration, string message, Exception exception = null, bool verbose = false, [CallerMemberName] string identifier = null, string type = "log", string accountKey = null, string integrationKey = null)
            => await HandleLogAsync(new Log(accountIntegration, exception, message, verbose, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.WARNING, type: type, accountKey: accountKey, integrationKey: integrationKey);
        public static async Task Warning(AccountIntegration accountIntegration, HttpContext httpCtx, string message, Exception exception = null, bool verbose = false, [CallerMemberName] string identifier = null, string type = "log", string accountKey = null, string integrationKey = null)
            => await HandleLogAsync(new Log(accountIntegration, httpCtx, exception, message, verbose, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.WARNING, type: type, accountKey: accountKey, integrationKey: integrationKey);

        public static async Task Notice(AccountIntegration accountIntegration, string message, Exception exception = null, bool verbose = false, [CallerMemberName] string identifier = null, string type = "log", string accountKey = null, string integrationKey = null)
            => await HandleLogAsync(new Log(accountIntegration, exception, message, verbose, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.NOTICE, type: type, accountKey: accountKey, integrationKey: integrationKey);
        public static async Task Notice(AccountIntegration accountIntegration, HttpContext httpCtx, string message, Exception exception = null, bool verbose = false, [CallerMemberName] string identifier = null, string type = "log", string accountKey = null, string integrationKey = null)
            => await HandleLogAsync(new Log(accountIntegration, httpCtx, exception, message, verbose, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.NOTICE, type: type, accountKey: accountKey, integrationKey: integrationKey);

        public static async Task Error(AccountIntegration accountIntegration, Exception e, string url = "", string payload = null, [CallerMemberName] string identifier = null, string integrationKey = null, string accountKey = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, e, verbose: true, url: url, payload: payload, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.ERROR, integrationKey, accountKey, type: type);
        public static async Task Error(AccountIntegration accountIntegration, HttpContext httpCtx, Exception e, string url = "", string payload = null, [CallerMemberName] string identifier = null, string integrationKey = null, string accountKey = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, httpCtx, e, verbose: true, url: url, payload: payload, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.ERROR, integrationKey, accountKey, type: type);

        public static async Task Success(AccountIntegration accountIntegration, string message, [CallerMemberName] string identifier = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, message: message, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.SUCCESS, type: type);
        public static async Task Success(AccountIntegration accountIntegration, HttpContext httpCtx, string message, [CallerMemberName] string identifier = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, httpCtx, message: message, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.SUCCESS, type: type);

        public static async Task Failure(AccountIntegration accountIntegration, Exception e, [CallerMemberName] string identifier = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, e, verbose: true, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.FAILURE, type: type);
        public static async Task Failure(AccountIntegration accountIntegration, HttpContext httpCtx, Exception e, [CallerMemberName] string identifier = null, string type = "log")
            => await HandleLogAsync(new Log(accountIntegration, httpCtx, e, verbose: true, tags: new Dictionary<string, string>() { { "identifier", identifier ?? GetCallerMethod() } }), LogLevel.FAILURE, type: type);

        public static string WriteException(AccountIntegration accountIntegration, Exception e)
            => WriteExceptionLog(new Log(accountIntegration, e), LogLevel.ERROR);

        #endregion

        #region LogWriter Private Methods

        private static string GetCallerMethod() => new StackTrace().GetFrame(2).GetMethod().Name;

        private static async Task<DataDogLog> AddHttpRequestData(DataDogLog ddLog, HttpContext httpCtx, string reqBody = null)
        {
            Task<string> bodyReaderTask = null;
            if (reqBody == null)
            {
                httpCtx.Request.Body.Seek(0, SeekOrigin.Begin);
                bodyReaderTask = new StreamReader(httpCtx.Request.Body, Encoding.UTF8, true, 1024, true).ReadToEndAsync();
            }

            ddLog.IPAddress = httpCtx.Connection.RemoteIpAddress.ToString();
            ddLog.Origin = httpCtx.Request.Headers["User-Agent"].ToString();
            ddLog.Method = httpCtx.Request.Method;
            ddLog.Url = $"{httpCtx.Request.Scheme}://{httpCtx.Request.Host}{httpCtx.Request.PathBase}{httpCtx.Request.Path}{httpCtx.Request.QueryString}";
            ddLog.Endpoint = httpCtx.Request.Path.ToString();
            ddLog.QueryString = httpCtx.Request.QueryString.ToString();

            var headersJson = new JObject();
            foreach (var h in httpCtx.Request.Headers)
                if (!h.Key.EndsWith("key"))
                    headersJson.Add(h.Key, h.Value.ToString());
            ddLog.Headers = JsonConvert.SerializeObject(headersJson, Formatting.Indented);

            ddLog.HttpStatusCode = httpCtx.Response.StatusCode;
            string body = reqBody != null ? reqBody : await bodyReaderTask;
            if (body != null)
            {
                try
                {
                    var bodyJObject = JObject.Parse(body);
                    foreach (var prop in bodyJObject.DescendantsAndSelf().OfType<JProperty>().Where(p => p.Value.Type == JTokenType.String && p.Value.ToString().Length > 5000))
                        prop.Value = "<ploomescrmcallbackhub2::propriedade minimizada>";
                    ddLog.RequestBody = JsonConvert.SerializeObject(bodyJObject, Formatting.Indented);
                }
                catch (Exception)
                {
                    ddLog.RequestBody = body;
                }
            }

            try
            {
                var bodyJObject = JObject.Parse(body);
                if (bodyJObject.ContainsKey("ActionUserId"))
                {
                    var actionUserId = (int)bodyJObject["ActionUserId"];
                    ddLog.UserId = actionUserId;
                    ddLog.EntityType = bodyJObject["Entity"].ToString();
                    if (bodyJObject["New"].Type != JTokenType.Null)
                        ddLog.EntityId = (long)bodyJObject["New"]["Id"];
                    else
                        ddLog.EntityId = (long)bodyJObject["Old"]["Id"];
                }
            }
            catch { }

            httpCtx.Request.Body.Seek(0, SeekOrigin.Begin);

            return ddLog;
        }

        private static int GetRequestDuration(HttpContext httpContext)
        {
            var startTime = httpContext.Items["ReqStartTime"] as DateTime?;
            if (startTime == null)
                return 0;

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime.Value;
            return (int)duration.TotalMilliseconds;
        }

        private static async Task HandleLogAsync(Log log, LogLevel level, string integrationKey = null, string accountKey = null, string type = "log")
        {
            string logString = null;
            if (level >= currentLogLevel)
            {
                logString = WriteExceptionLog(log, level);
                Console.WriteLine(logString);
            }

            if (level >= LogLevel.INFO)
            {
                logString ??= WriteExceptionLog(log, level);
                DataDogLog ddLog = new DataDogLog(type) { Message = logString };

                if (log.HttpContext != null)
                {
                    try
                    {
                        ddLog.ElapsedMilliseconds = GetRequestDuration(log.HttpContext);
                        ddLog = await AddHttpRequestData(ddLog, log.HttpContext);
                    }
                    catch
                    {

                    }
                }

                if (log.AccountIntegration != null)
                {
                    ddLog.AccountId = log?.AccountIntegration?.AccountId ?? 0;

                    ddLog.DataDogTags.Add("accountKey", log.AccountIntegration.Key);
                    ddLog.DataDogTags.Add("integration", log.AccountIntegration?.Integration?.Key ?? "null");
                }
                else
                {
                    ddLog.DataDogTags.Add("accountKey", accountKey);
                    ddLog.DataDogTags.Add("integration", integrationKey ?? "null");
                }

                ddLog.Level = level != LogLevel.FAILURE ? level.ToString() : "error";
                ddLog.DataDogTags.Add("logLevel", level.ToString());

                foreach (var t in log.Tags) ddLog.DataDogTags[t.Key] = t.Value;

                if (!isDevelopment)
                    SendToDataDog(ddLog);
                //SendToCosmos(log.AccountIntegration, log.Url ?? "", new JObject() { { "content", logString } }, additionalMessages: log.Message, e: log.Exception);
            }
        }

        private static string WriteExceptionLog(Log log, LogLevel level = LogLevel.ERROR)
        {
            if (log == null)
                return "";

            StringBuilder logString = new StringBuilder().AppendLine();
            logString.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ");

            if (log.AccountIntegration?.Integration?.Key != null)
                logString.Append($"[{log.AccountIntegration.Integration?.Key}] ");

            logString.Append($"[{level}] ");

            if (log.Exception != null)
                logString.Append($"({log.Exception?.GetType()?.Name ?? "null"}) ");

            logString.Append($"Message: {(log.Message ?? log.Exception?.Message ?? "null")} ");

            if (log.AccountIntegration != null)
                logString.Append($"#{{{log.AccountIntegration.Key}}}");

            logString.AppendLine();

            if (log.Payload != null)
                logString.AppendLine(log.Payload);

            if (log.Exception != null)
            {
                if (log.Verbose)
                {
                    logString.AppendLine($"Exception StackTrace:\r\n{log.Exception?.StackTrace ?? "null"}");
                    logString.AppendLine($"Full StackTrace:\r\n{new System.Diagnostics.StackTrace()?.ToString() ?? "null"}");
                }
                if (log.Exception.Data?.Count > 0)
                {
                    logString.AppendLine(ExceptionDataToString(log.Exception.Data));
                }
            }

            return logString.ToString();
        }

        private static string ExceptionDataToString(System.Collections.IDictionary exceptionData)
        {
            StringBuilder result = new StringBuilder("\r\nData:\r\n");

            foreach (var key in exceptionData.Keys)
                result.AppendLine($"   Data[\"{key}\"] = {exceptionData[key] ?? "null"}");

            return result.ToString();
        }

        private class Log
        {
            public AccountIntegration AccountIntegration { get; private set; }
            public HttpContext HttpContext { get; private set; }
            public Exception Exception { get; private set; }
            public string Message { get; private set; }
            public bool Verbose { get; private set; }
            public string Url { get; private set; }
            public string Payload { get; private set; }
            public Dictionary<string, string> Tags { get; private set; }

            public Log(AccountIntegration accountIntegration, Exception exception = null, string message = null, bool verbose = false, string url = "", string payload = null, Dictionary<string, string> tags = null)
            {
                AccountIntegration = accountIntegration;
                Exception = exception;
                Message = message;
                Verbose = verbose;
                Url = url;
                Payload = payload;
                Tags = tags ?? new Dictionary<string, string>();
            }

            public Log(AccountIntegration accountIntegration, HttpContext httpContext, Exception exception = null, string message = null, bool verbose = false, string url = "", string payload = null, Dictionary<string, string> tags = null)
            {
                AccountIntegration = accountIntegration;
                HttpContext = httpContext;
                Exception = exception;
                Message = message;
                Verbose = verbose;
                Url = url;
                Payload = payload;
                Tags = tags ?? new Dictionary<string, string>();
            }
        }

        private enum LogLevel
        {
            DEBUG = 0,
            INFO = 1,
            WARNING = 2,
            ERROR = 3,
            SUCCESS = 4,
            FAILURE = 5,
            NOTICE = 6
        }

        #endregion
    }
}
