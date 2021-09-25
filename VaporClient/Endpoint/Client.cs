using CommunicationInterface;
using SettingsManagerInterface;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using StringProtocol;
using ValidationsImplementations;
using VaporServer;

namespace VaporCliente.Endpoint
{
    public class Client
    {
        private readonly ICommunication communication;
        private readonly ISettingsManager manager;
        private string clientIpAddress;
        private int clientPort;
        private  string serverIp;
        private int serverPort;
        private string filesPathRecived;
        private string filesPathToSend;

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
            this.filesPathRecived = manager.ReadSetting(ClientConfig.FilePathForRecive);
            this.filesPathToSend = manager.ReadSetting(ClientConfig.FilePathToSend);
            
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
                        
                        GetCoverPage(socket,String.Empty);

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
                    
                    case "modificar juego":
                        
                        ModifyGame(socket);

                        break;
                    
                    case "ver detalle juego":
                        GetGameDetails(socket);
                        break;
                    
                    case "buscar juegos":
                        LookUpForGame(socket);
                        break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }

        private void LookUpForGame(Socket socket)
        {
            Console.WriteLine("Ingresar atributo a buscar: titulo / genero / clasificacion");
            var lookupAtribute = GameValidation.ValidateValue() ;
            
            Console.WriteLine($"Ingrese {lookupAtribute}:");

            var lookupValue = GameValidation.ValidNotEmpty();

            var lookup = lookupAtribute + "|" + lookupValue + "|";
            
            var header = new Header(HeaderConstants.Request, CommandConstants.LookupGame, lookup.Length);
            
            communication.SendData(socket, header, lookup);

            var gameTitle = GetResponse(socket);
            if (gameTitle.Contains(ResponseConstants.Error))
            {
                Console.WriteLine(gameTitle);
            }
            else
            {
                Console.WriteLine($"Se encontro el juego:{gameTitle}");
            }
        }

        private void GetGameDetails(Socket socket)
        {
            Console.WriteLine("La lista de Juegos disponibles es:");
            GetGames(socket);

            Console.WriteLine("Ingrese el nombre de un Juego:");
            var gameName = GameValidation.ValidNotEmpty();
            
            var header = new Header(HeaderConstants.Request, CommandConstants.GameDetail, gameName.Length);
            
            communication.SendData(socket, header, gameName);

            try
            {
                var response = GetResponse(socket);

                //Console.WriteLine(response);

                var getInformation = response.Split('|');
                var ratingAverage = getInformation[0];
                var gameReviews = getInformation[1];
                var gameGender = getInformation[2];
                var gameSinopsis = getInformation[3];
                var gameESRB = getInformation[4];

                //ToDo: Obtener Imagen

                Console.Write("Mi rating es: "+ratingAverage+"\n");
                Console.Write("Mis reviews: "+gameReviews+"\n");
                Console.Write("Soy un juego de: "+gameGender+"\n");
                Console.Write("Sinopsis: "+gameSinopsis+"\n");
                Console.Write("Esrb edad permitida : "+gameESRB);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        
        private void ModifyGame(Socket socket)
        {
            Console.WriteLine("Ingrese el título del juego a modificar");
            var gameToModify = GameValidation.ValidNotEmpty();
            
            Console.WriteLine("Si no desea modificar la información del campo, dejelo vacio y presione enter ");
            
            Console.WriteLine("Ingrese un título");
            var title = Console.ReadLine();

            Console.WriteLine("Ingrese un genero:");
            var gender = Console.ReadLine();

            Console.WriteLine("Ingrese una calificación del 0 a 5");
            var esbr = GameValidation.ModifyCalification();
            
            Console.WriteLine("Haga una breve descripción del juego");
            var sinopsis = Console.ReadLine();

            Console.WriteLine("Ingrese una carátula");
            var coverPage = Console.ReadLine();
            
            var publicGame = gameToModify +  "|" + title + "|" + gender + "|" + esbr + "|" + sinopsis + "|" + coverPage;
            
            var header = new Header(HeaderConstants.Request, CommandConstants.ModifyGame, publicGame.Length);

            communication.SendData(socket, header, publicGame);

            if (!string.IsNullOrEmpty(coverPage))
            {
                var fileToSend = filesPathToSend + coverPage;
                communication.SendFile(socket, fileToSend);
            }
            
            var response = GetResponse(socket);
            
            Console.WriteLine(response);
        }
        
        private void PublicGame(Socket socket)
        {
            Console.WriteLine("Ingrese un titulo");
            var title = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese un genero de los siguientes:");
            var gender = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese una calificacion del 0 al 5");
            var esbr = GameValidation.ValidCalification();
            
            Console.WriteLine("Haga una breve descripcion del juego");
            var sinopsis = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese una caratula");
            var coverPage = GameValidation.ValidNotEmpty();

            var publicGame = title + "|" + gender + "|" + esbr + "|" + sinopsis + "|" + coverPage;

            var header = new Header(HeaderConstants.Request, CommandConstants.PublicGame, publicGame.Length);

            communication.SendData(socket, header, publicGame);

            var fileToSend = filesPathToSend + coverPage;
            
            communication.SendFile(socket,fileToSend);
            
            var response = GetResponse(socket);
            
            Console.WriteLine(response);
            
        }

        private void BuyGame(Socket socket)
        {
            Console.WriteLine("Ingrese un usuario");
            var userName = GameValidation.ValidNotEmpty();
            
            Console.WriteLine("Ingrese un juego");
            var gameName = GameValidation.ValidNotEmpty();
            var userAndGame = userName + "|" + gameName;

            var header = new Header(HeaderConstants.Request, CommandConstants.BuyGame ,userAndGame.Length);
            
            communication.SendData(socket, header, userAndGame);
            try
            {
                var response = GetResponse(socket);
                Console.WriteLine(response);
            }
            catch (Exception)
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
            catch (Exception)
            {
                Console.WriteLine("explote");
            }
        }

        private void GetCoverPage(Socket socket, string gameName)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                Console.WriteLine("Ingrese el nombre del juego: ");
                gameName = GameValidation.ValidNotEmpty();
            }; 
            
            var headerToSendImg = new Header(HeaderConstants.Request, CommandConstants.SendImage, gameName.Length);

            communication.SendData(socket, headerToSendImg, gameName); 
            
            try//se queda trancado recibiendo la imagen
            {
                communication.ReceiveFile(socket,filesPathRecived); // esto recibe la imagen
                Console.WriteLine("Imagen recibida ");
            }
            catch (Exception)
            {
                Console.WriteLine("explote");
            }
        }

        private void ShowMenu()
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