using System;

namespace Reactor.Exceptions
{
    /// <summary>
    /// Helper methods to assist in throwing exceptions. Code copied from the Daemoniq project. http://code.google.com/p/daemoniq
    /// </summary>
    internal static class ThrowHelper
    {
        public static void ThrowArgumentNullIfNull(object o, string paramName)
        {
            if (o == null)
                throw new ArgumentNullException(paramName);
        }

        public static void ThrowArgumentOutOfRangeIf<T>(Predicate<T> predicate, T value, string paramName, string message)
        {
            if (predicate(value))
                throw new ArgumentOutOfRangeException(paramName, message);
        }

        public static void ThrowInvalidOperationExceptionIf<T>(Predicate<T> predicate, T value, string message)
        {
            if (predicate(value))
                throw new InvalidOperationException(message);
        }

        public static void ThrowInvalidOperationExceptionIf<T>(Predicate<T> predicate, T value, string message, params object[] args)
        {
            if (predicate(value))
                throw new InvalidOperationException(string.Format(message, args));
        }

        public static void ThrowArgumentOutOfRangeIf<T>(Predicate<T> predicate, T value, string paramName)
        {
            if (predicate(value))
                throw new ArgumentOutOfRangeException(paramName);
        }

        public static void ThrowArgumentOutOfRangeIfEmpty(string s, string paramName)
        {
            ThrowArgumentOutOfRangeIf(str => string.IsNullOrEmpty(str), s, paramName);
        }

        public static void ThrowArgumentOutOfRangeIfEmpty(char c, string paramName)
        {
            ThrowArgumentOutOfRangeIf(Char.IsWhiteSpace, c, paramName);
        }

        public static void ThrowArgumentOutOfRangeIfZero(int i, string paramName)
        {
            ThrowArgumentOutOfRangeIf(num => num == 0, i, paramName,
                string.Format("Argument '{0}' must not be equal to '0'(zero) ", paramName));
        }
    }
}
