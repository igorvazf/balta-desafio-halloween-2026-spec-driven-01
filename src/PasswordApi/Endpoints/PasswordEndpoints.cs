using Microsoft.AspNetCore.Http.HttpResults;
using PasswordApi.Contracts;
using PasswordApi.Services;

namespace PasswordApi.Endpoints;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/passwords");

        group.MapPost("", CreatePasswordAsync);

        return app;
    }

    private static async Task<Created<PasswordResponse>> CreatePasswordAsync(
        IPasswordService passwordService,
        CancellationToken cancellationToken)
    {
        var response = await passwordService.CreateAsync(cancellationToken);

        return TypedResults.Created($"/passwords/{response.Id}", response);
    }
}
