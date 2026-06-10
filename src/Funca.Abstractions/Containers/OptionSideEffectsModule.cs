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
            if (@this.IsSome)
                action(@this.Value!);

            return @this;
        }

        public async Task<Option<T>> Tee(
            Func<T, Task> action)
        {
            if (@this.IsSome)
                await action(@this.Value!);

            return @this;
        }

        public async ValueTask<Option<T>> Tee(
            Func<T, ValueTask> action)
        {
            if (@this.IsSome)
                await action(@this.Value!);

            return @this;
        }

        // =========================
        // IfNone
        // =========================

        public Option<T> IfNone(Action action)
        {
            if (@this.IsNone)
                action();

            return @this;
        }

        public async Task<Option<T>> IfNone(
            Func<Task> action)
        {
            if (@this.IsNone)
                await action();

            return @this;
        }

        public async ValueTask<Option<T>> IfNone(
            Func<ValueTask> action)
        {
            if (@this.IsNone)
                await action();

            return @this;
        }

        // =========================
        // Match Side Effects
        // =========================

        public void Match(
            Action<T> onSome,
            Action onNone)
        {
            if (@this.IsSome)
                onSome(@this.Value!);
            else
                onNone();
        }

        public async Task Match(
            Func<T, Task> onSome,
            Func<Task> onNone)
        {
            if (@this.IsSome)
                await onSome(@this.Value!);
            else
                await onNone();
        }

        public async ValueTask Match(
            Func<T, ValueTask> onSome,
            Func<ValueTask> onNone)
        {
            if (@this.IsSome)
                await onSome(@this.Value!);
            else
                await onNone();
        }
    }
}