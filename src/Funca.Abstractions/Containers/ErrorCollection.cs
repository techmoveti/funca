using System.Collections.Immutable;

namespace Funca.Abstractions.Containers;

public readonly record struct ErrorCollection
{
    private readonly ImmutableArray<Error> _errors;

    public ErrorCollection(IEnumerable<Error> Errors)
    {
        ArgumentNullException.ThrowIfNull(Errors);
        _errors = Errors.ToImmutableArray();
    }

    public ImmutableArray<Error> Errors => _errors.IsDefault ? [] : _errors;

    public static implicit operator ErrorCollection(Error error)
        => new([error]);

    public static implicit operator ErrorCollection(Error[] errors)
        => new(errors);

    public static implicit operator ErrorCollection(List<Error> errors)
        => new(errors);
}