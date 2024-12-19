namespace Chat;

using System.Net;
using System.Net.Sockets;

public class Client(TcpClient client)
{
    private TcpClient client = client;

    /// <summary>
    /// Connect to peer
    /// </summary>
    /// <param name="addr">Address of peer</param>
    /// <param name="port">Port of chat app</param>
    /// <returns>Task which returns client</returns>
    public static async Task<Client> Connect(IPAddress addr, int port)
    {
        TcpClient client = new();
        await client.ConnectAsync(addr, port);
        return new Client(client);
    }

    /// <summary>
    /// Send message
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <returns>Task</returns>
    public async Task SendMessage(string message)
    {
        var writer = new StreamWriter(client.GetStream()) {AutoFlush = true};
        await writer.WriteLineAsync(message);
    }

    /// <summary>
    /// Receive message
    /// </summary>
    /// <returns>Task</returns>
    public async Task ReceiveMessage()
    {
        while (client.Connected)
        {
            var reader = new StreamReader(client.GetStream());
            var message = await reader.ReadLineAsync();
            Console.WriteLine(message);
        }
    }
}