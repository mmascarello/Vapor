using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CommunicationInterface;
using SettingsManagerInterface;
using StringProtocol;
using VaporServer.BusinessLogic;

namespace VaporServer.Endpoint
{
    public class Server
    {
        private bool exit;
        private readonly List<Socket> clients = new List<Socket>();
        private readonly Logic businessLogic;
        private readonly ISettingsManager settingsManager;
        private readonly ICommunication communication;
        private string serverIpAddress; 
        private int serverPort;
        private int backLog;
        private string serverFilesPath;
        private readonly GameLogic gameLogic;

        public Server(Logic businessLogic,ISettingsManager settingsManager,ICommunication communication)
        {
            this.settingsManager = settingsManager;
            this.businessLogic = businessLogic;
            this.communication = communication;
            this.gameLogic = this.businessLogic.GameLogic;
            this.serverIpAddress = this.settingsManager.ReadSetting(ServerConfig.ServerIpConfigKey);
            this.serverPort = int.Parse(this.settingsManager.ReadSetting(ServerConfig.SeverPortConfigKey));
            this.backLog = int.Parse(this.settingsManager.ReadSetting(ServerConfig.MaxConnectionConfigKey));
            this.serverFilesPath = this.settingsManager.ReadSetting(ServerConfig.ServerFilePath);
        }

        public void Start()
        {
            Console.WriteLine($"ip: {serverIpAddress} - puerto {serverPort} - backlog {backLog}");
            
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            socketServer.Bind(new IPEndPoint(IPAddress.Parse(serverIpAddress),serverPort));
            socketServer.Listen(backLog);
           
            var threadServer = new Thread(()=> ListenForConnections(socketServer));
            threadServer.Start();
            
            ShowMenu();

            HandleServer(socketServer);
        }

        private void HandleServer(Socket socketServer)
        {
            while (!exit)
            {
                var userInput = Console.ReadLine();
                switch (userInput)
                {
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
                    var threadClient = new Thread(() => HandleClient(clientConnected));
                    threadClient.Start();
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
                    
                    communication.ReceiveData(clientSocket, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    
                    switch (header.ICommand)
                    {
                        case CommandConstants.GetGames:
                            GetGames(clientSocket);
                            break;
                        
                        case  CommandConstants.BuyGame:
                            BuyGame(clientSocket, header);
                            break;
                        
                        case CommandConstants.SendImage:
                            SendImage(clientSocket, header);
                            break;
                        
                        case CommandConstants.PublicGame:
                            ProcessGame(clientSocket, header);
                            break;
                        case CommandConstants.ModifyGame:
                            ModifyGame(clientSocket, header);
                            break;
                        case CommandConstants.GameDetail:
                            GetGameDetail(clientSocket, header);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing, will not process more data -> Message {e.Message}..");    
                }
            }
        }

        private void GetGameDetail(Socket clientSocket, Header header)
        {
            
            var receiveGameNameBuffer = new byte[header.IDataLength];
            
            communication.ReceiveData(clientSocket, header.IDataLength, receiveGameNameBuffer);
            
            try
            {
                
                var ratingAverage = this.gameLogic.GetRatingAverage(receiveGameNameBuffer);
                
                //Console.WriteLine($"Obtuve el average {ratingAverage}");
                
                var gameReviews = this.gameLogic.GetReviews(receiveGameNameBuffer);
                
                var gameInfo = this.gameLogic.GetData(receiveGameNameBuffer);//aca va la imagen de portada del juego (ver como manejar el path).

                var response = ratingAverage + "|" + gameReviews + "|" + gameInfo;

                
               
                Console.WriteLine($"{response}");

                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.GameDetail, response.Length);
                
                communication.SendData(clientSocket,headerResponse,response);

            }
            catch (Exception e)
            {
                ErrorResponse(clientSocket,e,CommandConstants.GameDetail);
            }
        }
        
        private void ModifyGame(Socket clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            communication.ReceiveData(clientSocket, header.IDataLength, gameBuffer);

            try
            {
                this.gameLogic.ModifyGame(gameBuffer);
                
                var cover = Encoding.UTF8.GetString(gameBuffer).Split('|')[5];
                
                if (!string.IsNullOrEmpty(cover))
                {
                    communication.ReceiveFile(clientSocket,serverFilesPath);
                }
                
                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.ModifyGame, ResponseConstants.Ok.Length);
                communication.SendData(clientSocket,headerResponse,ResponseConstants.Ok);
                
            }
            catch(Exception e)
            {
                ErrorResponse(clientSocket,e,CommandConstants.ModifyGame);
            }
        }
        
        private void ProcessGame(Socket clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            communication.ReceiveData(clientSocket, header.IDataLength, gameBuffer);

            try
            {
                this.gameLogic.PublicGame(gameBuffer);
                
                communication.ReceiveFile(clientSocket,serverFilesPath);
                
                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.PublicGame, ResponseConstants.Ok.Length);
                communication.SendData(clientSocket,headerResponse,ResponseConstants.Ok);
                
            }
            catch(Exception e)
            {
                ErrorResponse(clientSocket,e,CommandConstants.PublicGame);
            }
        }

        private void SendImage(Socket clientSocket, Header header)
        {
            var dataBuffer = new byte[header.IDataLength];
            
            communication.ReceiveData(clientSocket, header.IDataLength, dataBuffer);
            
            var game = Encoding.UTF8.GetString(dataBuffer);
            
            //buscar la ruta de la imagen con el nombre del juego en la bl
            
            Console.WriteLine(game);
            try
            {
                var cover = this.serverFilesPath + this.gameLogic.GetCover(game);
                Console.WriteLine(cover);
                
                communication.SendFile(clientSocket, cover);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void BuyGame(Socket clientSocket, Header header)
        {
            var dataBuffer = new byte[header.IDataLength];

            communication.ReceiveData(clientSocket, header.IDataLength, dataBuffer);

            var userAndGame = Encoding.UTF8.GetString(dataBuffer).Split('|');
            var user = userAndGame[0];
            var game = userAndGame[1];

            Console.WriteLine($"Message received: {user} and {game}");
            
            Header headerResponse;
            try
            {
                businessLogic.UserLogic.BuyGame(user, game);
                    headerResponse = new Header(HeaderConstants.Response, CommandConstants.BuyGame,
                    ResponseConstants.Ok.Length);
                
                communication.SendData(clientSocket, headerResponse, ResponseConstants.Ok);

                // ToDo:validar porque no funciona adquirir 2 juegos seguidos / adquirir un juego y error.
            }
            catch (Exception e)
            {
                ErrorResponse(clientSocket,e,CommandConstants.BuyGame);
            }
        }

        private void GetGames(Socket clientSocket)
        {
            var gameList = businessLogic.GameLogic.GetGames();
            var games = String.Empty;
            gameList.ForEach(g => games += g.Title + "-");

            var headerToSend = new Header(HeaderConstants.Response, CommandConstants.GetGames,
                games.Length);
            communication.SendData(clientSocket, headerToSend, games);
        }
        
        private void ErrorResponse(Socket clientSocket, Exception e,int command)
        {
            var dataLength = (e.Message.Length + ResponseConstants.Error.Length);
            var errorMessage = ResponseConstants.Error + e.Message;
            var headerResponse = new Header(HeaderConstants.Response, command,
                dataLength);
            communication.SendData(clientSocket, headerResponse, errorMessage);
        }
    }
}
