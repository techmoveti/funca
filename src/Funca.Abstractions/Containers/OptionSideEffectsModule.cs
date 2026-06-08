namespace Funca.Abstractions.Containers;

public static partial class Option
{
    extension<T>(Option<T> @this)
    {
        public Option<T> IfNone(Action action)
        {
            if (@this.IsNone)
                action();

            return @this;
        }

        public Option<T> IfNone(Func<Option<T>> fallback) => @this.IsNone
            ? @this
            : fallback();

        public Option<T> IfNone(T fallbackValue) => @this.IsNone
            ? @this
            : Some(fallbackValue);
    }
}