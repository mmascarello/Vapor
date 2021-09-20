using System;
using System.Net.Sockets;
using System.Text;
using Common.FileHandler;
using Common.FileHandler.Interfaces;

using FileProtocol.Protocol;

namespace FileProtocol
{
    public class Methods
    {
        /*private IFileHandler _fileHandler;
        private IFileStreamHandler _fileStreamHandler;

        private ICommunication _communication;
        // client recive
        
        public void ReceiveFile(Socket socket)
        {
            //var header = _networkStreamHandler.Read(Header.GetLength());
            var header = _networkStreamHandler.Read(Header.GetLength());
            
            
            communication.ReceiveData(clientSocket, headerLength, buffer);
            
            var fileNameSize = BitConverter.ToInt32(header, 0); //int - convierte a int el largo de nombre
            
            var fileSize = BitConverter.ToInt64(header, Specification.FixedFileNameLength);//long - convierte a long el largo del archivo

            var fileName = Encoding.UTF8.GetString(_networkStreamHandler.Read(fileNameSize)); // lee el nombre del archivo de la info enviada

            long parts = SpecificationHelper.GetParts(fileSize);
            long offset = 0; // el archivo armandose desde 0
            long currentPart = 1;

            Console.WriteLine($"Will receive file {fileName} with size {fileSize} that will be received in {parts} segments");
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    Console.WriteLine($"Will receive segment number {currentPart} with size {lastPartSize}");
                    data = _networkStreamHandler.Read(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    Console.WriteLine($"Will receive segment number {currentPart} with size {Specification.MaxPacketSize}");
                    data = _networkStreamHandler.Read(Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
        
        
        
        public void SendFile(string path)
        {
            //Construimos la info :
            var fileName = _fileHandler.GetFileName(path); // nombre del archivo -> XXXX
            var fileSize = _fileHandler.GetFileSize(path); // tamaño del archivo -> YYYYYYYY
            var header = new Header().Create(fileName, fileSize);
            _networkStreamHandler.Write(header);

            _networkStreamHandler.Write(Encoding.UTF8.GetBytes(fileName));

            long parts = SpecificationHelper.GetParts(fileSize);
            Console.WriteLine("Will Send {0} parts",parts);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.Read(path, offset, lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _fileStreamHandler.Read(path, offset, Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                _networkStreamHandler.Write(data);
                currentPart++;
            }
        }*/
    }
}