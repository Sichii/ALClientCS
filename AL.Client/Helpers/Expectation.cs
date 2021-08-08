using System;

namespace AL.Client.Helpers
{
    public readonly struct Expectation
    {
        public static readonly Expectation Success = new();

        private readonly Exception? Exception;

        /// <summary>
        ///     If the result did not meet the expectation, this is the error that occurred.
        /// </summary>
        public string? Error => Exception?.Message;

        /// <summary>
        ///     Checks to see if the result is an expected value.
        /// </summary>
        public bool IsSuccessful => Exception == null;

        public static implicit operator bool(Expectation expectation) => expectation.IsSuccessful;
        public static implicit operator Expectation(string error) => new(error);
        public static implicit operator Expectation(Exception exception) => new(exception);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Expectation{T}" /> class.
        /// </summary>
        /// <param name="message">An an error message.</param>
        public Expectation(string message) => Exception = new InvalidOperationException(message);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Expectation{T}" /> class.
        /// </summary>
        /// <param name="e">An exception.</param>
        public Expectation(Exception e) => Exception = e;

        /// <summary>
        ///     Throws an exception if the result is not an expected value.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void ThrowIfUnsuccessful()
        {
            if (Exception != null)
                throw Exception;
        }
    }

    /// <summary>
    ///     Represents an expected result. <br />
    ///     This object allows for gracefully handling unexpected results without throwing an exception.
    /// </summary>
    /// <typeparam name="T">The underlying type of the expected result.</typeparam>
    public readonly struct Expectation<T>
    {
        private readonly T _result;
        private readonly Exception? Exception;

        /// <summary>
        ///     If the result did not meet the expectation, this is the error that occurred.
        /// </summary>
        public string? Error => Exception?.Message;

        /// <summary>
        ///     Checks to see if the result is an expected value.
        /// </summary>
        public bool IsSuccessful => Exception == null;

        /// <summary>
        ///     Attempts to get the expected result. If the result is not as expected, this will instead throw an exception. <br />
        /// </summary>
        /// <exception cref="Exception">The reason the result was not as expected</exception>
        public T Result => Exception == null ? _result : throw Exception;

        public static implicit operator bool(Expectation<T> expectation) => expectation.IsSuccessful;
        public static implicit operator T(Expectation<T> expectation) => expectation.Result;
        public static implicit operator Expectation<T>(T result) => new(result);
        public static implicit operator Expectation<T>(string error) => new(error);
        public static implicit operator Expectation<T>(Exception exception) => new(exception);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Expectation{T}" /> class.
        /// </summary>
        /// <param name="result">An expected result.</param>
        public Expectation(T result)
        {
            _result = result;
            Exception = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Expectation{T}" /> class.
        /// </summary>
        /// <param name="message">An an error message.</param>
        public Expectation(string message)
        {
            Exception = new InvalidOperationException(message);
            _result = default!;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Expectation{T}" /> class.
        /// </summary>
        /// <param name="e">An exception.</param>
        public Expectation(Exception e)
        {
            Exception = e;
            _result = default!;
        }

        /// <summary>
        ///     Throws an exception if the result is not an expected value.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void ThrowIfUnsuccessful()
        {
            if (Exception != null)
                throw Exception;
        }
    }
}