using System.Collections.Immutable;

namespace Funca.Abstractions.Containers;

public sealed class ResultBuilder
{
    private readonly Lazy<List<ErrorResult>> _errors = new(() => []);
    private readonly Lazy<Dictionary<string, object?>> _validObjects = new(() => new Dictionary<string, object?>());

    /// <summary>
    ///     Builder is valid ?
    /// </summary>
    public bool IsValid { get; private set; } = true;

    /// <summary>
    ///     Add Result object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public ResultBuilder Add<T>(Result<T> result)
    {
        result.Match(
            arg => _validObjects.Value.Add(typeof(T).Name, arg),
            errors =>
            {
                IsValid = false;
                _errors.Value.AddRange(errors);
            });

        return this;
    }

    /// <summary>
    ///     Add Result object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="alias"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public ResultBuilder Add<T>(string alias, Result<T> result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        result.Match(
            arg => _validObjects.Value.Add(alias, arg),
            errors =>
            {
                IsValid = false;
                _errors.Value.AddRange(errors);
            });

        return this;
    }

    /// <summary>
    ///     Add Result objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="alias"></param>
    /// <param name="results"></param>
    /// <returns></returns>
    public ResultBuilder Add<T>(string alias, IEnumerable<Result<T>> results)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentNullException.ThrowIfNull(results);

        var materializedResults = results.ToArray();
        var fails = materializedResults.Where(p => p.IsError).ToArray();
        if (fails.Length != 0)
        {
            IsValid = false;
            _errors.Value.AddRange(fails.SelectMany(f => f.ErrorsToArray()));

            return this;
        }

        _validObjects.Value.Add(alias, materializedResults.Select(r => r.Unwrap()).ToArray());

        return this;
    }

    /// <summary>
    ///     Get object validated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T GetObject<T>()
    {
        var typeName = typeof(T).Name;

        return GetObject<T>(typeName);
    }

    /// <summary>
    ///     Get object validated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T GetObject<T>(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        if (_validObjects.Value.TryGetValue(alias, out var value)) return (T)value!;

        throw new InvalidOperationException($"Object {alias} is not valid.");
    }

    /// <summary>
    ///     Try build valid type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    /// <returns></returns>
    public Result<T> Build<T>(Func<ResultBuilder, T> factory)
    {
        if (!IsValid) return Result<T>.Error([.. _errors.Value]);

        try
        {
            var factoryResult = factory(this);

            return Result<T>.Wrap(factoryResult);
        }
        catch (Exception e)
        {
            return ErrorResult.Create(e.Message);
        }
    }


    public Result<T> Build<T>(Func<ResultBuilder, Result<T>> factory)
    {
        if (!IsValid) return Result<T>.Error([.. _errors.Value]);

        try
        {
            return factory(this);
        }
        catch (Exception e)
        {
            return ErrorResult.Create(e.Message);
        }
    }

    public ImmutableArray<ErrorResult> GetErrors() => [.. _errors.Value];
}