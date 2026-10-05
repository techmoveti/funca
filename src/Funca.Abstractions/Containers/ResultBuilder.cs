using System.Collections.Immutable;

namespace Funca.Abstractions.Containers;

public union Result<T>(T, ErrorResults);

public sealed class ResultBuilder
{
    private readonly List<ErrorResult> _errors = [];
    private readonly Dictionary<string, object?> _validObjects = [];

    public bool IsValid 
        => _errors.Count == 0;

    public static ResultBuilder Combine() => new();

    public ResultBuilder Ensure<T>(string alias, T value, Func<T, bool> validator, string message)
        => Ensure(alias, value, validator, ErrorResult.Invalid(alias, message));

    public ResultBuilder Ensure<T>(string alias, T value, Func<T, bool> validator, ErrorResult error)
    {
        if (validator(value))
            _validObjects[alias] = value; // Guarda pelo apelido fornecido
        else
            _errors.Add(error);

        return this;
    }

    public ResultBuilder Ensure<T>(T value, Func<T, bool> validator, string message)
        => Ensure(typeof(T).Name, value, validator, ErrorResult.Invalid(typeof(T).Name, message));

    public ResultBuilder Ensure<T>(T value, Func<T, bool> validator, ErrorResult error)
        => Ensure(typeof(T).Name, value, validator, error);

    public async ValueTask<ResultBuilder> EnsureAsync<T>(
        string alias,
        T value,
        Func<T, CancellationToken, ValueTask<bool>> validator,
        ErrorResult error,
        CancellationToken cancellationToken = default)
    {
        if (await validator(value, cancellationToken))
            _validObjects[alias] = value;
        else
            _errors.Add(error);

        return this;
    }

    public T Get<T>() 
        => Get<T>(typeof(T).Name);

    public T Get<T>(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        if (_validObjects.TryGetValue(alias, out var value))
            return (T)value!;

        throw new InvalidOperationException(
            $"O objeto com alias ou tipo '{alias}' não foi validado com sucesso ou não foi registrado no Builder.");
    }

    public Result<T> Build<T>(Func<ResultBuilder, T> factory)
    {
        if (IsValid)
            return factory(this);

        return new ErrorResults([.. _errors]);
    }

    public ImmutableArray<ErrorResult> GetErrors() 
        => [.. _errors];
}