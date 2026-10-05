namespace Funca.Abstractions.Containers;

public readonly record struct ErrorResults(ErrorResult[] Errors)
{
    public static implicit operator ErrorResults(ErrorResult error)
        => new([error]);

    public static implicit operator ErrorResults(ErrorResult[] errors)
        => new(errors);

    public static implicit operator ErrorResults(List<ErrorResult> errors)
        => new([.. errors]);
}