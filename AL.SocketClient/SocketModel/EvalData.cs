namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents eval data. <br />
    ///     The original language Adventure Land is meant for is JavaScript. <br />
    ///     Javascript allows lazy evaluation of code with a simple eval command, but it's a lot more complicated to do in C#.
    /// </summary>
    public record EvalData
    {
        /// <summary>
        ///     The code to be eval'd and executed. <br />
        /// </summary>
        public string? Code { get; set; } = null!;
    }
}