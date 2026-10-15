using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace PasswordApi.Tests;

public class SwaggerTests(PasswordApiFactory factory) : IClassFixture<PasswordApiFactory>
{
    [Fact]
    public async Task SwaggerUi_InDevelopment_IsAvailable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerDocument_InDevelopment_DocumentsSuccessAndErrorResponses()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        Assert.Equal(2, paths.EnumerateObject().Count());

        var post = paths.GetProperty("/passwords").GetProperty("post");
        Assert.Equal("CreatePassword", post.GetProperty("operationId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(post.GetProperty("summary").GetString()));
        AssertResponses(post, "201", "500");

        var get = paths.GetProperty("/passwords/{id}").GetProperty("get");
        Assert.Equal("GetPasswordById", get.GetProperty("operationId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(get.GetProperty("summary").GetString()));
        AssertResponses(get, "200", "404", "500");
    }

    [Fact]
    public async Task Swagger_OutsideDevelopment_IsNotAvailable()
    {
        var client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Production"))
            .CreateClient();

        var document = await client.GetAsync("/swagger/v1/swagger.json");
        var ui = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.NotFound, document.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ui.StatusCode);
    }

    private static void AssertResponses(JsonElement operation, params string[] statusCodes)
    {
        var responses = operation.GetProperty("responses");

        foreach (var statusCode in statusCodes)
        {
            Assert.True(responses.TryGetProperty(statusCode, out _), $"Missing response {statusCode}");
        }

        foreach (var errorCode in statusCodes.Where(code => code.StartsWith('4') || code.StartsWith('5')))
        {
            var content = responses.GetProperty(errorCode).GetProperty("content");
            Assert.True(content.TryGetProperty("application/problem+json", out _), $"Response {errorCode} is not ProblemDetails");
        }
    }
}
