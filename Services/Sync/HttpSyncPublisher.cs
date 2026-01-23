using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TicketManagementSystem.Server.DTOs.Sync;
using System.Text.Json;
using System.Net.Http.Headers;

namespace TicketManagementSystem.Server.Services.Sync
{
    public class HttpSyncPublisher : ISyncPublisher
    {
        private readonly IConfiguration _configuration;
        private string? _cachedToken;
        public HttpSyncPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        private async Task<bool> EnsureAuthAsync(HttpClient client, string baseUrl)
        {
            var token = _cachedToken ?? _configuration["StarApi:SyncBearerToken"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return true;
            }

            var email = _configuration["StarApi:SyncEmail"];
            var password = _configuration["StarApi:SyncPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Sync auth not configured: set StarApi:SyncBearerToken or StarApi:SyncEmail/SyncPassword");
                return false;
            }

            try
            {
                var loginPayload = new { Email = email, Password = password };
                var res = await client.PostAsJsonAsync($"{baseUrl.TrimEnd('/')}/api/auth/login", loginPayload);
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sync login failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                    return false;
                }
                using var stream = await res.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var root = doc.RootElement;
                var accessToken = root.TryGetProperty("AccessToken", out var at) ? at.GetString() : null;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    Console.WriteLine("Sync login response missing AccessToken");
                    return false;
                }
                _cachedToken = accessToken;
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync login error: {ex.Message}");
                return false;
            }
        }
        
        public async Task PublishUserRegisteredAsync(UserRegisteredSyncDto dto)
        {
            var baseUrl = _configuration["StarApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return;
            using var client = new HttpClient();
            var token = _configuration["StarApi:SyncBearerToken"];
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            try
            {
                var res = await client.PostAsJsonAsync($"{baseUrl.TrimEnd('/')}/api/auth/register", dto);
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sync user register failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine("Sync user register succeeded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync user register error: {ex.Message}");
            }
        }

        public async Task PublishTicketCreatedAsync(TicketCreatedSyncDto dto)
        {
            var baseUrl = _configuration["StarApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return;
            using var client = new HttpClient();
            var integKey = _configuration["StarApi:IntegrationKey"];
            if (!string.IsNullOrWhiteSpace(integKey))
            {
                client.DefaultRequestHeaders.Add("X-Integration-Key", integKey);
            }
            else
            {
                await EnsureAuthAsync(client, baseUrl);
            }
            try
            {
                var path = !string.IsNullOrWhiteSpace(integKey) ? "/api/sync/ticket" : "/api/ticket";
                var res = await client.PostAsJsonAsync($"{baseUrl.TrimEnd('/')}{path}", dto);
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sync ticket create failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine("Sync ticket create succeeded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync ticket create error: {ex.Message}");
            }
        }

        public async Task PublishTicketUpdatedAsync(TicketUpdatedSyncDto dto)
        {
            var baseUrl = _configuration["StarApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return;
            using var client = new HttpClient();
            var integKey = _configuration["StarApi:IntegrationKey"];
            if (!string.IsNullOrWhiteSpace(integKey))
            {
                client.DefaultRequestHeaders.Add("X-Integration-Key", integKey);
            }
            else
            {
                await EnsureAuthAsync(client, baseUrl);
            }
            try
            {
                var path = !string.IsNullOrWhiteSpace(integKey) ? "/api/sync/ticket" : "/api/ticket";
                var res = await client.PutAsJsonAsync($"{baseUrl.TrimEnd('/')}{path}/{dto.Id}", dto);
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sync ticket update failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine("Sync ticket update succeeded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync ticket update error: {ex.Message}");
            }
        }

        public async Task PublishTicketDeletedAsync(TicketDeletedSyncDto dto)
        {
            var baseUrl = _configuration["StarApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return;
            using var client = new HttpClient();
            var integKey = _configuration["StarApi:IntegrationKey"];
            if (!string.IsNullOrWhiteSpace(integKey))
            {
                client.DefaultRequestHeaders.Add("X-Integration-Key", integKey);
            }
            else
            {
                await EnsureAuthAsync(client, baseUrl);
            }
            try
            {
                var path = !string.IsNullOrWhiteSpace(integKey) ? "/api/sync/ticket" : "/api/ticket";
                var res = await client.DeleteAsync($"{baseUrl.TrimEnd('/')}{path}/{dto.Id}");
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sync ticket delete failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine("Sync ticket delete succeeded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync ticket delete error: {ex.Message}");
            }
        }

        public async Task PublishTicketStatusChangedAsync(TicketStatusChangedSyncDto dto)
        {
            var baseUrl = _configuration["StarApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return;
            using var client = new HttpClient();
            var integKey = _configuration["StarApi:IntegrationKey"];
            if (!string.IsNullOrWhiteSpace(integKey))
            {
                client.DefaultRequestHeaders.Add("X-Integration-Key", integKey);
            }
            else
            {
                await EnsureAuthAsync(client, baseUrl);
            }
            var body = new { Status = dto.Status };
            try
            {
                var path = !string.IsNullOrWhiteSpace(integKey) ? "/api/sync/ticket" : "/api/ticket";
                var res = await client.PatchAsJsonAsync($"{baseUrl.TrimEnd('/')}{path}/{dto.Id}/move", body);
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sync ticket move failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine("Sync ticket move succeeded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync ticket move error: {ex.Message}");
            }
        }

        public async Task PublishUserAvatarUpdatedAsync(Guid userId, byte[] data, string mimeType)
        {
            var baseUrl = _configuration["StarApi:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return;
            var integKey = _configuration["StarApi:IntegrationKey"];
            if (string.IsNullOrWhiteSpace(integKey))
            {
                Console.WriteLine("IntegrationKey not configured; cannot sync avatar via integration endpoint");
                return;
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Integration-Key", integKey);
            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(data);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType);
                var fileName = $"avatar_{userId}";
                content.Add(fileContent, "file", fileName);
                var res = await client.PostAsync($"{baseUrl.TrimEnd('/')}/api/sync/users/{userId}/avatar", content);
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sync user avatar failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine("Sync user avatar succeeded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync user avatar error: {ex.Message}");
            }
        }
    }
}
