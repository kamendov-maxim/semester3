namespace Chat;

using System.Net.Sockets;

/// <summary>
/// Class that represents server
/// </summary>
public class Server
{
    private readonly TcpListener _listener;  
    private TcpClient? _client;

    /// <summary>
    /// Constructor of the class
    /// </summary>
    /// <param name="port">Port on which app will work</param>
    public Server(int port)
    {
        _listener = new(System.Net.IPAddress.Any, port);
        _listener.Start();
    }

    /// <summary>
    /// Awaitable method that waits for connection
    /// </summary>
    /// <returns>Task that returns client</returns>
    public async Task<Client> WaitForConnection()
    {
        while (_client == null)
        {
            _client = await _listener.AcceptTcpClientAsync();
        }
        return new Client(_client);
    }
}









