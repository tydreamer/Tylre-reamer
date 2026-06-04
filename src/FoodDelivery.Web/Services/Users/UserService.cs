using System.Net.Http.Json;
using FoodDelivery.Web.Helpers;

namespace FoodDelivery.Web.Services.Users;

public class UserService(HttpClient http)
{
    public async Task<PagedResult<UserDto>?> GetPageAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDesc = false)
    {
        var url = $"api/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(sortBy))
            url += $"&sortBy={Uri.EscapeDataString(sortBy.Trim())}&sortDesc={sortDesc.ToString().ToLowerInvariant()}";

        return await http.GetBrowseJsonAsync<PagedResult<UserDto>>(url);
    }

    public Task<Result<UserDto>> CreateAsync(CreateUserRequest request) =>
        http.PostForResultAsync<UserDto>("api/users", request);

    public Task<Result> BlockAsync(int userId) =>
        http.PutForResultAsync($"api/users/{userId}/block");

    public Task<Result> UnblockAsync(int userId) =>
        http.PutForResultAsync($"api/users/{userId}/unblock");
}
