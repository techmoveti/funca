namespace Funca.Abstractions.Containers;

public readonly record struct Error(string? Key, ErrorType Type, string Message)
{
    public string Message
    {
        get => field ?? string.Empty;
        init => field = value;
    } = Message;

    public static readonly Error Empty = default;

    public bool IsEmpty()
        => Type == ErrorType.Failure && string.IsNullOrEmpty(Key) && Message.Length == 0;

    public bool Equals(Error other)
        => Key == other.Key && Type == other.Type && Message == other.Message;

    public override int GetHashCode()
        => HashCode.Combine(Key, Type, Message);

    public static Error Create(string message) => new(null, ErrorType.Failure, message);
    public static Error Create(ErrorType type, string message) => new(null, type, message);
    public static Error Create(string key, ErrorType type, string message) => new(key, type, message);

    public static Error Failure(string message = "failure") => Create(ErrorType.Failure, message);
    public static Error Invalid(string message) => Create(ErrorType.Invalid, message);
    public static Error Invalid(string key, string message) => Create(key, ErrorType.Invalid, message);
    public static Error NotFound(string message = "not found") => Create(ErrorType.NotFound, message);
    public static Error Unauthorized(string message = "unauthorized") => Create(ErrorType.Unauthorized, message);
    public static Error Forbidden(string message = "forbidden") => Create(ErrorType.Forbidden, message);
}
