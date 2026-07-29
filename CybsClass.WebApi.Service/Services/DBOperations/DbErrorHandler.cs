using System.Text.Json;
using CybsClass.Cybersource.Models.BaseData;

namespace CybsClass.WebApi.Service.Services.DBOperations;

/// <summary>
/// The single chokepoint for database error reporting. Every catch block in DBOperations
/// builds its ErrorObject and writes its log through here, so the shape of a database
/// failure is identical everywhere and callers never have to guess at a message format.
/// </summary>
public static class DbErrorHandler
{
    /// <summary>
    /// The one key under which every Persist* dictionary reports a failure. Consumers read it
    /// with TryGetValue; a dictionary containing this key contains nothing else worth reading.
    /// </summary>
    public const string ErrorKey = "Error";

    private static readonly JsonSerializerOptions LogOptions = new() { WriteIndented = true };

    private static readonly JsonSerializerOptions CompactOptions = new();

    public static ErrorObject BuildError(Exception ex, string operation)
    {
        Exception baseEx = ex.GetBaseException();

        return new ErrorObject
        {
            Error = "DATABASE_ERROR",
            Message = baseEx.Message,
            Reason = baseEx.GetType().Name,
            Action = $"Database operation '{operation}' failed. See server logs."
        };
    }

    public static ErrorObject BuildNotFound(string message) =>
        new() { Error = "NOT_FOUND", Message = message, Reason = "NotFound" };

    /// <summary>
    /// Compact-serialized ErrorObject, for the DTOs whose Error property is a string on the wire
    /// and cannot change type without breaking client deserialization.
    /// </summary>
    public static string BuildErrorString(Exception ex, string operation) =>
        ToErrorString(BuildError(ex, operation));

    /// <summary>Compact-serializes an already-built ErrorObject for the same string-typed DTOs.</summary>
    public static string ToErrorString(ErrorObject error) =>
        JsonSerializer.Serialize(error, CompactOptions);

    public static void Log(string operation, Exception ex)
    {
        Console.WriteLine($"\n[DB ERROR] {operation}:");
        Console.WriteLine(JsonSerializer.Serialize(BuildError(ex, operation), LogOptions));
        Console.WriteLine(ex.ToString());
    }

    /// <summary>Guards a database read/write whose result is carried by <see cref="DbResult{T}"/>.</summary>
    public static async Task<DbResult<T>> GuardAsync<T>(string operation, Func<Task<T>> action)
    {
        try
        {
            return DbResult<T>.Success(await action());
        }
        catch (Exception ex)
        {
            Log(operation, ex);
            return DbResult<T>.Fail(BuildError(ex, operation));
        }
    }

    /// <summary>Guards a database call that returns a DTO able to carry its own error.</summary>
    public static async Task<T> GuardDtoAsync<T>(string operation, Func<Task<T>> action, Func<ErrorObject, T> onError)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            Log(operation, ex);
            return onError(BuildError(ex, operation));
        }
    }

    /// <summary>Guards a Persist* call returning a string dictionary.</summary>
    public static async Task<Dictionary<string, string>> GuardDictAsync(
        string operation, Func<Task<Dictionary<string, string>>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            Log(operation, ex);
            return new Dictionary<string, string> { [ErrorKey] = ex.GetBaseException().Message };
        }
    }

    /// <summary>Guards a Persist* call returning an object dictionary.</summary>
    public static async Task<Dictionary<string, object>> GuardObjDictAsync(
        string operation, Func<Task<Dictionary<string, object>>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            Log(operation, ex);
            return new Dictionary<string, object> { [ErrorKey] = ex.GetBaseException().Message };
        }
    }
}
