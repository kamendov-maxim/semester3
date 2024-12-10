/*
MIT License

Copyright (c) 2024 Maxim Kamendov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
namespace SimpleFTP.Server;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// An implementation of SimpleFTP server
/// </summary>
/// <param name="addr">Address for server to listen on</param>
/// <param name="port">Port for server to listen on</param>
public class Server(string addr = "0.0.0.0", short port = 21)
{
    private readonly TcpListener _listener = new(
            IPAddress.Parse(addr),
            port);
    private readonly List<TcpClient> _clients = [];
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Start the server
    /// </summary>
    public async Task Start()
    {
        Console.WriteLine("Start server");
        _listener.Start();
        Console.WriteLine("Server is listening on " + _listener.LocalEndpoint);
        while (true)
        {
            var c = await _listener.AcceptTcpClientAsync(_cts.Token);
            _clients.Add(c);
            var clientTask = HandleConnection(c); 
#pragma warning disable CS4014 // no await so server can handle multiple clients simultaneously
            clientTask.ContinueWith(t => _clients.Remove(c));
        }
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
    }

    private async Task HandleConnection(TcpClient client)
    {
        IPEndPoint? addr = (IPEndPoint?)client.Client.RemoteEndPoint ?? throw new NullReferenceException("Null client address");
        Console.WriteLine("Client {0} connected", addr.ToString());
        NetworkStream stream = client.GetStream();

        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        while (!_cts.IsCancellationRequested)
        {
            var request = await reader.ReadLineAsync(_cts.Token);
            Console.WriteLine(addr.ToString() + ' ' + request); await RequestHandler.Handle(stream, request);
        }

        client.Close();
        Console.WriteLine("Client {0} disconnected", addr.ToString());
    }
}