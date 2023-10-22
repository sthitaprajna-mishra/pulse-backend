using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace CloseConnectv1.Services
{
    public class APIClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public APIClientService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<APIResponse> GetApiResponseAsync(string endpoint, string? queryParams = null)
        {
            // Retrieve the access key from the configuration
            string? apiKey = _configuration.GetSection("MediaStackConfig:MEDIASTACK_ACCESS_KEY").Value;

            // Append the access key as a query parameter to the endpoint
            string apiUrl = $"{endpoint}?access_key={apiKey}";

            if (queryParams is not null)
            {
                // "&languages=en&countries=in&sort=published_desc&limit=10"
                apiUrl = string.Concat(apiUrl, queryParams);
            }           

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                APIResponse result = new()
                {
                    Content = content
                };
                return result;
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                string json = File.ReadAllText("fallbackData.json");
                APIResponse result = new()
                {
                    StatusCode = HttpStatusCode.TooManyRequests,
                    Content = json
                };
                return result;
            }
            else
            {
                // Handle error here if needed
                return null;
            }
        }
    }
}
