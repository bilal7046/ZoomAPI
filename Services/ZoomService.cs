using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using ZoomAPI.Controllers;
using ZoomAPI.Data;
using ZoomAPI.Interfaces;
using ZoomAPI.Models;

namespace ZoomAPI.Services
{
    public class ZoomService : IZoom
    {
        private readonly ILogger<ZoomService> _logger;
        private readonly Zoom _zoomConfig;
        private readonly ApplicationDbContext _context;
        public ZoomService(ILogger<ZoomService> logger, IOptions<Zoom> zoomConfig, ApplicationDbContext context)
        {
            _logger = logger;
            _zoomConfig = zoomConfig.Value;
            _context = context;
        }
       
        public async Task<bool> Authenticate(string code)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://zoom.us/oauth/");
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                    var requestContent = new FormUrlEncodedContent(new[]
                    {
                                                                    new KeyValuePair<string, string>("code", code),
                                                                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                                                                    new KeyValuePair<string, string>("redirect_uri", _zoomConfig.redirect_uri),
                                                                    new KeyValuePair<string, string>("client_id", _zoomConfig.app_id),
                                                                    new KeyValuePair<string, string>("client_secret", _zoomConfig.app_Secret)
                    });

                    HttpResponseMessage response = await httpClient.PostAsync("token", requestContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseContent);
                        string refreshToken = result.refresh_token;
                        string accessToken = result.access_token;

                        var token = await RefreshTokenAsync(refreshToken);
                        await SaveRefreshToken(refreshToken);

                        //var meetings = await GetMeetings(token);
                        return true;
                    }
                    else
                    {
                        // Handle unsuccessful response
                        return false;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<(bool status,string error,string joinUrl)> CreateMeeting(MeetingRequest meetingRequest)
        {
            var token = await GetRefreshToken();
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.zoom.us/v2/users/me/meetings");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var json = JsonConvert.SerializeObject(meetingRequest);
            var content = new StringContent(json, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var rawJson = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());


                return (true, "",responseJson.start_url);
            }
            else
            {
                var responseJson = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                return (false, responseJson.message,"");
            }
        }
        public async Task<bool> DeleteMeeting(string meetingId)
        {
            var token = await GetRefreshToken();
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.zoom.us/v2/meetings/{meetingId}");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DisconnectZoom()
        {
            var configuration = await _context.Configuration.FirstOrDefaultAsync(c => c.Name.ToLower() == "zoom");
            _context.Configuration.Remove(configuration);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Root> GetMeetings()
        {
            try
            {
                var token = await GetRefreshToken();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = await client.GetAsync("https://api.zoom.us/v2/users/me/meetings");

                    if (response.IsSuccessStatusCode)
                    {
                        var meetings = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<Root>(meetings);

                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve meetings: {response.ReasonPhrase}");
                        return new Root();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> GetRefreshToken()
        {
            string token = string.Empty;

            var configuration = await _context.Configuration.FirstOrDefaultAsync(c => c.Name.ToLower() == "zoom");

            if (configuration != null)
            {
                token = await RefreshTokenAsync(configuration.RefreshToken);
            }
            return token;
        }

        public async Task<bool> IsConnected()
        {
            var configuration=await _context.Configuration.FirstOrDefaultAsync(c => c.Name.ToLower() == "zoom");
            if (configuration!=null)
            {
                return true;
            }
            return false;
        }

        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                string token = "";

                using (var httpClient = new HttpClient())
                {
                    var credentials = $"{_zoomConfig.app_id}:{_zoomConfig.app_Secret}";
                    var plainTextBytes = Encoding.UTF8.GetBytes(credentials);
                    string val = Convert.ToBase64String(plainTextBytes);

                    var postData = new List<KeyValuePair<string, string>>
                                                    {
                                                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                                                        new KeyValuePair<string, string>("refresh_token", refreshToken),
                                                        new KeyValuePair<string, string>("client_id", _zoomConfig.app_id),
                                                        new KeyValuePair<string, string>("client_secret", _zoomConfig.app_Secret)
                                                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, "https://zoom.us/oauth/token")
                    {
                        Content = new FormUrlEncodedContent(postData)
                    };

                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", val);

                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        token = result.access_token; //the name of the access token in your response can be different, change it to whatever suits your needs
                    }
                }

                return token;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> SaveRefreshToken(string refreshToken)
        {
            try
            {
                var config = await _context.Configuration.SingleOrDefaultAsync();
                if (config == null)
                {
                    Configuration configuration = new Configuration()
                    {
                        Name = "Zoom",
                        RefreshToken = refreshToken
                    };
                    _context.Configuration.Add(configuration);

                }
                else
                {
                    config.RefreshToken = refreshToken;

                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> UpdateMeeting(string meetingId,string note)
        {
            var token = await GetRefreshToken();

            MeetingRequest Req = new MeetingRequest(){Agenda = note};

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Patch, $"https://api.zoom.us/v2/meetings/{meetingId}");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var json = JsonConvert.SerializeObject(Req);
            var content = new StringContent(json, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
              
            }
            return false;
        }
    }
}
