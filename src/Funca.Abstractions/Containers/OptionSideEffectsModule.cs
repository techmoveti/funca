namespace Funca.Abstractions.Containers;

public static partial class Option
{
    extension<T>(Option<T> @this)
    {
        // =========================
        // Tee
        // =========================

        public Option<T> Tee(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsSome)
                action(@this.Value!);

            return @this;
        }

        public Task<Option<T>> Tee(Func<T, Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsNone)
                return Task.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async Task<Option<T>> ExecuteAsync(
                Option<T> option,
                Func<T, Task> action)
            {
                await action(option.Value!);

                return option;
            }
        }

        public ValueTask<Option<T>> TeeValueTask(Func<T, ValueTask> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsNone)
                return ValueTask.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async ValueTask<Option<T>> ExecuteAsync(
                Option<T> option,
                Func<T, ValueTask> action)
            {
                await action(option.Value!);

                return option;
            }
        }

        // =========================
        // IfNone
        // =========================

        public Option<T> IfNone(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsNone)
                action();

            return @this;
        }

        public Task<Option<T>> IfNone(Func<Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsSome)
                return Task.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async Task<Option<T>> ExecuteAsync(
                Option<T> option,
                Func<Task> action)
            {
                await action();

                return option;
            }
        }

        public ValueTask<Option<T>> IfNoneValueTask(Func<ValueTask> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (@this.IsSome)
                return ValueTask.FromResult(@this);

            return ExecuteAsync(@this, action);

            static async ValueTask<Option<T>> ExecuteAsync(
                Option<T> option,
                Func<ValueTask> action)
            {
                await action();

                return option;
            }
        }

        // =========================
        // Match Side Effects
        // =========================

        public void Match(
            Action<T> onSome,
            Action onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            if (@this.IsSome)
                onSome(@this.Value!);
            else
                onNone();
        }

        public Task Match(
            Func<T, Task> onSome,
            Func<Task> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            return @this.IsSome
                ? onSome(@this.Value!)
                : onNone();
        }

        public ValueTask MatchValueTask(
            Func<T, ValueTask> onSome,
            Func<ValueTask> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            return @this.IsSome
                ? onSome(@this.Value!)
                : onNone();
        }
    }
}