using System.Net;
using System.Net.Sockets;

namespace SimpleFTP;

public class Client(string addr = "127.0.0.1", short port = 21)
{
    public IPAddress Destination { get; } = IPAddress.Parse(addr);
    public short Port { get; } = port;
    private readonly TcpClient _client = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly string _helpmessage = "This is a SimpleFTP client" +
        "Type on of the following commands:\n" +
        "list <path-to-dir> - list files in the directory\n" +
        "get <path-to-file> <where-to-save> - to download file from the server\n" +
        "    (if destination address is empty, then file will be saved to the current\n" +
        "    directory with same name it has on the server)\n" +
        "quit - to quit\n";

    public async Task Start()
    {
        await _client.ConnectAsync(Destination, Port, _cts.Token);
        await Run();
    }

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
            var request = Console.ReadLine();
            if (request == null)
            {
                throw new NullReferenceException("Error when reading a command");
            }
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