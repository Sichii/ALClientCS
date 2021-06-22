using System;

namespace AL.SocketClient.ClientModel
{
    public readonly struct Expectation<T>
    {
        private readonly T _result;
        public readonly string Message;

        public T Result => _result ?? throw new Exception(Message);

        public static implicit operator bool(Expectation<T> expectation) => expectation._result != null;
        public static implicit operator T(Expectation<T> expectation) => expectation.Result;
        public static implicit operator Expectation<T>(string error) => new(error);
        public static implicit operator Expectation<T>(T result) => new(result);

        public Expectation(string message)
        {
            _result = default;
            Message = message;
        }

        public Expectation(T result)
        {
            _result = result;
            Message = null;
        }
    }
}