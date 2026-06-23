using Funca.Abstractions.Data;

namespace Funca.Abstractions.Metadata;

/// <summary>
/// Defines metadata for a state object, including a collection of field metadata that describes the fields available in the state.
/// </summary>
/// <typeparam name="T">The type of state that implements the <see cref="IState"/> interface.</typeparam>
public interface IStateMetadata<T>
    where T : IState
{
    IReadOnlyCollection<FieldMetadata<T>> Fields { get; }
}