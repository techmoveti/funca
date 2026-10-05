using System.Collections.Immutable;

namespace Funca.Abstractions.Containers;

public sealed class ResultBuilder
{
    private readonly List<Error> _errors = [];
    private readonly Dictionary<object, object?> _validObjects = [];

    public bool IsValid
        => _errors.Count == 0;

    public static ResultBuilder Combine() => new();

    public ResultBuilder Ensure<T>(string alias, T value, Func<T, bool> validator, string message)
        => Ensure(alias, value, validator, Error.Invalid(alias, message));

    public ResultBuilder Ensure<T>(string alias, T value, Func<T, bool> validator, Error error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        return Ensure((object)alias, value, validator, error);
    }

    public ResultBuilder Ensure<T>(T value, Func<T, bool> validator, string message)
        => Ensure(typeof(T), value, validator, Error.Invalid(typeof(T).Name, message));

    public ResultBuilder Ensure<T>(T value, Func<T, bool> validator, Error error)
        => Ensure(typeof(T), value, validator, error);

    public async ValueTask<ResultBuilder> EnsureAsync<T>(
        string alias,
        T value,
        Func<T, CancellationToken, ValueTask<bool>> validator,
        Error error,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentNullException.ThrowIfNull(validator);
        cancellationToken.ThrowIfCancellationRequested();

        var isValid = await validator(value, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return Register(alias, value, isValid, error);
    }

    public T Get<T>()
        => GetValidatedValue<T>(typeof(T));

    public T Get<T>(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        return GetValidatedValue<T>(alias);
    }

    public Result<T> Build<T>(Func<ResultBuilder, T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (IsValid)
            return Result<T>.Ok(factory(this));

        return Result<T>.Fail(new ErrorCollection(_errors));
    }

    public ImmutableArray<Error> GetErrors()
        => [.. _errors];

    private ResultBuilder Ensure<T>(object key, T value, Func<T, bool> validator, Error error)
    {
        ArgumentNullException.ThrowIfNull(validator);

        return Register(key, value, validator(value), error);
    }

    private ResultBuilder Register<T>(object key, T value, bool isValid, Error error)
    {
        if (isValid)
            _validObjects[key] = value;
        else
        {
            _validObjects.Remove(key);
            _errors.Add(error);
        }

        return this;
    }

    private T GetValidatedValue<T>(object key)
    {
        if (!_validObjects.TryGetValue(key, out var value))
            throw new InvalidOperationException(
                $"O objeto com alias ou tipo '{key}' não foi validado com sucesso ou não foi registrado no Builder.");

        if (value is T typedValue)
            return typedValue;

        if (value is null && default(T) is null)
            return default!;

        throw new InvalidOperationException(
            $"O objeto com alias ou tipo '{key}' possui tipo '{value?.GetType().ToString() ?? "null"}', incompatível com '{typeof(T)}'.");
    }
}