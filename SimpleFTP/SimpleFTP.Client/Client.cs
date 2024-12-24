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
namespace SimpleFTP.Client;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// An implementation of SimpleFTP client
/// </summary>
/// <param name="addr">Destination address</param>
/// <param name="port">Destination port</param>
public class Client(string addr = "127.0.0.1", short port = 21)
{
    public IPAddress Destination { get; } = IPAddress.Parse(addr);
    public short Port { get; } = port;
    private readonly TcpClient _client = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly string _helpmessage = $"This is a SimpleFTP client{Environment.NewLine}" +
        $"Type on of the following commands:{Environment.NewLine}" +
        $"list <path-to-dir> - list files in the directory{Environment.NewLine}" +
        $"get <path-to-file> <where-to-save> - to download file from the server{Environment.NewLine}" +
        $"    (if destination address is empty, then file will be saved to the current{Environment.NewLine}" +
        $"    directory with same name it has on the server){Environment.NewLine}" +
        $"quit - to quit{Environment.NewLine}";

    /// <summary>
    /// Start the client
    /// </summary>
    public async Task Start()
    {
        await _client.ConnectAsync(Destination, Port, _cts.Token);
        await Run();
    }

    /// <summary>
    /// Stop the client
    /// </summary
    public void Stop()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        if (_client.Connected)
        {
            _client.Close();
        }
    }

    private async Task Run()
    {
        NetworkStream stream = _client.GetStream();
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);
        while (!_cts.IsCancellationRequested)
        {
            Console.Write(_helpmessage);
            var request = Console.ReadLine() ?? throw new NullReferenceException("Error when reading a command");
            var requestSplitted = request.Split();
            if (!
               ((requestSplitted.Length == 2 && requestSplitted[0] == "list")
                    ||
            ((requestSplitted.Length == 2 || requestSplitted.Length == 3) && requestSplitted[0] == "get"))
               )
            {
                if (requestSplitted[0] == "quit")
                {
                    Stop();
                    continue;
                }
                Console.WriteLine("Wrong command");
                continue;
            }
            switch (requestSplitted[0])
            {
                case "list":
                    request = "1 " + requestSplitted[1];
                    await writer.WriteLineAsync(request);
                    var reply = await reader.ReadLineAsync(_cts.Token);
                    Console.WriteLine(reply);
                    break;
                case "get":
                    request = "2 " + requestSplitted[1];
                    await writer.WriteLineAsync(request);
                    await HandleGetResponse(stream,
                requestSplitted.Length == 3 && requestSplitted[2] != string.Empty ? requestSplitted[2] : requestSplitted[1].Split('/').Last()
                        );
                    break;
                default:
                    Console.WriteLine("Wrong command");
                    break;
            }
        }
        _client.Close();
    }

    internal async Task HandleGetResponse(Stream stream, string destinationFile)
    {
        int bufferSize = 4096;
        int size = 0;
        int nextByte;
        int i = 0;
        while ((nextByte = stream.ReadByte()) != ' ')
        {
            size += nextByte << i++;
        }

        int downloaded = 0;
        var buffer = new byte[bufferSize];
        await stream.FlushAsync();
        using (var fileStream = File.Open(destinationFile, FileMode.Create))
        {
            Console.WriteLine("Downloading...");
            while (downloaded < size)
            {
                var bytesRead = await stream.ReadAsync(buffer, _cts.Token);
                await fileStream.WriteAsync(
                    (size - downloaded < bufferSize) ?
                        buffer.Take(bytesRead).ToArray() :
                        buffer
                    );

                downloaded += bufferSize;
            }
            Console.WriteLine("Completed. File is located at {0}", destinationFile);
        }
    }
}