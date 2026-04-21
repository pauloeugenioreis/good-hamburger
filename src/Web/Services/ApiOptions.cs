namespace GoodHamburger.Web.Services;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; set; } = "https://localhost:3060/";

    public int RequestTimeoutSeconds { get; set; } = 30;
}
