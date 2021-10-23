using System;
using System.IO;
using System.Threading.Tasks;
using FileProtocol.FileHandler.Interfaces;

namespace FileProtocol.FileHandler
{
    public class FileStreamHandler : IFileStreamHandler
    {
        public async Task<byte[]> ReadAsync(string path, long offset, int length)
        {
            var data = new byte[length];

            await using (var fs = new FileStream(path, FileMode.Open))
            {
                fs.Position = offset;
                var bytesRead = 0;
                while (bytesRead < length)
                {
                    var read = await fs.ReadAsync(data, bytesRead, length - bytesRead).ConfigureAwait(false);
                    if (read == 0)
                    {
                        throw new Exception("Couldn't not read file");
                    }
                    bytesRead += read;
                }
            }

            return data;
        }

        public async Task WriteAsync(string fileName, byte[] data)
        {
            if (File.Exists(fileName))
            {
                await using (var fs = new FileStream(fileName, FileMode.Append))
                {
                    await fs.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                }
            }
            else
            {
                await using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    await fs.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                }
            }
        }
    }
}