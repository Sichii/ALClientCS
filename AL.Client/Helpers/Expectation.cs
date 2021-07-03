using System;

namespace AL.Client.Helpers
{
    internal readonly struct Expectation<T>
    {
        private readonly T _result;
        private readonly Exception Exception;
        internal string Error => Exception?.Message;
        internal T Result => Exception == null ? _result : throw Exception;
        internal bool Success => Exception == null;

        public static implicit operator bool(Expectation<T> expectation) => expectation.Success;
        public static implicit operator T(Expectation<T> expectation) => expectation.Result;
        public static implicit operator Expectation<T>(Exception exception) => new(exception);
        public static implicit operator Expectation<T>(string error) => new(error);
        public static implicit operator Expectation<T>(T result) => new(result);

        internal Expectation(Exception e)
        {
            _result = default;
            Exception = e;
        }

        internal Expectation(string message)
        {
            _result = default;
            Exception = new InvalidOperationException(message);
        }

        internal Expectation(T result)
        {
            _result = result;
            Exception = null;
        }

        public void ThrowIfUnsuccessful()
        {
            if (!Success)
                throw Exception;
        }
    }
}