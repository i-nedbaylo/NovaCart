namespace NovaCart.Services.Identity.Domain;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> All = [Admin, Customer];
}
