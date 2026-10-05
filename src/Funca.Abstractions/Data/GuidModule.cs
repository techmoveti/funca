namespace Funca.Abstractions.Data;

public static class GuidModule
{
    /// <summary>
    ///     Sequential Guid - better database indexing.
    /// </summary>
    /// <returns></returns>
    public static Guid Sequential()
        => Guid.CreateVersion7();
}