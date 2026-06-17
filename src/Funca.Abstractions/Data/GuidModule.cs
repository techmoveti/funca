namespace Funca.Abstractions.Data;

public static class GuidModule
{
    /// <summary>
    ///     Sequential Guid - better database indexing.
    /// </summary>
    /// <returns></returns>
    public static Guid Sequential()
        => Guid.CreateVersion7();

    /// <summary>
    ///     String to Guid.
    /// </summary>
    /// <param name="guidString"></param>
    /// <returns></returns>
    public static Result<Guid> ToGuid(this string guidString)
        => Guid.TryParse(guidString, out var guid)
            ? Result.Ok(guid)
            : ErrorResult.Validation($"'{guidString}' is not a valid GUID.");

    /// <summary>
    ///     String to Guid.
    /// </summary>
    /// <param name="guidString"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static Result<Guid> ToGuid(this string guidString, string errorMessage)
        => Guid.TryParse(guidString, out var guid)
            ? Result.Ok(guid)
            : ErrorResult.Validation(errorMessage);
}