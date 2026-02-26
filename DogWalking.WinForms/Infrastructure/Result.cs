namespace DogWalking.WinForms.Infrastructure;

/// <summary>
/// Explicit success/failure return type — eliminates scattered try/catch in the UI layer.
/// Services still throw; the UI layer wraps calls via RunAsync() which converts
/// exceptions into failure results, keeping all error-display logic in one place.
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool ok, string? error) { IsSuccess = ok; Error = error; }

    public static Result Ok() => new(true, null);
    public static Result Fail(string err) => new(false, err);
}

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool ok, T? value, string? error) { IsSuccess = ok; Value = value; Error = error; }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string err) => new(false, default, err);
}

/// <summary>Extension methods to wrap Task-returning calls in a Result.</summary>
public static class ResultExtensions
{
    public static async Task<Result> ToResultAsync(this Task task)
    {
        try { await task; return Result.Ok(); }
        catch (OperationCanceledException) { return Result.Ok(); }   // cancelled = silent
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T> task)
    {
        try { return Result<T>.Ok(await task); }
        catch (OperationCanceledException) { return Result<T>.Ok(default!); }
        catch (Exception ex) { return Result<T>.Fail(ex.Message); }
    }
}
