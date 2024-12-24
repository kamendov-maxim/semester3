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
namespace SimpleFTP.Tests;

using SimpleFTP.Server;
using SimpleFTP.Client;

public partial class Tests
{
    private readonly string _projectDir = Path.Combine("..", "..", "..");

    [Test]
    public async Task TestServerRequestsHandling()
    {
        using (FileStream stream1 = File.Create("Output1"))
        {
            using FileStream stream2 = File.Create("Output2");
            await RequestHandler.Handle(stream1, "1 " + Path.Combine(_projectDir, "TestFiles"));
            await RequestHandler.Handle(stream2, "2 " + Path.Combine(_projectDir, "TestFiles", "a"));
        }
        var file1_1 = File.ReadAllText("Output1").Split();
        var file2_1 = File.ReadAllText(Path.Combine(_projectDir, "TestFiles", "Answer1")).Split();
        Array.Sort(file1_1); Array.Sort(file2_1);
        Assert.That(file1_1, Is.EqualTo(file2_1));
        File.Delete("Output1");

        byte[] file1_2 = File.ReadAllBytes("Output2");
        byte[] file2_2 = File.ReadAllBytes(Path.Combine(_projectDir, "TestFiles", "Answer2"));
        Assert.That(file1_2, Is.EqualTo(file2_2));
        File.Delete("Output2");
    }

    [Test]
    public async Task TestClientDownloading()
    {
        var client = new Client();
        using (FileStream stream = File.Open(Path.Combine(_projectDir, "TestFiles", "Answer2"), FileMode.Open))
        {
            await client.HandleGetResponse(stream, "Output");
        }
        byte[] file1 = File.ReadAllBytes(Path.Combine(_projectDir, "TestFiles", "a"));
        byte[] file2 = File.ReadAllBytes("Output");
        Assert.That(file1, Is.EqualTo(file2));
        File.Delete("Output");
    }
}