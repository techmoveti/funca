namespace Funca.Abstractions.Shell;

/// <summary>
///     Use Case Abstraction - Imperative Shell.
/// </summary>
public interface IInteractor<in TInput, TOutput>
    where TInput : class, IMessage
{
    ValueTask<TOutput> InteractAsync(TInput input, CancellationToken cancellationToken);
}