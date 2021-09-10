using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.StringProtocol;
using VaporServer.BusinessLogic;

namespace VaporServer.Endpoint
{
    public class Server
    {
        private Logic _businessLogic;
        static bool _exit = false;
        private static List<Socket> _clients = new List<Socket>();

        public Server(Logic business)
        {
            this._businessLogic = business;
        }

        public void Start()
        {
            Console.WriteLine("Holis");
            
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            socketServer.Listen(100);
            
            //Lanzar un thread para manejar las conexiones
            var threadServer = new Thread(()=> ListenForConnections(socketServer));
            threadServer.Start();
            
            Console.WriteLine("Bienvenido al Sistema Server");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
            while (!_exit)
            {
                var userInput = Console.ReadLine();
                switch (userInput)
                {
                    // Cosas a hacer al cerrar el server
                    // 1 - Cerrar el socket que esta escuchando conexiones nuevas
                    // 2 - Cerrar todas las conexiones abiertas desde los clientes
                    case "exit":
                        _exit = true;
                        socketServer.Close(0);
                        foreach (var client in _clients)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                        var fakeSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
                        fakeSocket.Connect("127.0.0.1",20000);
                        break;
                    default:
                        Console.WriteLine("Opcion incorrecta ingresada");
                        break;
                }
            }
        }
        
        private void ListenForConnections(Socket socketServer)
        {
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    Console.WriteLine("Accepted new connection...");
                    var threadcClient = new Thread(() => HandleClient(clientConnected));
                    threadcClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }
            Console.WriteLine("Exiting....");
        }
        
        private void HandleClient(Socket clientSocket)
        {
            while (!_exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    ReceiveData(clientSocket, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            Console.WriteLine("Llegue a login");
                            var bufferDataLogin = new byte[header.IDataLength];
                            ReceiveData(clientSocket,header.IDataLength,bufferDataLogin);
                            var dataInString = Encoding.UTF8.GetString(bufferDataLogin);
                            this.AddUser(dataInString);
                            Console.WriteLine(this._businessLogic.GetUsers().Count);
                            break;
                        
                        case CommandConstants.ListUsers:
                            Console.WriteLine("Not Implemented yet...");
                            //la lista la recorremos y armamos en un string y se lo enviamos
                            
                            break;
                        
                        case CommandConstants.Message:
                            Console.WriteLine("Will receive message to display...");
                            var bufferData = new byte[header.IDataLength];  
                            ReceiveData(clientSocket,header.IDataLength,bufferData);
                            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing, will not process more data -> Message {e.Message}..");    
                }
            }
        }

        private void ReceiveData(Socket clientSocket,  int Length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                try
                {
                    var localRecv = clientSocket.Receive(buffer, iRecv, Length - iRecv, SocketFlags.None);
                    if (localRecv == 0) // Si recieve retorna 0 -> la conexion se cerro desde el endpoint remoto
                    {
                        if (!_exit)
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

        private void AddUser(string user)
        {
            this._businessLogic.AddUser(user);
        }
    }
}

// tenemos que ver donode usamos los mecanimos de mutua exclusion para evitar deadlocks o que 2 usuarios diferentes alteren el mismo recurso