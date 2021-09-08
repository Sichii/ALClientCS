using System;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.SocketClient.Definitions;

namespace AL.SocketClient.Interfaces
{
    /// <summary>
    ///     Represents a socket connection to Adventure.Land
    /// </summary>
    public interface IALSocketClient : IAsyncDisposable
    {
        /// <summary>
        ///     Occurs when the underlying socket disconnects.
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<string> Disconnected;

        /// <summary>
        ///     Whether or not the socket is currently open.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        ///     Asynchronously connects to an Adventure Land server.
        /// </summary>
        /// <param name="server">
        ///     An object containing information about the server to connect to. Use the <see cref="APIClient" />
        ///     to get this information.
        /// </param>
        Task ConnectAsync(Server server);

        /// <summary>
        ///     Asynchronously disconnects this client from the server. <br />
        ///     Also disposes of the internal socket.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        ///     Serializes the data and Emits a message to the server via socket.io protocol.
        /// </summary>
        /// <param name="emitType">A value indicating the title of the message.</param>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="T">The type of the data being serialized.</typeparam>
        Task EmitAsync<T>(ALSocketEmitType emitType, T data);

        /// <summary>
        ///     Emits a message to the server via socket.io protocol.
        /// </summary>
        /// <param name="emitType">A value indicating the title of the message.</param>
        Task EmitAsync(ALSocketEmitType emitType);

        /// <summary>
        ///     Handles a received socket event based on the title of the message, and how certain messages are set up to be
        ///     handled via <see cref="On{T}" />.
        /// </summary>
        /// <param name="rawJson">The rawJson json of the received message.</param>
        public ValueTask HandleEventAsync(string rawJson);

        /// <summary>
        ///     Instructs the client on how to handle a certain message type. <br />
        ///     There can be any number of handlers stacked for a specific message. They will be executed in the order they were
        ///     configured. <br />
        ///     If any given handler returns <c>true</c>, execution will stop. (it signals that the event was handled)
        /// </summary>
        /// <param name="messageType">The type of message.</param>
        /// <param name="callback">A function to be called when receiving the specified message type.</param>
        /// <typeparam name="T">The type of data to expect and deserialize when receiving the message.</typeparam>
        /// <returns>
        ///     A disposable object that when disposed will remove this handler from the handler list. <br />
        ///     This is the preferred way of handling this, but alternatively, you can use <see cref="Unsub{T}" />.
        /// </returns>
        IAsyncDisposable On<T>(ALSocketMessageType messageType, Func<T, Task<bool>> callback);

        /// <summary>
        ///     If certain constraints restrict you from using the disposable pattern, this can be used to unsubscribe a callback.
        /// </summary>
        /// <param name="messageType">The message type to unsubscribe from.</param>
        /// <param name="callback">The callback to remove from the subscription list.</param>
        /// <typeparam name="T">The type of data that was expected.</typeparam>
        ValueTask Unsub<T>(ALSocketMessageType messageType, Func<T, Task<bool>> callback);
    }
}