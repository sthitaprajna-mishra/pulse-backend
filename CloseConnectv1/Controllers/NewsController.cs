using CloseConnectv1.Filters;
using CloseConnectv1.Services;
using CloseConnectv1.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace CloseConnectv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(TokenValidationFilter))]
    public class NewsController : ControllerBase
    {
        private readonly APIClientService _apiClientService;

        public NewsController(APIClientService apiClientService)
        {
            _apiClientService = apiClientService;
        }

        [HttpGet("GetNews")]
        public async Task<IActionResult> GetNews([FromQuery] string languages = "en", [FromQuery] string countries = "in", [FromQuery] string sort = "published_desc", [FromQuery] int limit = 100)
        {
            string endpoint = Constants.MEDIASTACK_BASEURL; 

            try
            {
                string queryParams = $"&languages={languages}&countries={countries}&sort={sort}&limit={limit}";
                APIResponse response = await _apiClientService.GetApiResponseAsync(endpoint, queryParams);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle the exception and return an error response
                return BadRequest(ex.Message);
            }
        }
    }
}
