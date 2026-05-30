using System.Net.Http.Json;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class UserService(HttpClient http)
{
    public async Task<List<UserDto>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<UserDto>>("api/users") ?? [];
    }

    public async Task<(bool Success, UserDto? User, string? Error)> CreateAsync(CreateUserRequest request)
    {
        var response = await http.PostAsJsonAsync("api/users", request);
        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return (true, user, null);
        }

        return (false, null, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> BlockAsync(int userId)
    {
        var response = await http.PutAsync($"api/users/{userId}/block", null);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }
}
