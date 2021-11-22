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
            
            var networkstream = tcpClient.GetStream();
            
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var localRecv = await networkstream.ReadAsync(buffer, iRecv, length - iRecv).ConfigureAwait(false);
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
            
            
        }

        public async Task WriteDataAsync(TcpClient tcpClient, Header header, string data)
        {
            
            var networkstream = tcpClient.GetStream();
            var dataToSend = header.BuildRequest();
            var sentBytes = 0;
            
            await networkstream.WriteAsync(dataToSend, sentBytes, dataToSend.Length).ConfigureAwait(false);

            var bytesMessage = Encoding.UTF8.GetBytes(data);
            await networkstream.WriteAsync(bytesMessage, sentBytes, bytesMessage.Length).ConfigureAwait(false);

            
        }

        public async Task WriteFileAsync(TcpClient tcpClient, string path)
        {
            
            try
            {
                var networkstream = tcpClient.GetStream();
                FileHandler fileHandler = new FileHandler();
                FileStreamHandler fileStreamHandler = new FileStreamHandler();

               
                var fileName = fileHandler.GetFileName(path); 
                var fileSize = fileHandler.GetFileSize(path); 
                
                var header = new FileHeader().Create(fileName, fileSize);
                await networkstream.WriteAsync(header,0, header.Length).ConfigureAwait(false);

                var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                await networkstream.WriteAsync(fileNameBytes, 0,fileNameBytes.Length).ConfigureAwait(false);

                long parts = SpecificationHelper.GetParts(fileSize);
                
                long offset = 0;
                long currentPart = 1;

                while (fileSize > offset)
                {
                    
                    byte[] data;
                    if (currentPart == parts)
                    {
                        var lastPartSize = (int) (fileSize - offset);
                        data = await fileStreamHandler.ReadAsync(path, offset, lastPartSize).ConfigureAwait(false);
                        offset += lastPartSize;
                    }
                    else
                    {
                        data = await  fileStreamHandler.ReadAsync(path, offset, Specification.MaxPacketSize).ConfigureAwait(false);
                        offset += Specification.MaxPacketSize;
                    }

                    await networkstream.WriteAsync(data,0, data.Length).ConfigureAwait(false);
                    currentPart++;
                }
                
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
        }
        
        public async Task ReadFileAsync(TcpClient tcpClient, string path)
        {
            
            FileStreamHandler fileStreamHandler = new FileStreamHandler();

            var header = new byte[FileHeader.GetLength()];
            
            await  ReadDataAsync(tcpClient,FileHeader.GetLength(),header).ConfigureAwait(false);
            
            var fileNameSize = BitConverter.ToInt32(header, 0);
            var fileSize = BitConverter.ToInt64(header, Specification.FixedFileNameLength);

            var fileNameByte = new byte[fileNameSize];
            await ReadDataAsync(tcpClient,fileNameSize,fileNameByte).ConfigureAwait(false);
            
            var fileName = Encoding.UTF8.GetString(fileNameByte);

            long parts = SpecificationHelper.GetParts(fileSize);
            long offset = 0; 
            long currentPart = 1;
            
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    
                    data = new byte[lastPartSize];
                    
                    await ReadDataAsync(tcpClient,lastPartSize,data).ConfigureAwait(false);
                    offset += lastPartSize;
                }
                else
                {
                    data = new byte[Specification.MaxPacketSize];
                    
                    await ReadDataAsync(tcpClient,Specification.MaxPacketSize,data).ConfigureAwait(false);
                    offset += Specification.MaxPacketSize;
                }
                await fileStreamHandler.WriteAsync((path + fileName), data).ConfigureAwait(false);
                currentPart++;
            }
            
        }
    }
}