using System.Text.RegularExpressions;

namespace PloomesCRM.ERP.Proxy.Helpers
{
    public static class Helper
    {
        private readonly static DateTime MIN_VALUE = Convert.ToDateTime("1800-01-01");

        public readonly static bool isDevelopment = String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PLOOMESCRMCALLBACKHUB2_MASTER_DEFAULT_SERVICE_HOST"));

        public readonly static bool isContainer = !String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));

        public readonly static string source = "integrations.hub";

        public readonly static string service = Environment.GetEnvironmentVariable("INSTANCE_NAME") ?? "local";

        public readonly static string node = Environment.GetEnvironmentVariable("NODE_NAME") ?? "unknown";

        public readonly static string cluster = Environment.GetEnvironmentVariable("CLUSTER_NAME") ?? "unknown";

        public readonly static string hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.GetEnvironmentVariable("COMPUTERNAME");

        public readonly static string gitCommit = Environment.GetEnvironmentVariable("GIT_COMMIT") ?? "local";

        public static string GetQueryString(string url)
        {
            string result = "";
            try
            {
                result = Regex.Matches(url, @".+\?(.+)").Last().Groups[1].Value;
            }
            catch { }

            return result;
        }
    }
}
