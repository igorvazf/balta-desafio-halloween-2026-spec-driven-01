using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PasswordApi.Contracts;
using PasswordApi.Services;

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

    [Fact]
    public async Task Get_WithIdReturnedByPost_Returns200WithSamePassword()
    {
        var client = factory.CreateClient();
        var created = await CreatePasswordAsync(client);

        var response = await client.GetAsync($"/passwords/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PasswordResponse>();
        Assert.Equal(created, body);
    }

    [Fact]
    public async Task Get_WithUnknownId_Returns404WithProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/passwords/{Guid.NewGuid()}");

        await AssertNotFoundProblemAsync(response);
    }

    [Fact]
    public async Task Get_WithEmptyId_Returns404WithProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/passwords/{Guid.Empty}");

        await AssertNotFoundProblemAsync(response);
    }

    [Fact]
    public async Task Get_WithMalformedId_Returns404WithProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/passwords/abc");

        await AssertNotFoundProblemAsync(response);
    }

    [Fact]
    public async Task Post_WhenSaveFails_Returns500WithProblemDetailsWithoutIdOrPassword()
    {
        await using var failingFactory = new PasswordApiFactory { FailOnSave = true };
        var client = failingFactory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddSingleton<IPasswordGenerator, FixedPasswordGenerator>()))
            .CreateClient();

        var response = await client.PostAsync("/passwords", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(response.Headers.Location);

        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(FixedPasswordGenerator.Password, content);
        Assert.DoesNotMatch(GuidPattern(), content);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
    }

    private static async Task<PasswordResponse> CreatePasswordAsync(HttpClient client)
    {
        var response = await client.PostAsync("/passwords", null);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PasswordResponse>())!;
    }

    private static async Task AssertNotFoundProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
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

    private static Regex GuidPattern() =>
        new("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

    private sealed class FixedPasswordGenerator : IPasswordGenerator
    {
        public const string Password = "Fixed#Password123";

        public string Generate() => Password;
    }
}
