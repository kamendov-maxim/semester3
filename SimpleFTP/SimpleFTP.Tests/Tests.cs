namespace SimpleFTP.Tests;

using SimpleFTP;

public class Tests
{
    [Test]
    public async Task TestServerRequestsHandling()
    {
        var server = new Server();
        using (FileStream stream1 = File.Create("Output1"))
        {
            using (FileStream stream2 = File.Create("Output2"))
            {
                await RequestHandler.Handle(stream1, "1 ../../../TestFiles/");
                await RequestHandler.Handle(stream2, "2 ../../../TestFiles/a");
            }
        }
        var file1_1 = File.ReadAllText("../../../TestFiles/Answer1").Split();
        var file2_1 = File.ReadAllText("Output1").Split();
        Array.Sort(file1_1); Array.Sort(file2_1);
        Assert.That(file1_1, Is.EqualTo(file2_1));
        File.Delete("Output1");

        byte[] file1_2 = File.ReadAllBytes("../../../TestFiles/Answer2");
        byte[] file2_2 = File.ReadAllBytes("Output2");
        Assert.That(file1_2, Is.EqualTo(file2_2));
        File.Delete("Output2");
    }

    [Test]
    public async Task TestClientDownloading()
    {
        var client = new Client();
        using (FileStream stream = File.Open("../../../TestFiles/Answer2", FileMode.Open))
        {
            await client.HandleGetResponse(stream, "Output");
        }
        byte[] file1 = File.ReadAllBytes("../../../TestFiles/a");
        byte[] file2 = File.ReadAllBytes("Output");
        Assert.That(file1, Is.EqualTo(file2));
        File.Delete("Output");
    }
}
