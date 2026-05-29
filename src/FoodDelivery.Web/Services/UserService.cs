namespace FoodDelivery.Web.Services;

public class UserService(HttpClient http)
{
    public async Task<(bool Success, string? Error)> BlockCustomerAsync(int customerId)
    {
        var response = await http.PutAsync($"api/users/{customerId}/block", null);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await response.Content.ReadAsStringAsync());
    }
}
