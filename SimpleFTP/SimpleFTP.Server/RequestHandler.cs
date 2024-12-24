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

using System.Text;

internal class RequestHandler
{
    public async static Task Handle(Stream stream, string? request)
    {
        if (request == null)
        {
            await WriteText(stream, $"Error occured: null request message{Environment.NewLine}");
            return;
        }
        string[] words = request.Split();
        if (words.Length != 2)
        {
            await WriteText(stream, $"Incorrect request format{Environment.NewLine}");
            return;
        }

        switch (words[0])
        {
            case "1":
                await ListRequest(stream, words[1]);
                break;
            case "2":
                await GetRequest(stream, words[1]);
                break;
            default:
                await WriteText(stream,  $"Incorrect request format{Environment.NewLine}");
                break;
        }
    }

    private async static Task ListRequest(Stream stream, string targetDirectory)
    {
        await stream.FlushAsync();
        string reply;
        try
        {
            var files = Directory.GetFileSystemEntries(targetDirectory);
            var builder = new StringBuilder();
            builder.Append(files.Length);
            foreach (var filename in files)
            {
                builder.Append($" {filename} {Directory.Exists(filename)}".Replace('\\', '/'));
            }
            builder.Append(Environment.NewLine);
            reply = builder.ToString();
        }
        catch (DirectoryNotFoundException)
        {
            reply = $"-1{Environment.NewLine}";
        }
        await stream.WriteAsync(Encoding.UTF8.GetBytes(reply));
    }

    private async static Task GetRequest(Stream stream, string target)
    {
        await stream.FlushAsync();
        if (!File.Exists(target))
        {
            await WriteText(stream, "There is no such file");
            return;
        }

        using FileStream file = File.OpenRead(target);
        var size = BitConverter.GetBytes(file.Length);
        await stream.WriteAsync(size);
        stream.WriteByte(0x20);
        await file.CopyToAsync(stream);
        return;
    }

    private async static Task WriteText(Stream stream, string str)
    {
        await stream.WriteAsync(Encoding.UTF8.GetBytes(str));
        await stream.FlushAsync();
    }
}