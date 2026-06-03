namespace FoodDelivery.Web.Helpers;

public static class OwnerOrderActionDefinitions
{
    public static IEnumerable<(string Status, string Label)> GetStatusActions(string status) =>
        status switch
        {
            "Placed" =>
            [
                ("Processing", "Start processing"),
                ("Cancelled", "Cancel order")
            ],
            "Processing" => [("InRoute", "Mark in route")],
            "InRoute" => [("Delivered", "Mark delivered")],
            _ => []
        };
}
