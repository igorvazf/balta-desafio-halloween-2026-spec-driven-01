using System.Net;
using System.Net.Http.Json;
using PasswordApi.Contracts;

namespace PasswordApi.Tests;

public class PasswordEndpointsTests(PasswordApiFactory factory) : IClassFixture<PasswordApiFactory>
{
    private const string AllowedSpecials = "!@#$%^&*()-_=+[]{}<>?";

    [Fact]
    public async Task Post_Returns201WithLocationIdAndValidPassword()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/passwords", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PasswordResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal($"/passwords/{body.Id}", response.Headers.Location?.OriginalString);
        AssertValidPassword(body.Password);
    }

    private static void AssertValidPassword(string password)
    {
        Assert.InRange(password.Length, 16, 128);
        Assert.Contains(password, char.IsAsciiLetterUpper);
        Assert.Contains(password, char.IsAsciiLetterLower);
        Assert.Contains(password, char.IsAsciiDigit);
        Assert.Contains(password, c => AllowedSpecials.Contains(c));
        Assert.DoesNotContain(password, char.IsWhiteSpace);
    }
}
