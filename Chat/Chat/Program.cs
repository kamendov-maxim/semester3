using System.Net;

using Chat;

var helmessage = "help\n";

Client client;

if (args.Length == 1)
{
    int port;
    if (!int.TryParse(args[0], out port))
    {
        Console.Write(helmessage);
    }
    var server = new Server(port);
    client = await server.WaitForConnection();
}
else if (args.Length == 2)
{
    IPAddress addr;
    int port;
    if (!IPAddress.TryParse(args[0], out addr) || !int.TryParse(args[1], out port))
    {
        Console.Write(helmessage);
        return 1;
    }

    client = await Client.Connect(addr, port);
}
else
{
    return 1;
}

var receiveTask = client.ReceiveMessage();

while (true)
{
    var message = Console.ReadLine();
    await client.SendMessage(message);
}

