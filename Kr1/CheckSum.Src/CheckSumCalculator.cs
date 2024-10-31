using System.Security.Cryptography;

namespace CheckSum;

public class CheckSumCalculator
{
    public static byte[] Calculate(string path)
    {
        FileAttributes attr = File.GetAttributes(path);
        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            return HashDirectory(new DirectoryInfo(path));
        else
            return HashFile(new FileInfo(path));
    }

    public static async Task<byte[]> CalculateAsync(string path)
    {
        FileAttributes attr = File.GetAttributes(path);
        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            return await HashDirectoryAsync(new DirectoryInfo(path));
        else
            return await HashFileAsync(new FileInfo(path));
    }

    private static byte[] HashDirectory(DirectoryInfo info)
    {
        using (MemoryStream stream = new())
        {
            stream.Write(System.Text.Encoding.UTF8.GetBytes(info.Name));
            var subdirectories = info.GetDirectories();
            var files = info.GetFiles();
            Array.Sort(subdirectories.Select(x => x.Name).ToArray(), subdirectories);
            Array.Sort(files.Select(file => file.Name).ToArray(), files);
            // subdirectories.Select(d=>d).Concat(files.Select(f=>f));
            foreach (var dirHash in subdirectories.Select(HashDirectory))
            {
                stream.Write(dirHash);
            }
            foreach (var fileHash in files.Select(HashFile))
            {
                stream.Write(fileHash);
            }
            return MD5.HashData(stream);
        }
    }

    private static async Task<byte[]> HashDirectoryAsync(DirectoryInfo info)
    {
        using (MemoryStream stream = new())
        {
            await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(info.Name));
            var subdirectories = info.GetDirectories();
            var files = info.GetFiles();
            Array.Sort(subdirectories.Select(x => x.Name).ToArray(), subdirectories);
            Array.Sort(files.Select(file => file.Name).ToArray(), files);
            // subdirectories.Select(d=>d).Concat(files.Select(f=>f));
            foreach (var dirHash in subdirectories.Select(HashDirectory))
            {
                await stream.WriteAsync(dirHash);
            }
            foreach (var fileHash in files.Select(HashFile))
            {
                await stream.WriteAsync(fileHash);
            }
            return await MD5.HashDataAsync(stream);
        }
    }


    private static async Task<byte[]> HashFileAsync(FileInfo file)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = file.OpenRead())
            {
                return await MD5.HashDataAsync(stream);
            }
        }
    }

    private static byte[] HashFile(FileInfo file)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = file.OpenRead())
            {
                return MD5.HashData(stream);
            }
        }
    }
}