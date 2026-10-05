namespace Funca.Abstractions.Shell;

public interface IOutcome<out TSuccess>
{
}

/// <summary>
///     Use Case Abstraction - Imperative Shell.
/// </summary>
public interface IInteractor<in TInput, out TSuccess, TOutput>
    where TInput : class, IMessage
    where TOutput : IOutcome<TSuccess>
{
    ValueTask<TOutput> InteractAsync(TInput input, CancellationToken cancellationToken);
}