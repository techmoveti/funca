namespace Funca.Abstractions.Containers;

public static class ResultModule
{
    public static Success<T> Ok<T>(T value)
        => new(value);

    public static ErrorCollection Fail(string errorMessage)
        => new(Error.Invalid(errorMessage));

    public static ErrorCollection Fail(Error error)
        => new(error);

    public static ErrorCollection Fail(Error[] error)
        => new(error);

    public static Result<T> Of<T>(T value)
        => new(new Success<T>(value));
}