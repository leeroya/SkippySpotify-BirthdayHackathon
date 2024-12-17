using Microsoft.JSInterop;

namespace SkippySpotify;

public class TokenStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public TokenStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task SetAccessTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", token);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");
    }

    public async Task RemoveAccessTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
    }
}
