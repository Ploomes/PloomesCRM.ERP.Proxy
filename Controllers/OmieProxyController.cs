using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using PloomesCRM.ERP.Proxy.Domain.Ploomes;
using PloomesCRM.ERP.Proxy.Helpers;

namespace PloomesCRM.ERP.Proxy.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class OmieProxyController : ProxyController
    {
        public OmieProxyController(ILogger<ProxyController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
            : base(logger, configuration, httpClientFactory) { }

        protected override async Task<IActionResult> InternalForwardRequest(HttpMethod httpMethod, HttpRequest originalResquest, object requestBody)
        {
            AccountIntegration accountIntegration = new AccountIntegration();

            try
            {
                accountIntegration = AccountIntegrationFromHeaders(Request.Headers);

                if (Request.Headers.TryGetValue("MyOmieUri", out var myOmieUri))
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    httpClient.BaseAddress = new Uri(_configuration["BaseAdresses:Omie"]);

                    string jsonBody = JsonSerializer.Serialize(requestBody);

                    LogWriter.Info(accountIntegration, $"{httpMethod.Method} - OmieProxy request received. {Environment.NewLine}" +
                            $"Endpoint: {myOmieUri} {Environment.NewLine}" +
                            $"Body: {jsonBody} {Environment.NewLine}");

                    var requestMessage = new HttpRequestMessage(httpMethod, myOmieUri[0]);

                    if (jsonBody != "null")
                    {
                        requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    }

                    foreach (var header in Request.Headers)
                    {
                        if (header.Key != "Host" &&
                            header.Key != "MyOmieUri" &&
                            !(header.Key.Equals("AccountId") || header.Key.Equals("AccountKey") ||
                            header.Key.Equals("IntegrationId") || header.Key.Equals("IntegrationKey")))
                        {
                            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                        }
                    }

                    var response = await httpClient.SendAsync(requestMessage);

                    string responseBody = string.Empty;
                    if (response != null)
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        LogWriter.Success(accountIntegration, $"{httpMethod.Method} - OmieProxy request successfull. {Environment.NewLine}" +
                            $"Endpoint: {myOmieUri} {Environment.NewLine}" +
                            $"Body: {jsonBody} {Environment.NewLine}");

                        return Content(responseBody);
                    }
                    else
                    {
                        LogWriter.Warning(accountIntegration, $"{httpMethod.Method} - OmieProxy request failed. {Environment.NewLine}" +
                            $"StatusCode: {response.StatusCode.GetHashCode()} {Environment.NewLine}" +
                            $"Endpoint: {myOmieUri} {Environment.NewLine}");

                        return Problem(responseBody, statusCode: response.StatusCode.GetHashCode());
                    }
                }
                else
                {
                    return BadRequest("Header 'MyOmieUri' not found.");
                }
            }
            catch (Exception ex)
            {
                LogWriter.Error(accountIntegration, ex);
                return Problem(ex.Message, statusCode: HttpStatusCode.InternalServerError.GetHashCode());
            }
        }

        protected override AccountIntegration AccountIntegrationFromHeaders(IDictionary<string, StringValues> headers)
        {
            headers.TryGetValue("AccountId", out StringValues accountId);
            headers.TryGetValue("AccountKey", out StringValues AccountKey);
            headers.TryGetValue("IntegrationId", out StringValues integrationId);
            headers.TryGetValue("IntegrationKey", out StringValues IntegrationKey);

            return new AccountIntegration()
            {
                AccountId = int.Parse(accountId),
                Key = AccountKey,
                Integration = new Integration()
                {
                    Id = int.Parse(integrationId),
                    Key = IntegrationKey
                }

            };
        }
    }
}
