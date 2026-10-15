using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PasswordApi.Tests;

public class ProblemDetailsTests(PasswordApiFactory factory) : IClassFixture<PasswordApiFactory>
{
    [Fact]
    public async Task UnknownRoute_Returns404WithProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/unknown-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }
}
