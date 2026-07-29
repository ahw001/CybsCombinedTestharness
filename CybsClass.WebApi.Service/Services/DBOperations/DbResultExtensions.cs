using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.WebApi.Service.Services.DBOperations;

/// <summary>
/// Endpoint-side unwrapping for <see cref="DbResult{T}"/>. Both the success and the failure
/// path return HTTP 200 — application-level errors travel as a serialized ErrorObject in the
/// body, never as a 4XX/5XX status.
/// </summary>
public static class DbResultExtensions
{
    public static IResult ToOkOrError<T>(this DbResult<T> result) =>
        result.Ok ? Results.Ok(result.Value) : Results.Json(result.Error);

    /// <summary>
    /// For get-by-id style reads: distinguishes a database failure from a genuinely missing row.
    /// A missing row is reported as an ErrorObject with Error = "NOT_FOUND", still HTTP 200.
    /// </summary>
    public static IResult ToOkOrNotFound<T>(this DbResult<T> result, string notFoundMessage) =>
        !result.Ok ? Results.Json(result.Error)
        : result.Value is null ? Results.Json(DbErrorHandler.BuildNotFound(notFoundMessage))
        : Results.Ok(result.Value);

    /// <summary>
    /// For affected-row-count writes (update/delete): zero rows means the target did not exist.
    /// </summary>
    public static IResult ToOkOrNotFound(this DbResult<int> result, string notFoundMessage) =>
        !result.Ok ? Results.Json(result.Error)
        : result.Value <= 0 ? Results.Json(DbErrorHandler.BuildNotFound(notFoundMessage))
        : Results.Ok(result.Value);
}
