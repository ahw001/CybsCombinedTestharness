using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.WebApi.Service.Services.DBOperations;

/// <summary>
/// Server-internal carrier for a database operation's outcome. Never serialized to the wire —
/// endpoints unwrap it (see <see cref="DbResultExtensions"/>) into either the value or a
/// serialized <see cref="ErrorObject"/>, both as HTTP 2XX per the project error convention.
/// </summary>
public sealed class DbResult<T>
{
    public T? Value { get; private init; }

    public ErrorObject? Error { get; private init; }

    public bool Ok => Error is null;

    public static DbResult<T> Success(T? value) => new() { Value = value };

    public static DbResult<T> Fail(ErrorObject error) => new() { Error = error };
}
