using System;
using System.Runtime.InteropServices;

namespace DataFlowWPF
{
    internal class Guard
    {
        internal static void NotNull<T>(T parameter, [Optional] string argumentName)
        {
            if(parameter == null)
            {
                var argumentText = argumentName == null
                    ? ""
                    : argumentName + " ";

                throw new ArgumentNullException($"Argument {argumentText }must not be null)");
            }
        }

        public static GuardThat<T> That<T>(T argument) where T : IComparable<T>
        {
            NotNull(argument);
            return new GuardThat<T>(argument);
        }
    }

    public class GuardThat<T> where T : IComparable<T>
    {
        private T _argument;

        internal GuardThat(T argument)
        {
            _argument = argument;
        }

        public GuardThatBetweenLower<T> IsBetween(T lowerBound)
        {
            Guard.NotNull(lowerBound);
            return new GuardThatBetweenLower<T>(_argument, lowerBound);
        }
    }

    public class GuardThatBetweenLower<T> where T : IComparable<T>
    {
        private T _argument;
        private T _lowerBound;

        internal GuardThatBetweenLower(T argument, T lowerBound)
        {
            _argument = argument;
            _lowerBound = lowerBound;
        }

        public GuardThatBetween<T> And(T upperBound)
        {
            Guard.NotNull(upperBound);
            return new GuardThatBetween<T>(_argument, _lowerBound, upperBound);
        }
    }

    public class GuardThatBetween<T> where T : IComparable<T>
    {
        private T _argument;
        private T _upperBound;
        private T _lowerBound;

        internal GuardThatBetween(T argument, T lowerBound, T upperBound)
        {
            _argument = argument;
            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public void Inclusive()
        {
            var inBounds = _argument.CompareTo(_lowerBound) >= 0 && _argument.CompareTo(_upperBound) <= 0;

            if (!inBounds)
                throw new ArgumentException($"Argument must be between {_lowerBound} and {_upperBound}");
        }
    }
}