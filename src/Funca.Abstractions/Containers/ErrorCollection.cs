using System.Collections.Immutable;

namespace Funca.Abstractions.Containers;

public readonly record struct ErrorCollection
{
    public ErrorCollection(IEnumerable<Error> Errors)
    {
        ArgumentNullException.ThrowIfNull(Errors);
        this.Errors = [.. Errors];
    }

    public ImmutableArray<Error> Errors => field.IsDefault ? [] : field;

    public static implicit operator ErrorCollection(Error error)
        => new([error]);

    public static implicit operator ErrorCollection(Error[] errors)
        => new(errors);

    public static implicit operator ErrorCollection(List<Error> errors)
        => new(errors);
}