using System.Text;

namespace SimpleFTP;

internal class RequestHandler
{
    public async static Task Handle(Stream stream, string? request)
    {
        if (request == null)
        {
            await WriteText(stream, "Error occured: null request message\n");
            return;
        }
        string[] words = request.Split();
        if (words.Length != 2)
        {
            await WriteText(stream, "Incorrect request format\n");
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
                await WriteText(stream,  "Incorrect request format\n");
                break;
        }
    }

    private async static Task ListRequest(Stream stream, string targetDirectory)
    {
        string reply;
        try
        {
            var files = Directory.GetFileSystemEntries(targetDirectory);
            var builder = new StringBuilder();
            builder.Append(files.Length);
            foreach (var filename in files)
            {
                builder.Append($" {filename} {Directory.Exists(filename)}");
            }
            builder.Append('\n');
            reply = builder.ToString();
        }
        catch (DirectoryNotFoundException)
        {
            reply = "-1\n";
        }
        await stream.WriteAsync(Encoding.UTF8.GetBytes(reply));
    }

    private async static Task GetRequest(Stream stream, string target)
    {
        if (!File.Exists(target))
        {
            await WriteText(stream, "There is no such file");
            return;
        }

        using (FileStream file = File.OpenRead(target))
        {
            var size = BitConverter.GetBytes(file.Length);
            await stream.WriteAsync(size);
            stream.WriteByte(0x20);
            await file.CopyToAsync(stream);
        }
        return;
    }

    private async static Task WriteText(Stream stream, string str)
    {
        await stream.WriteAsync(Encoding.UTF8.GetBytes(str));
        await stream.FlushAsync();
    }
}
