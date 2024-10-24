﻿using System.Net;
using System.Net.Sockets;

namespace SimpleFTP;

public class Server(string addr = "0.0.0.0", short port = 21)
{
    private readonly TcpListener _listener = new(
            IPAddress.Parse(addr),
            port);
    private readonly List<TcpClient> _clients = [];
    private readonly CancellationTokenSource _cts = new();

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