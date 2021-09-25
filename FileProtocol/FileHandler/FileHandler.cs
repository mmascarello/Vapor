using System;
using System.IO;
using Common.FileHandler.Interfaces;

namespace FileProtocol.FileHandler
{
    public class FileHandler : IFileHandler
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string GetFileName(string path)
        {
            if (FileExists(path))
            {
                return new FileInfo(path).Name;
            }

            throw new Exception("El archivo no existe");
        }

        public long GetFileSize(string path)
        {
            if (FileExists(path))
            {
                return new FileInfo(path).Length;
            }

            throw new Exception("El archivo no existe");
        }
    }
}