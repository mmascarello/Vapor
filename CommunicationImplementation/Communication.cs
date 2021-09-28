﻿using System;
using System.Net.Sockets;
using System.Text;
using CommunicationInterface;
using FileProtocol.FileHandler;
using FileProtocol.Protocol;
using StringProtocol;

namespace CommunicationImplementation
{
    public class Communication : ICommunication
    {
        public void ReceiveData(Socket clientSocket, int length, byte[] buffer)
        {
            //Console.WriteLine("ENTRO al RECIVE DATA");
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    //Console.WriteLine($"DATA EN BUFFER ANTES: {Encoding.UTF8.GetString(buffer)}");
                    var localRecv = clientSocket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
                    //Console.WriteLine($"DATA EN BUFFER DESPUES: {Encoding.UTF8.GetString(buffer)}");
                    
                    if (localRecv == 0)
                    {
                        Console.WriteLine("se cerro la conexion porque local rec = 0");
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
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
            //Console.WriteLine("SALGO del RECIVE DATA");
        }

        public void SendData(Socket ourSocket, Header header, string data)
        {
            //Console.WriteLine("ENTRO al SEND DATA");
            //var headercopy = header.BuildRequest();
            //var headercopydecode = new Header();
            //headercopydecode.DecodeData(headercopy);
            
            //Console.WriteLine($"Comando SendData: comando: {headercopydecode.ICommand} - tamano: {headercopydecode.IDataLength} - direccion: {headercopydecode.SDirection}");
            //Console.WriteLine($"Para el buffer en SendData: {data}");
            
            var dataToSend = header.BuildRequest();
            
            var sentBytes = 0;
            //Console.WriteLine($"Primer DataLength: {dataToSend.Length}");
            while (sentBytes < dataToSend.Length)
            {
                sentBytes += ourSocket.Send(dataToSend, sentBytes, dataToSend.Length - sentBytes, SocketFlags.None);
                //Console.WriteLine($"Primer SEND: {sentBytes}");
            }

            var sentDataBytes = 0;
            //Console.WriteLine($"LENGT {data.Length}");
            //Console.WriteLine($"DATA {data}");
            var bytesMessage = Encoding.UTF8.GetBytes(data);
            //Console.WriteLine($"Segundo DataLength: {bytesMessage.Length}");
            while (sentDataBytes < bytesMessage.Length)
            {
                sentDataBytes += ourSocket.Send(bytesMessage, sentDataBytes, bytesMessage.Length - sentDataBytes,
                    SocketFlags.None);
                //Console.WriteLine($"Segundo SEND: {sentDataBytes}");
            }
            //Console.WriteLine("SALGO del SEND DATA");
        }

        public void SendFile(Socket ourSocket, string path)
        {

            try
            {
                FileHandler fileHandler = new FileHandler();
                FileStreamHandler fileStreamHandler = new FileStreamHandler();

                //Construimos la info :
                var fileName = fileHandler.GetFileName(path); // nombre del archivo -> XXXX
                Console.WriteLine(fileName);
                var fileSize = fileHandler.GetFileSize(path); // tamaño del archivo -> YYYYYYYY
                Console.WriteLine(fileSize);

                var header = new FileHeader().Create(fileName, fileSize);
                ourSocket.Send(header, header.Length, SocketFlags.None);

                var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                ourSocket.Send(fileNameBytes, fileNameBytes.Length, SocketFlags.None);

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
                        data = fileStreamHandler.Read(path, offset, lastPartSize);
                        offset += lastPartSize;
                    }
                    else
                    {
                        data = fileStreamHandler.Read(path, offset, Specification.MaxPacketSize);
                        offset += Specification.MaxPacketSize;
                    }

                    ourSocket.Send(data, data.Length, SocketFlags.None);
                    currentPart++;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
        }
        
        public void ReceiveFile(Socket ourSocket, string path)
        {
           
            FileStreamHandler fileStreamHandler = new FileStreamHandler();
            
            var header = new byte[FileHeader.GetLength()];
            
            ReceiveData(ourSocket,FileHeader.GetLength(),header);
            
            var fileNameSize = BitConverter.ToInt32(header, 0);//int
            var fileSize = BitConverter.ToInt64(header, Specification.FixedFileNameLength);//long

            var fileNameByte = new byte[fileNameSize];
            ReceiveData(ourSocket,fileNameSize,fileNameByte);
            
            var fileName = Encoding.UTF8.GetString(fileNameByte);

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
                    
                    data = new byte[lastPartSize];
                    Console.WriteLine($"Will receive segment number {currentPart} with size {lastPartSize}");
                    ReceiveData(ourSocket,lastPartSize,data);
                    offset += lastPartSize;
                }
                else
                {
                    data = new byte[Specification.MaxPacketSize];
                    Console.WriteLine($"Will receive segment number {currentPart} with size {Specification.MaxPacketSize}");
                    ReceiveData(ourSocket,Specification.MaxPacketSize,data);
                    offset += Specification.MaxPacketSize;
                }
                fileStreamHandler.Write((path + fileName), data);
                currentPart++;
            }
        }
    }
}