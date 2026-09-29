using System.Text.Json;
using Winnipeg311Analytics.Models;

namespace Winnipeg311Analytics.Services
{
    public class City311Service
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public City311Service(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<ServiceRequestRecord>> GetServiceRequestsAsync(
            string? ward = null,
            string? serviceArea = null)
        {
            var baseUrl =
                _configuration["City311Api:BaseUrl"];

            var appToken =
                _configuration["City311Api:AppToken"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new Exception(
                    "City311Api BaseUrl is missing."
                );
            }

            var queryParts = new List<string>
            {
                "$limit=100",
                "$order=sr_date DESC"
            };

            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(ward))
            {
                var safeWard =
                    ward.Replace("'", "''");

                conditions.Add(
                    $"ward='{safeWard}'"
                );
            }

            if (!string.IsNullOrWhiteSpace(serviceArea))
            {
                var safeServiceArea =
                    serviceArea.Replace("'", "''");

                conditions.Add(
                    $"service_area='{safeServiceArea}'"
                );
            }

            if (conditions.Count > 0)
            {
                var where =
                    string.Join(
                        " AND ",
                        conditions
                    );

                queryParts.Add(
                    "$where=" +
                    Uri.EscapeDataString(where)
                );
            }

            var url =
                baseUrl +
                "?" +
                string.Join("&", queryParts);

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                );

            if (!string.IsNullOrWhiteSpace(appToken))
            {
                request.Headers.Add(
                    "X-App-Token",
                    appToken
                );
            }

            var response =
                await _httpClient.SendAsync(
                    request
                );

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content
                    .ReadAsStringAsync();

            var records =
                JsonSerializer.Deserialize<
                    List<ServiceRequestRecord>
                >(json);

            return records ??
                   new List<ServiceRequestRecord>();
        }


        public async Task<List<string>> GetWardsAsync()
        {
            var baseUrl =
                _configuration["City311Api:BaseUrl"];

            var appToken =
                _configuration["City311Api:AppToken"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new Exception(
                    "City311Api BaseUrl is missing."
                );
            }

            var url =
                $"{baseUrl}" +
                "?$select=ward" +
                "&$group=ward" +
                "&$order=ward";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                );

            if (!string.IsNullOrWhiteSpace(appToken))
            {
                request.Headers.Add(
                    "X-App-Token",
                    appToken
                );
            }

            var response =
                await _httpClient.SendAsync(
                    request
                );

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content
                    .ReadAsStringAsync();

            var data =
                JsonSerializer.Deserialize<
                    List<Dictionary<string, string>>
                >(json);

            return data?
                .Where(
                    x => x.ContainsKey("ward")
                )
                .Select(
                    x => x["ward"]
                )
                .Where(
                    x => !string.IsNullOrWhiteSpace(x)
                )
                .ToList()
                ?? new List<string>();
        }


        public async Task<List<string>> GetServiceAreasAsync()
        {
            var baseUrl =
                _configuration["City311Api:BaseUrl"];

            var appToken =
                _configuration["City311Api:AppToken"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new Exception(
                    "City311Api BaseUrl is missing."
                );
            }

            var url =
                $"{baseUrl}" +
                "?$select=service_area" +
                "&$group=service_area" +
                "&$order=service_area";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                );

            if (!string.IsNullOrWhiteSpace(appToken))
            {
                request.Headers.Add(
                    "X-App-Token",
                    appToken
                );
            }

            var response =
                await _httpClient.SendAsync(
                    request
                );

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content
                    .ReadAsStringAsync();

            var data =
                JsonSerializer.Deserialize<
                    List<Dictionary<string, string>>
                >(json);

            return data?
                .Where(
                    x => x.ContainsKey(
                        "service_area"
                    )
                )
                .Select(
                    x => x["service_area"]
                )
                .Where(
                    x => !string.IsNullOrWhiteSpace(x)
                )
                .ToList()
                ?? new List<string>();
        }


        public async Task<int> GetWardRequestCountAsync(
            string ward)
        {
            var baseUrl =
                _configuration["City311Api:BaseUrl"];

            var appToken =
                _configuration["City311Api:AppToken"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new Exception(
                    "City311Api BaseUrl is missing."
                );
            }

            if (string.IsNullOrWhiteSpace(ward))
            {
                return 0;
            }

            var safeWard =
                ward.Replace("'", "''");

            var where =
                $"ward='{safeWard}'";

            var url =
                $"{baseUrl}" +
                "?$select=count(*) as count" +
                "&$where=" +
                Uri.EscapeDataString(where);

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                );

            if (!string.IsNullOrWhiteSpace(appToken))
            {
                request.Headers.Add(
                    "X-App-Token",
                    appToken
                );
            }

            var response =
                await _httpClient.SendAsync(
                    request
                );

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content
                    .ReadAsStringAsync();

            var data =
                JsonSerializer.Deserialize<
                    List<Dictionary<string, string>>
                >(json);

            if (
                data != null &&
                data.Count > 0 &&
                data[0].TryGetValue(
                    "count",
                    out var countValue
                ) &&
                int.TryParse(
                    countValue,
                    out var count
                )
            )
            {
                return count;
            }

            return 0;
        }
    }
}