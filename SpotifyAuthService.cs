using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace SkippySpotify;

public class SpotifyAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly NavigationManager _navigationManager;
    private readonly TokenStorageService _tokenStorageService;
    
    
    public SpotifyAuthService(IHttpClientFactory factory, IConfiguration configuration, NavigationManager manager, TokenStorageService tokenStorageService)
    {
        _httpClient = factory.CreateClient();
        _httpClientFactory = factory;
        _configuration = configuration;
        _navigationManager = manager;
        _tokenStorageService = tokenStorageService;
    }

    public void RedirectToSpotifyLogin()
    {
        var clientId = _configuration["Spotify:ClientId"];
        var redirectUri = _navigationManager.BaseUri + "spotify-callback";
        var scope = "user-read-private user-read-email";
        var url = $"https://accounts.spotify.com/authorize?response_type=code&client_id={clientId}&scope={scope}&redirect_uri={redirectUri}";
        _navigationManager.NavigateTo(url);
    }
    
    public async Task<string> GetAccessTokenAsync(string code)
    {
        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];
        var redirectUri = _navigationManager.BaseUri + "spotify-callback";

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<SpotifyTokenResponse>(content);

        
//        await _tokenStorageService.SetAccessTokenAsync(tokenResponse.AccessToken);

        return tokenResponse.AccessToken;
    }


    public async Task<Profile> GetProfile(string token)
    {
        var url = "https://api.spotify.com/v1/me";
        var client = _httpClientFactory.CreateClient("spotify");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var request = await client.GetAsync(url);
        var response = await request.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<Profile>(response);
        return profile;
    }
}

public class Profile
{
    public string country { get; set; }
    public string display_name { get; set; }
    public string email { get; set; }
}

public class SpotifyTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
}
