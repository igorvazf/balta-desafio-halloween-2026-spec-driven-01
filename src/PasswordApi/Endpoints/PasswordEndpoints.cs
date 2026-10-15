using Microsoft.AspNetCore.Http.HttpResults;
using PasswordApi.Contracts;
using PasswordApi.Services;

namespace PasswordApi.Endpoints;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/passwords").WithTags("Passwords");

        group.MapPost("", CreatePasswordAsync)
            .WithName("CreatePassword")
            .WithSummary("Generates and stores a new strong password")
            .Produces<PasswordResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", GetPasswordAsync)
            .WithName("GetPasswordById")
            .WithSummary("Returns the password associated with the given id")
            .Produces<PasswordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

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
