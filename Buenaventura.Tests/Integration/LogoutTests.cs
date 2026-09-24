using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buenaventura.Tests.Integration;

public class LogoutTests
{
    [Theory]
    [InlineData(true, HttpStatusCode.Redirect)]
    [InlineData(false, HttpStatusCode.BadRequest)]
    public async Task Logout_RequiresTokenFromServerRenderedForm(bool includeToken, HttpStatusCode expectedStatus)
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services => services
                    .AddAuthentication(options => options.DefaultAuthenticateScheme = "Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { }));
            });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var page = await client.GetAsync("/");
        page.EnsureSuccessStatusCode();
        var html = await page.Content.ReadAsStringAsync();
        var form = Regex.Match(html, "<form[^>]*id=\"logoutForm\"[^>]*>(.*?)</form>", RegexOptions.Singleline);
        form.Success.Should().BeTrue("the logout form must exist before WebAssembly starts");
        var token = Regex.Match(form.Value, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        token.Success.Should().BeTrue("the server must render a request verification token");

        var fields = new Dictionary<string, string> { ["ReturnUrl"] = "accounts" };
        if (includeToken)
        {
            fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value);
        }

        var response = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent(fields));

        response.StatusCode.Should().Be(expectedStatus);
        if (includeToken)
        {
            response.Headers.Location!.OriginalString.Should().Be("/accounts");
            response.Headers.GetValues("Set-Cookie").Should()
                .Contain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=;"));
        }
    }
}
