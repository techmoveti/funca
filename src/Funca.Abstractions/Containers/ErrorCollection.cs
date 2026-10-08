using System.Collections.Immutable;

namespace Funca.Abstractions.Containers;

public readonly record struct ErrorCollection
{
    public ErrorCollection(Error error) => Errors = ImmutableArray.Create(error);

    public ErrorCollection(ImmutableArray<Error> errors) => Errors = errors;

    public ErrorCollection(IEnumerable<Error> Errors)
    {
        ArgumentNullException.ThrowIfNull(Errors);
        this.Errors = [.. Errors];
    }

    public ImmutableArray<Error> Errors => field.IsDefault ? [] : field;

    public bool Equals(ErrorCollection other)
    {
        var left = Errors;
        var right = other.Errors;

        return left == right || left.AsSpan().SequenceEqual(right.AsSpan());
    }

    public ErrorCollection Combine(ErrorCollection other)
    {
        var left = Errors;
        var right = other.Errors;

        if (left.IsEmpty)
            return other;
        if (right.IsEmpty)
            return this;

        return new ErrorCollection(left.AddRange(right));
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var error in Errors)
            hash.Add(error);

        return hash.ToHashCode();
    }

    public static implicit operator ErrorCollection(Error error)
        => new(error);

    public static implicit operator ErrorCollection(Error[] errors)
        => new(errors);

    public static implicit operator ErrorCollection(List<Error> errors)
        => new(errors);
}