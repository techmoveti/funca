namespace Funca.Abstractions.Shell;

/// <summary>
///     Use Case Abstraction - Imperative Shell.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public interface IInteractor<in TInput, TOutput>
    where TInput : class, IMessage
    where TOutput : class, IMessage
{
    ValueTask<Result<TOutput>> InteractAsync(TInput input, CancellationToken cancellationToken);
}