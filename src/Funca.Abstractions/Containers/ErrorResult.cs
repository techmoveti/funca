namespace Funca.Abstractions.Containers;

public record ErrorResult(string? Key, ErrorType Type, string Message)
{
    public static readonly ErrorResult Empty = new(string.Empty, ErrorType.Failure, string.Empty);

    public bool IsEmpty() => Equals(Empty);

    public static ErrorResult Create(string message) => new(null, ErrorType.Failure, message);

    public static ErrorResult Create(ErrorType type, string message) => new(null, type, message);

    public static ErrorResult Create(string key, ErrorType type, string message) => new(key, type, message);

    public static ErrorResult Failure(string message = "failure") => Create(ErrorType.Failure, message);

    public static ErrorResult Validation(string message) => Create(ErrorType.Validation, message);

    public static ErrorResult Validation(string key, string message) => Create(key, ErrorType.Validation, message);

    public static ErrorResult NotFound(string message = "not found") => Create(ErrorType.NotFound, message);

    public static ErrorResult Unauthorized(string message = "unauthorized") => Create(ErrorType.Unauthorized, message);

    public static ErrorResult Forbidden(string message = "forbidden") => Create(ErrorType.Forbidden, message);
}