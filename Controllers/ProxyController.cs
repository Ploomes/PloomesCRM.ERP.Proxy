using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using PloomesCRM.ERP.Proxy.Domain;
using PloomesCRM.ERP.Proxy.Domain.Ploomes;

namespace PloomesCRM.ERP.Proxy.Controllers
{
    public abstract class ProxyController : ControllerBase
    {
        protected ILogger<ProxyController> _logger;
        protected IConfiguration _configuration;
        protected IHttpClientFactory _httpClientFactory;

        public ProxyController(ILogger<ProxyController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public Task<IActionResult> Post([FromBody] object responseBody) => InternalForwardRequest(HttpMethod.Post, Request, responseBody);

        [HttpGet]
        public Task<IActionResult> Get([FromBody] object responseBody) => InternalForwardRequest(HttpMethod.Get, Request, responseBody);

        protected virtual Task<IActionResult> InternalForwardRequest(HttpMethod httpMethod, HttpRequest originalResquest, object requestBody)
        {
            throw new NotImplementedException(nameof(InternalForwardRequest));
        }

        protected virtual AccountIntegration AccountIntegrationFromHeaders(IDictionary<string, StringValues> headers)
        {
            throw new NotImplementedException(nameof(AccountIntegrationFromHeaders));
        }
    }
}
