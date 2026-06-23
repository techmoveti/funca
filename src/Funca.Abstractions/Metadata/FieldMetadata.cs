using Funca.Abstractions.Data;

namespace Funca.Abstractions.Metadata;

/// <summary>
/// Represents metadata for a field in a state object, defining its key properties such as code, name, type, and selector expression.
/// </summary>
/// <typeparam name="TState">The type of the state that implements the <see cref="IState"/> interface.</typeparam>
public sealed record FieldMetadata<TState>(
    string Codigo,
    string Nome,
    FieldType Tipo,
    Expression<Func<TState, object?>> Selector)
    where TState : IState;