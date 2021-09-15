using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StringProtocol;

namespace VaporClient
{
    class Program
    {
        public static bool exit = false;
        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            socket.Connect("127.0.0.1", 20000);
            
            
            var connected = true;
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("message -> envia un mensaje al server");
            Console.WriteLine("login -> autentica el usuario en el sistema");
            Console.WriteLine("user-list -> obtiene el listado de usuarios en el sistema");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
            while (connected)
            {
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "exit":
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        connected = false;
                        break;
                    case "juegos":
                        
                            SendMessage(String.Empty, CommandConstants.GetGames, socket);
                        
                            var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                               HeaderConstants.DataLength;
                            var buffer = new byte[headerLength];
                            try
                            {
                                Console.WriteLine("antes del primer recive data");
                                ReceiveData(socket, headerLength, buffer);
                                var header = new Header();
                                header.DecodeData(buffer);
                                Console.WriteLine("despues de hacer el decode del buffer");
                                //swtich
                                
                                var bufferData2 = new byte[header.IDataLength];
                                Console.WriteLine("antes del segundo recive data");
                                ReceiveData(socket,header.IDataLength,bufferData2);
                                Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData2));
                            }catch(Exception e)
                            {
                                Console.WriteLine("explote");
                            }
                            break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }
        
        private static void ReceiveData(Socket clientSocket,  int length, byte[] buffer)
        {
            var iRecv = 0;
            Console.WriteLine("antes del while en ReciveData");
            while (iRecv < length)
            {
                try
                {
                    Console.WriteLine("antes de recibir la parte en ReciveData");
                    var localRecv = clientSocket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
                    Console.WriteLine("recibi la parte en ReciveData");
                    if (localRecv == 0) // Si recieve retorna 0 -> la conexion se cerro desde el endpoint remoto
                    {
                        if (!exit)
                        {
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
                        }
                        else
                        {
                            throw new Exception("Server is closing");
                        }
                        
                        
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
        
        private static void SendMessage(String mensaje, int command, Socket socket)
        {
            var header = new Header(HeaderConstants.Request, command, mensaje.Length);
            var data = header.BuildRequest();
            var sentBytes = 0;
            while (sentBytes < data.Length)
            {
                sentBytes += socket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
            }

            sentBytes = 0;
            var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
            while (sentBytes < bytesMessage.Length)
            {
                sentBytes += socket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                    SocketFlags.None);
            }
        }
        
        
    }
}