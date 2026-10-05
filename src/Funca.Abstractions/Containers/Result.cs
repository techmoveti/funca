namespace Funca.Abstractions.Containers;

public union Result<T>(Success<T>, ErrorCollection) : IOutcome<T>
{
    public static Result<T> Ok(T value) => new Success<T>(value);

    public static Result<T> Fail(Error error) => new ErrorCollection([error]);

    public static Result<T> Fail(ErrorCollection errors) => errors;
}