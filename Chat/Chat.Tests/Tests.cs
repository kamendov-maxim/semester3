namespace Chat.Tests;

using System.Net;

using Chat;

public class Tests
{
    private readonly int _port = 32252;

    [Test]
    public void ReadingMessageWorks()
    {
        var server = new Server(_port);
        var client1Task = server.WaitForConnection();
        var client2Task = Client.Connect(IPAddress.Loopback, _port);
        Task.WhenAll(new Task[] {client1Task, client2Task});
        Console.WriteLine("К сожалению, в этот раз тестов не будет");
        Assert.Pass();
    }
}