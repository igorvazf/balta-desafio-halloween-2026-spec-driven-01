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
        group.MapGet("/{id:guid}", GetPasswordAsync);

        return app;
    }

    private static async Task<Created<PasswordResponse>> CreatePasswordAsync(
        IPasswordService passwordService,
        CancellationToken cancellationToken)
    {
        var response = await passwordService.CreateAsync(cancellationToken);

        return TypedResults.Created($"/passwords/{response.Id}", response);
    }

    private static async Task<Results<Ok<PasswordResponse>, ProblemHttpResult>> GetPasswordAsync(
        Guid id,
        IPasswordService passwordService,
        CancellationToken cancellationToken)
    {
        var response = await passwordService.GetByIdAsync(id, cancellationToken);

        if (response is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Password not found",
                detail: $"No password was found for id '{id}'.");
        }

        return TypedResults.Ok(response);
    }
}
