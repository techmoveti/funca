namespace Funca.Abstractions.Containers;

/// <summary>Starts a typed builder for one to eight independent validation results.</summary>
public readonly struct ResultBuilder
{
    public static ResultBuilder Combine() => new();

    public ResultBuilder<T> Add<T>(Result<T> result)
    {
        if (!result.HasValue)
            throw new ArgumentException("The result must be initialized.", nameof(result));

        return new(result);
    }
}

/// <summary>Combines 1 validation result and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1>
{
    private readonly Result<T1> _result;

    internal ResultBuilder(Result<T1> result) => _result = result;

    public ResultBuilder<T1, T2> Add<T2>(Result<T2> result)
        => new(_result.Combine(result, (values, value) =>
            (values, value)));

    public Result<TOut> Build<TOut>(Func<T1, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(factory);
    }
}

/// <summary>Combines 2 validation results and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1, T2>
{
    private readonly Result<(T1, T2)> _result;

    internal ResultBuilder(Result<(T1, T2)> result) => _result = result;

    public ResultBuilder<T1, T2, T3> Add<T3>(Result<T3> result)
        => new(_result.Combine(result, (values, value) =>
            (values.Item1, values.Item2, value)));

    public Result<TOut> Build<TOut>(Func<T1, T2, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(values =>
            factory(values.Item1, values.Item2));
    }
}

/// <summary>Combines 3 validation results and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1, T2, T3>
{
    private readonly Result<(T1, T2, T3)> _result;

    internal ResultBuilder(Result<(T1, T2, T3)> result) => _result = result;

    public ResultBuilder<T1, T2, T3, T4> Add<T4>(Result<T4> result)
        => new(_result.Combine(result, (values, value) =>
            (values.Item1, values.Item2, values.Item3, value)));

    public Result<TOut> Build<TOut>(Func<T1, T2, T3, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(values =>
            factory(values.Item1, values.Item2, values.Item3));
    }
}

/// <summary>Combines 4 validation results and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1, T2, T3, T4>
{
    private readonly Result<(T1, T2, T3, T4)> _result;

    internal ResultBuilder(Result<(T1, T2, T3, T4)> result) => _result = result;

    public ResultBuilder<T1, T2, T3, T4, T5> Add<T5>(Result<T5> result)
        => new(_result.Combine(result, (values, value) =>
            (values.Item1, values.Item2, values.Item3, values.Item4, value)));

    public Result<TOut> Build<TOut>(Func<T1, T2, T3, T4, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(values =>
            factory(values.Item1, values.Item2, values.Item3, values.Item4));
    }
}

/// <summary>Combines 5 validation results and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1, T2, T3, T4, T5>
{
    private readonly Result<(T1, T2, T3, T4, T5)> _result;

    internal ResultBuilder(Result<(T1, T2, T3, T4, T5)> result) => _result = result;

    public ResultBuilder<T1, T2, T3, T4, T5, T6> Add<T6>(Result<T6> result)
        => new(_result.Combine(result, (values, value) =>
            (values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, value)));

    public Result<TOut> Build<TOut>(Func<T1, T2, T3, T4, T5, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(values =>
            factory(values.Item1, values.Item2, values.Item3, values.Item4, values.Item5));
    }
}

/// <summary>Combines 6 validation results and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1, T2, T3, T4, T5, T6>
{
    private readonly Result<(T1, T2, T3, T4, T5, T6)> _result;

    internal ResultBuilder(Result<(T1, T2, T3, T4, T5, T6)> result) => _result = result;

    public ResultBuilder<T1, T2, T3, T4, T5, T6, T7> Add<T7>(Result<T7> result)
        => new(_result.Combine(result, (values, value) =>
            (values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, values.Item6, value)));

    public Result<TOut> Build<TOut>(Func<T1, T2, T3, T4, T5, T6, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(values =>
            factory(values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, values.Item6));
    }
}

/// <summary>Combines 7 validation results and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1, T2, T3, T4, T5, T6, T7>
{
    private readonly Result<(T1, T2, T3, T4, T5, T6, T7)> _result;

    internal ResultBuilder(Result<(T1, T2, T3, T4, T5, T6, T7)> result) => _result = result;

    public ResultBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Add<T8>(Result<T8> result)
        => new(_result.Combine(result, (values, value) =>
            (values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, values.Item6, values.Item7, value)));

    public Result<TOut> Build<TOut>(Func<T1, T2, T3, T4, T5, T6, T7, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(values =>
            factory(values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, values.Item6, values.Item7));
    }
}

/// <summary>Combines 8 validation results and builds a value only on success.</summary>
public readonly struct ResultBuilder<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private readonly Result<(T1, T2, T3, T4, T5, T6, T7, T8)> _result;

    internal ResultBuilder(Result<(T1, T2, T3, T4, T5, T6, T7, T8)> result) => _result = result;

    public Result<TOut> Build<TOut>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TOut> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _result.Map(values =>
            factory(values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, values.Item6, values.Item7,
                values.Item8));
    }
}