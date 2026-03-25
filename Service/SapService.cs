using TTSteelWebAPI.Data;
using TTSteelWebAPI.Interface;
using TTSteelWebAPI.Model.Login;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace TTSteelWebAPI.Service
{


   
        public class SapService
        {
            private readonly HttpClient _httpClient;
            private readonly IConfiguration _config;
            private string _sessionId;
            private readonly AppDbContext _appDbContext;
            private readonly ICurrentUserInterface _currentUserService;
            public SapService(HttpClient httpClient, IConfiguration config, AppDbContext appDbContext, ICurrentUserInterface currentUserService)
            {
                _httpClient = httpClient;
                _config = config;
                _appDbContext = appDbContext;
                _currentUserService = currentUserService;
            }

            public async Task<object?> LoginUserAsync(loginModel payload)
            {
                try
                {
                    // Call SAP Login
                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("Login", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorText = await response.Content.ReadAsStringAsync();
                        return new
                        {
                            Success = false,
                            StatusCode = (int)response.StatusCode,
                            Message = $"Login failed: {response.ReasonPhrase}",
                            Error = errorText
                        };
                    }

                    // Extract SAP login JSON
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);

                    _sessionId = data.GetProperty("SessionId").GetString();

                    // Store SAP session
                    _httpClient.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={_sessionId}");

                    // 🔥 FETCH USER FROM LOCAL DB (OUSR)
                    var user = await _appDbContext.OUSR
                        .Where(u => u.UserCode == payload.UserName)   // <-- use proper column
                        .Select(u => new
                        {
                            u.UserCode,
                            u.UserName,
                            u.UserId,

                        })
                        .FirstOrDefaultAsync();

                    // Return response
                    return new
                    {
                        Success = true,
                        SessionId = _sessionId,
                        Version = data.TryGetProperty("Version", out var version) ? version.GetString() : null,
                        SessionTimeout = data.TryGetProperty("SessionTimeout", out var timeout) ? timeout.GetInt32() : (int?)null,

                        // Local DB user info (null if not found)
                        UserInfo = user
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Message = "Exception occurred while logging in.",
                        Error = ex.Message
                    };
                }
            }


            // 🔹 Authenticate with SAP Service Layer
            public async Task<bool> LoginAsync()
            {
                //var payload = new
                //{
                //    CompanyDB = _config["SapSettings:CompanyDB"],
                //    UserName = _config["SapSettings:UserName"],
                //    Password = _config["SapSettings:Password"]
                //};

                var userContext = _currentUserService.GetUser();

                var payload = new
                {
                    CompanyDB = userContext.Database,
                    UserName = userContext.Username,
                    Password = userContext.Password
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    _sessionId = data.GetProperty("SessionId").GetString();

                    _httpClient.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={_sessionId}");
                    return true;
                }

                return false;
            }

            private async Task EnsureLoginAsync()
            {
                if (string.IsNullOrEmpty(_sessionId))
                {
                    var success = await LoginAsync();
                    if (!success)
                        throw new Exception("Failed to login to SAP Service Layer.");
                }
            }

            // 🔹 Generic GET Method
            public async Task<string> GetAsync(string endpoint)
            {
                await EnsureLoginAsync();

                var response = await _httpClient.GetAsync(endpoint);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"SAP GET Error ({response.StatusCode}): {result}");

                return result;
            }

            // 🔹 Generic POST Method
            public async Task<string?> PostAsync(string endpoint, object payload)
            {
                await EnsureLoginAsync();

                var jsonString = JsonSerializer.Serialize(payload);
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var doc = JsonDocument.Parse(result);
                    var errorMessage = doc.RootElement
                           .GetProperty("error")
                           .GetProperty("message")
                           .GetProperty("value")
                           .GetString();
                    throw new Exception($"SAP POST Error ({response.StatusCode}): {errorMessage}");
                }

                return result;
            }

            public async Task<string?> PostCancelAsync(string endpoint, string key)
            {
                await EnsureLoginAsync();
                var content = new StringContent(JsonSerializer.Serialize("[]"), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{endpoint}({key})/Cancel", null);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var doc = JsonDocument.Parse(result);
                    var errorMessage = doc.RootElement
                           .GetProperty("error")
                           .GetProperty("message")
                           .GetProperty("value")
                           .GetString();
                    throw new Exception($"SAP POST Error ({response.StatusCode}): {errorMessage}");
                }

                return result;
            }

            public async Task<string?> PostCloseAsync(string endpoint, string key)
            {
                await EnsureLoginAsync();
                var content = new StringContent(JsonSerializer.Serialize("[]"), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{endpoint}({key})/Close", null);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var doc = JsonDocument.Parse(result);
                    var errorMessage = doc.RootElement
                           .GetProperty("error")
                           .GetProperty("message")
                           .GetProperty("value")
                           .GetString();
                    throw new Exception($"SAP POST Error ({response.StatusCode}): {errorMessage}");
                }

                return result;
            }

            public async Task<string> PatchAsync(string endpoint, string key, object payload)
            {
                await EnsureLoginAsync();
                var jsonString = JsonSerializer.Serialize(payload);
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{endpoint}({key})", content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var doc = JsonDocument.Parse(result);
                    var errorMessage = doc.RootElement
                           .GetProperty("error")
                           .GetProperty("message")
                           .GetProperty("value")
                           .GetString();
                    throw new Exception($"SAP PATCH Error ({response.StatusCode}): {errorMessage}");
                }

                return result;
            }


            public async Task<string> Patch_DocumentAsync(string endpoint, string key, object payload)
            {
                await EnsureLoginAsync();

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{endpoint}({key})", content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var doc = JsonDocument.Parse(result);
                    var errorMessage = doc.RootElement
                           .GetProperty("error")
                           .GetProperty("message")
                           .GetProperty("value")
                           .GetString();
                    throw new Exception($"SAP PATCH Error ({response.StatusCode}): {errorMessage}");
                }

                return result;
            }
            public async Task DeleteAsync(string endpoint, string key)
            {
                await EnsureLoginAsync();

                // Example endpoint:
                // CVS_MMR1('CODE',1)
                var response = await _httpClient.DeleteAsync($"{endpoint}({key})");
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var doc = JsonDocument.Parse(result);
                        var errorMessage = doc.RootElement
                            .GetProperty("error")
                            .GetProperty("message")
                            .GetProperty("value")
                            .GetString();

                        throw new Exception($"SAP DELETE Error ({response.StatusCode}): {errorMessage}");
                    }
                    catch
                    {
                        throw new Exception($"SAP DELETE Error ({response.StatusCode}): {result}");
                    }
                }
            }

            // 🔹 Common Filter Method (for OData queries)
            public async Task<string> GetFilteredAsync(string endpoint, string filterQuery)
            {
                // Example: endpoint = "BusinessPartners", filterQuery = "$filter=CardType eq 'C'"
                var fullUrl = $"{endpoint}?{filterQuery}";
                return await GetAsync(fullUrl);
            }

            public async Task<string> GetSingleAsync(string endpoint, string key)
            {
                await EnsureLoginAsync();

                // SAP SL entity by key → CVS_OQCGRN(12)  OR CVS_OQCGRN('ABC')
                string url = $"{endpoint}({key})";

                var response = await _httpClient.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"SAP GET Error ({response.StatusCode}): {result}");

                return result;
            }
        }
 }


