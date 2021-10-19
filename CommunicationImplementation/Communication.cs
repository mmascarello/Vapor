using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunicationInterface;
using FileProtocol.FileHandler;
using FileProtocol.Protocol;
using StringProtocol;

namespace CommunicationImplementation
{
    public class Communication : ICommunication
    {
        public async Task ReadDataAsync(TcpClient tcpClient, int length, byte[] buffer)
        {
            //Console.WriteLine("ENTRE A RECIVE DATA");
            var networkstream = tcpClient.GetStream();
            
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var localRecv = await networkstream.ReadAsync(buffer, iRecv, length - iRecv);
                    if (localRecv == 0)
                    {
                        /*socket.Shutdown(SocketShutdown.Both);
                        socket.Close();*/
                        networkstream.Close();
                        tcpClient.Close();
                        return;
                    }

                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }

            }
            
            //Console.WriteLine("SALI DE RECIVE DATA");
        }

        public async Task WriteDataAsync(TcpClient tcpClient, Header header, string data)
        {
            //Console.WriteLine("ENTRE A SEND DATA");
            var networkstream = tcpClient.GetStream();
            var dataToSend = header.BuildRequest();
            var sentBytes = 0;
            
            await networkstream.WriteAsync(dataToSend, sentBytes, dataToSend.Length);

            var bytesMessage = Encoding.UTF8.GetBytes(data);
            await networkstream.WriteAsync(bytesMessage, sentBytes, bytesMessage.Length);

            //Console.WriteLine("SALI DE SEND DATA");
        }

        public async Task WriteFile(TcpClient tcpClient, string path)
        {
            //Console.WriteLine("ENTRE A SEND FILE");
            try
            {
                var networkstream = tcpClient.GetStream();
                FileHandler fileHandler = new FileHandler();
                FileStreamHandler fileStreamHandler = new FileStreamHandler();

                //Construimos la info :
                var fileName = fileHandler.GetFileName(path); // nombre del archivo -> XXXX
                var fileSize = fileHandler.GetFileSize(path); // tamaño del archivo -> YYYYYYYY
                
                var header = new FileHeader().Create(fileName, fileSize);
                networkstream.Write(header,0, header.Length);

                var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                networkstream.Write(fileNameBytes, 0,fileNameBytes.Length);

                long parts = SpecificationHelper.GetParts(fileSize);
                //Console.WriteLine("Will Send {0} parts", parts);
                long offset = 0;
                long currentPart = 1;

                while (fileSize > offset)
                {
                    //Console.WriteLine($"current part: {currentPart}");
                    byte[] data;
                    if (currentPart == parts)
                    {
                        var lastPartSize = (int) (fileSize - offset);
                        data = await fileStreamHandler.ReadAsync(path, offset, lastPartSize);
                        offset += lastPartSize;
                    }
                    else
                    {
                        data = await  fileStreamHandler.ReadAsync(path, offset, Specification.MaxPacketSize);
                        offset += Specification.MaxPacketSize;
                    }

                    networkstream.Write(data,0, data.Length);
                    currentPart++;
                }
                //Console.WriteLine("SALI DE SEND FILE");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
        }
        
        public async Task ReadFileAsync(TcpClient tcpClient, string path)
        {
            //Console.WriteLine("ENTRE A RECIVE FILE");
            FileStreamHandler fileStreamHandler = new FileStreamHandler();

            var header = new byte[FileHeader.GetLength()];
            
            await  ReadDataAsync(tcpClient,FileHeader.GetLength(),header);
            
            var fileNameSize = BitConverter.ToInt32(header, 0);//int
            var fileSize = BitConverter.ToInt64(header, Specification.FixedFileNameLength);//long

            var fileNameByte = new byte[fileNameSize];
            await ReadDataAsync(tcpClient,fileNameSize,fileNameByte);
            
            var fileName = Encoding.UTF8.GetString(fileNameByte);

            long parts = SpecificationHelper.GetParts(fileSize);
            long offset = 0; // el archivo armandose desde 0
            long currentPart = 1;
            //Console.WriteLine(path + fileName);
            //Console.WriteLine($"Will receive file {fileName} with size {fileSize} that will be received in {parts} segments");
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    
                    data = new byte[lastPartSize];
                    //Console.WriteLine($"Will receive segment number {currentPart} with size {lastPartSize}");
                    await ReadDataAsync(tcpClient,lastPartSize,data);
                    offset += lastPartSize;
                }
                else
                {
                    data = new byte[Specification.MaxPacketSize];
                    //Console.WriteLine($"Will receive segment number {currentPart} with size {Specification.MaxPacketSize}");
                    await ReadDataAsync(tcpClient,Specification.MaxPacketSize,data);
                    offset += Specification.MaxPacketSize;
                }
                await fileStreamHandler.WriteAsync((path + fileName), data);
                currentPart++;
            }
            //Console.WriteLine("SALI DE RECIVE FILE");
        }
    }
}