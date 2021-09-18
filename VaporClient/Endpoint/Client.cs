using CommunicationInterface;
using SettingsManagerInterface;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using StringProtocol;
using VaporServer;

namespace VaporCliente.Endpoint
{
    public class Client
    {
        private readonly ICommunication communication;
        private readonly ISettingsManager manager;
        public static bool exit = false;
        private static string clientIpAddress;
        private static int clientPort;
        private static string serverIp;
        private static int serverPort;

        public Client(ICommunication communication, ISettingsManager manager)
        {
            this.communication = communication;
            this.manager = manager;
        }

        public void Start()
        {
            clientIpAddress = manager.ReadSetting(ClientConfig.ClientIpConfigKey);
            clientPort = int.Parse(manager.ReadSetting((ClientConfig.ClientPortConfigKey)));
            serverIp = manager.ReadSetting((ClientConfig.ServerIpConfigKey));
            serverPort = int.Parse(manager.ReadSetting((ClientConfig.ServerPortConfigKey)));

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(clientIpAddress), clientPort));
            socket.Connect(serverIp, serverPort);
            
            ShowMenu();
            
            HandleClient(socket);
            
        }

        private void HandleClient(Socket socket)
        {
            var connected = true;

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

                        var headerToSend = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);

                        communication.SendData(socket, headerToSend, String.Empty);

                        var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                           HeaderConstants.DataLength;
                        var buffer = new byte[headerLength];
                        try
                        {
                            communication.ReceiveData(socket, headerLength, buffer);
                            var header = new Header();
                            header.DecodeData(buffer);

                            //swtich

                            var bufferData2 = new byte[header.IDataLength];
                            communication.ReceiveData(socket, header.IDataLength, bufferData2);
                            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData2));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("explote");
                        }

                        break;
                    case "adquirir juego":

                        /*var gameName = Console.ReadLine();
                        communication.SendData(socket,new Header(HeaderConstants.Request,CommandConstants.BuyGame,gameName.Lenght));
                        */
                        
                        
                        break;
                        
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }

        private static void ShowMenu()
        {
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("juegos -> obtiene el listado de juegos en el sistema");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
        }
    }
}