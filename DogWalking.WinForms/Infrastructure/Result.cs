namespace DogWalking.WinForms.Infrastructure;

public sealed class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool ok, string? error) { IsSuccess = ok; Error = error; }

    public static Result Ok() => new(true, null);
    public static Result Fail(string err) => new(false, err);
}

public static class ResultExtensions
{
    public static async Task<Result> ToResultAsync(this Task task)
    {
        try { await task; return Result.Ok(); }
        catch (OperationCanceledException) { return Result.Ok(); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }
}
