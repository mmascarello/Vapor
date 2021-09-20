using CommunicationInterface;
using SettingsManagerInterface;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FileProtocol.Protocol;
using StringProtocol;
using VaporServer;

namespace VaporCliente.Endpoint
{
    public class Client
    {
        private readonly ICommunication communication;
        private readonly ISettingsManager manager;
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
                    case "obtener caratula":
                        
                        var path = "C:/Vapor/yay.png";
                        var headerToSendImg = new Header(HeaderConstants.Request, CommandConstants.SendImage, path.Length);
                        
                        communication.SendData(socket, headerToSendImg, path);//esto es para enviar el juego del que necesita la caratua

                        try
                        {
                            communication.ReceiveFile(socket); // esto recibe la imagen
                            Console.WriteLine("Message received ");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("explote");
                        }
                        
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
                        
                        Console.WriteLine("Sugerencia: Imprimir listado de usuarios");
                        Console.WriteLine("Ingrese un usuario");
                        var userName = Console.ReadLine();
                        Console.WriteLine("Sugerencia: Imprimir listado de juegos");
                        Console.WriteLine("Ingrese un juego");
                        var gameName = Console.ReadLine();
                        var userAndGame = userName + "|" + gameName;
                        
                        communication.SendData(socket,new Header(HeaderConstants.Request,CommandConstants.BuyGame,userAndGame.Length),userAndGame);
                        
                        var newHeaderLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                                           HeaderConstants.DataLength;

                        var newBuffer = new Byte[newHeaderLength];
                        communication.ReceiveData(socket,newHeaderLength,newBuffer);
                        
                        var newHeader = new Header();
                        newHeader.DecodeData(newBuffer);
                        
                        var newBufferData = new byte[newHeader.IDataLength];
                        communication.ReceiveData(socket,newHeader.IDataLength,newBufferData);

                        var message = Encoding.UTF8.GetString(newBufferData);
                        
                        Console.WriteLine(message);
                        
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