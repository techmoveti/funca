namespace Funca.Abstractions.Containers;

public static class ResultModule
{
    public static Success<T> Ok<T>(T value)
        => new(value);

    public static ErrorCollection Fail(Error error)
        => new([error]);
}