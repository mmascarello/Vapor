using CommunicationInterface;
using SettingsManagerInterface;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FileProtocol.Protocol;
using StringProtocol;
using ValidationsImplementations;
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
                        
                        GetCoverPage(socket);

                        break;
                    case "obtener juegos":

                        GetGames(socket);

                        break;
                    case "adquirir juego":
                        
                        BuyGame(socket);

                        break;
                    case "publicar juego":
                        
                        PublicGame(socket);

                        break;
                        
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }

        private void PublicGame(Socket socket)
        {
            Console.WriteLine("Ingrese un titulo");
            var title = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese un genero de los siguientes:...");
            var gender = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese una calificacion del 1 al 5");
            var score = GameValidation.ValidCalification();
            
            Console.WriteLine("Haga una breve descripcion del juego");
            var sinopsis = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese una caratula");
            var coverPage = GameValidation.ValidNotEmpty();

            var publicGame = title + "|" + gender + "|" + score + "|" + sinopsis + "|" + coverPage;

            var header = new Header(HeaderConstants.Request, CommandConstants.PublicGame, publicGame.Length);

            communication.SendData(socket, header, publicGame);
            
            //enviar el coverPage
            
            var response = GetResponse(socket);
            
            Console.WriteLine(response);
            
        }

        private void BuyGame(Socket socket)
        {
            Console.WriteLine("Sugerencia: Imprimir listado de usuarios");
            Console.WriteLine("Ingrese un usuario");
            var userName = Console.ReadLine();
            Console.WriteLine("Sugerencia: Imprimir listado de juegos");
            Console.WriteLine("Ingrese un juego");
            var gameName = Console.ReadLine();
            var userAndGame = userName + "|" + gameName;

            var header = new Header(HeaderConstants.Request, CommandConstants.BuyGame ,userAndGame.Length);
            
            communication.SendData(socket, header, userAndGame);
            try
            {
                var response = GetResponse(socket);
                Console.WriteLine(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("explote");
            }
        }
        

        private void GetGames(Socket socket)
        {
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
        }

        private void GetCoverPage(Socket socket)
        {
            var path = "C:/Vapor/yay.png";
            var headerToSendImg = new Header(HeaderConstants.Request, CommandConstants.SendImage, path.Length);

            communication.SendData(socket, headerToSendImg, path); //esto es para enviar el juego del que necesita la caratua

            try
            {
                communication.ReceiveFile(socket); // esto recibe la imagen
                Console.WriteLine("Message received ");
            }
            catch (Exception e)
            {
                Console.WriteLine("explote");
            }
        }

        private static void ShowMenu()
        {
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("juegos -> obtiene el listado de juegos en el sistema");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
        }
        
        private string GetResponse(Socket socket)
        {
            var newHeaderLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                                  HeaderConstants.DataLength;

            var newBuffer = new Byte[newHeaderLength];
            communication.ReceiveData(socket, newHeaderLength, newBuffer);

            var newHeader = new Header();
            newHeader.DecodeData(newBuffer);

            var newBufferData = new byte[newHeader.IDataLength];
            communication.ReceiveData(socket, newHeader.IDataLength, newBufferData);

            var message = Encoding.UTF8.GetString(newBufferData);
            return message;
        }
    }
}