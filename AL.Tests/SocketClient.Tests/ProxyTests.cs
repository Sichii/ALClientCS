#region
using System.Net;
using System.Net.Sockets;
using AL.APIClient.Interfaces;
using AL.APIClient.Model;
using AL.APIClient.Response;
using AL.Client;
using AL.Core.Helpers;
using AL.SocketClient;
using Common.Logging;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     A character routed through a proxy has to reach the game over SOCKS5, not over an HTTP CONNECT tunnel that
///     a SOCKS proxy would refuse.
/// </summary>
/// <remarks>
///     Restated against the wire rather than against the library. Asking SocketIOClient whether it would use the
///     proxy, or asking <c>ClientWebSocket</c> what it supports, is a probe agreeing with itself; the only answer
///     that means anything is the bytes that actually leave. A runtime or library change that quietly stopped
///     speaking SOCKS would put the character back on the machine's own IP, and nothing else here would notice.
/// </remarks>
public class ProxyTests
{
    [Test]
    public void NoProxyByDefault()
    {
        var client = new ALSocketClient(Logger());

        client.Proxy
              .Should()
              .BeNull();
    }

    [Test]
    public async Task ConnectsThroughTheProxyAsSocks5()
    {
        //a stand-in proxy that only has to accept one connection and read what is said to it first
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var greeting = ReadFirstBytesAsync(listener);

            var client = new ALSocketClient(Logger(), new WebProxy($"socks5://127.0.0.1:{port}"));

            //the stand-in never answers the handshake, so the connect fails - after the bytes under test are sent
            try
            {
                await client.ConnectAsync(
                    new Server
                    {
                        Address = "na2.adventure.land",
                        Path = "/ws4/"
                    });
            } catch
            {
                //expected: nothing behind the stand-in proxy completes a handshake
            }

            var first = await greeting;

            //SOCKS5: version 5, one authentication method offered, method 0 (none). An HTTP CONNECT tunnel would
            //instead open with the ascii "CONNECT ".
            first.Should()
                 .StartWith([(byte)0x05, (byte)0x01, (byte)0x00]);
        } finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    ///     <c>ALClient.EnsureSocketCarriesProxy</c> throws when the socket about to connect was not built
    ///     with this character's configured proxy.
    /// </summary>
    /// <remarks>
    ///     Two earlier attempts pinned this invariant by scanning <c>ALClient.cs</c>'s source text for how a
    ///     replacement socket's construction was spelled, and each was fooled by a different legal spelling of
    ///     the same construction. A source scan can only ever recognize spellings it was told about. Driving an
    ///     actual reconnect to exercise the real call site needs a live server, so instead this calls the guard
    ///     method directly (<c>internal</c>, visible here via <c>InternalsVisibleTo</c>) against a client
    ///     assembled by hand - the smallest honest way to prove the check fires on a mismatch and stays quiet
    ///     on a match, without caring how any particular socket happened to get built.
    /// </remarks>
    [Test]
    public void ThrowsWhenTheSocketDoesNotCarryTheConfiguredProxy()
    {
        var proxy = new WebProxy("socks5://127.0.0.1:1");

        //the socket was built without the proxy this character is configured to use - the exact shape of the bug
        //an evasive third construction site would produce
        var client = new Warrior("routed-character", new UnusedApiClient(), new ALSocketClient(Logger()))
        {
            SocketProxy = proxy
        };

        var act = () => client.EnsureSocketCarriesProxy();

        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("*routed-character*")
           .WithMessage("*NewSocket()*");
    }

    [Test]
    public void DoesNotThrowWhenTheSocketCarriesTheConfiguredProxy()
    {
        var proxy = new WebProxy("socks5://127.0.0.1:1");

        var client = new Warrior("routed-character", new UnusedApiClient(), new ALSocketClient(Logger(), proxy))
        {
            SocketProxy = proxy
        };

        var act = () => client.EnsureSocketCarriesProxy();

        act.Should()
           .NotThrow();
    }

    private static FormattedLogger Logger() => new("proxy-test", LogManager.GetLogger<ALSocketClient>());

    private static async Task<byte[]> ReadFirstBytesAsync(TcpListener listener)
    {
        using var accepted = await listener.AcceptTcpClientAsync();
        await using var stream = accepted.GetStream();

        var buffer = new byte[16];
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var read = await stream.ReadAsync(buffer, timeout.Token);

        return buffer[..read];
    }

    /// <summary>
    ///     The constructor rejects a null API client and nothing under test calls one.
    /// </summary>
    private sealed class UnusedApiClient : IAlApiClient
    {
        public AuthUser Auth => throw new NotSupportedException();

        public IAsyncEnumerable<Mail> GetMailAsync() => throw new NotSupportedException();

        public IAsyncEnumerable<MerchantInfo> GetMerchantsAsync() => throw new NotSupportedException();

        public Task<ServersAndCharactersResponse> GetServersAndCharactersAsync() => throw new NotSupportedException();

        public Task ReadMailAsync(Mail mail) => throw new NotSupportedException();

        public Task RenewAuth() => throw new NotSupportedException();
    }
}
