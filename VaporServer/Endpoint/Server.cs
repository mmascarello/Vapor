using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SettingsManagerInterface;
using StringProtocol;
using VaporServer.BusinessLogic;

namespace VaporServer.Endpoint
{
    public class Server
    {
        private Logic businessLogic;
        static bool exit = false;
        private static List<Socket> clients = new List<Socket>();
        private readonly ISettingsManager settingsManager;
        private static string serverIpAddress; 
        private static int serverPort; 
        public Server(Logic business,ISettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            this.businessLogic = business;
        }

        public void Start()
        {
            serverIpAddress = settingsManager.ReadSetting(ServerConfig.ServerIpConfigKey);
            serverPort = Int32.Parse(settingsManager.ReadSetting(ServerConfig.SeverPortConfigKey));
            var backlog = Int32.Parse(settingsManager.ReadSetting(ServerConfig.MaxConnectionConfigKey));
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse(serverIpAddress),serverPort));
            socketServer.Listen(backlog);
            
            //Lanzar un thread para manejar las conexiones
            var threadServer = new Thread(()=> ListenForConnections(socketServer));
            threadServer.Start();
            
            ShowMenu();

            HandleServer(socketServer);
        }

        private static void HandleServer(Socket socketServer)
        {
            while (!exit)
            {
                var userInput = Console.ReadLine();
                switch (userInput)
                {
                    // Cosas a hacer al cerrar el server
                    // 1 - Cerrar el socket que esta escuchando conexiones nuevas
                    // 2 - Cerrar todas las conexiones abiertas desde los clientes
                    case "exit":
                        exit = true;
                        socketServer.Close(0);
                        foreach (var client in clients)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }

                        var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        fakeSocket.Connect(serverIpAddress, serverPort);
                        break;
                    default:
                        Console.WriteLine("Opcion incorrecta ingresada");
                        break;
                }
            }
        }

        private static void ShowMenu()
        {
            Console.WriteLine("Bienvenido al Sistema Server");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
        }

        private void ListenForConnections(Socket socketServer)
        {
            while (!exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    clients.Add(clientConnected);
                    Console.WriteLine("Accepted new connection...");
                    var threadcClient = new Thread(() => HandleClient(clientConnected));
                    threadcClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    exit = true;
                }
            }
            Console.WriteLine("Exiting....");
        }
        
        private void HandleClient(Socket clientSocket)
        {
            while (!exit)
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
                            Console.WriteLine(this.businessLogic.GetUsers().Count);
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
                        
                        case CommandConstants.GetGames:
                            var bufferData2 = new byte[header.IDataLength];  
                            ReceiveData(clientSocket,header.IDataLength,bufferData2);
                           
                            //To do: Verificar el funcionamiento del send data. 
                            // El send data recibe un parametro que se llama command, si aplicamos esta logica para todos queda muy grande el Command Constant. 
                            //Dificil de mantener. 
                            
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing, will not process more data -> Message {e.Message}..");    
                }
            }
        }

        private void ReceiveData(Socket clientSocket,  int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var localRecv = clientSocket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
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
        
        private static void SendData(Socket ourSocket, int command, string data)
        {
            var headerLogin = new Header(HeaderConstants.Request, command, data.Length);
            var dataLogin = headerLogin.GetRequest();
                        
            var sentBytesLogin = 0;
            while (sentBytesLogin < dataLogin.Length)
            {
                sentBytesLogin += ourSocket.Send(dataLogin, sentBytesLogin, dataLogin.Length - sentBytesLogin, SocketFlags.None);
            }

            sentBytesLogin = 0;
                        
            var bytesMessageLogin = Encoding.UTF8.GetBytes(data);
            while (sentBytesLogin < bytesMessageLogin.Length)
            {
                sentBytesLogin += ourSocket.Send(bytesMessageLogin, sentBytesLogin, bytesMessageLogin.Length - sentBytesLogin,
                    SocketFlags.None);
            }
        }

        private void AddUser(string user)
        {
            this.businessLogic.AddUser(user);
        }
    }
}

// tenemos que ver donode usamos los mecanimos de mutua exclusion para evitar deadlocks o que 2 usuarios diferentes alteren el mismo recurso