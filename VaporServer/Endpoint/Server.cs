using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunicationInterface;
using SettingsManagerInterface;
using StringProtocol;
using VaporServer.BusinessLogic;

namespace VaporServer.Endpoint
{
    public class Server
    {
        private bool exit;
        private readonly List<TcpClient> clients = new List<TcpClient>();
        private readonly Logic businessLogic;
        private readonly ISettingsManager settingsManager;
        private readonly ICommunication communication;
        private TcpListener tcpListener; 
        private IPEndPoint serverIpAddress; 
        private int serverPort;
        private int backLog;
        private string serverFilesPath;
        private readonly GameLogic gameLogic;
        //long serverIP; 

        public Server(Logic businessLogic,ISettingsManager settingsManager,ICommunication communication)
        {
            this.settingsManager = settingsManager;
            this.businessLogic = businessLogic;
            this.communication = communication;
            this.gameLogic = this.businessLogic.GameLogic;
            //serverIP = Int64.Parse(this.settingsManager.ReadSetting(ServerConfig.ServerIpConfigKey));
            this.serverPort = int.Parse(this.settingsManager.ReadSetting(ServerConfig.SeverPortConfigKey));
            this.serverIpAddress =  new IPEndPoint(IPAddress.Loopback, serverPort);

            tcpListener = new TcpListener(serverIpAddress);
            
            this.backLog = int.Parse(this.settingsManager.ReadSetting(ServerConfig.MaxConnectionConfigKey));
            this.serverFilesPath = this.settingsManager.ReadSetting(ServerConfig.ServerFilePath);
            System.IO.Directory.CreateDirectory(serverFilesPath);
        }

        public void Start()
        {
            Console.WriteLine($"ip: {serverIpAddress} - puerto {serverPort} - backlog {backLog}");
            tcpListener.Start(backLog);
           Task.Run(()=> ListenForConnectionsAsync(tcpListener));

           ShowMenu();

            HandleServer(tcpListener);
        }

        private void HandleServer(TcpListener tcpListener)
        {
            while (!exit)
            {
                var userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "exit":
                        exit = true;
                        
                            tcpListener.Stop();
                            foreach (var client in clients)
                            {
                                client.GetStream().Close();
                                client.Close();
                            }

                        var fakeSocket = new TcpClient();
                            fakeSocket.Connect(serverIpAddress);
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

        private async Task ListenForConnectionsAsync(TcpListener tcpListener)
        {
            while (!exit)
            {
                try
                {
                    var clientConnected = tcpListener.AcceptTcpClient();
                    clients.Add(clientConnected);
                    Console.WriteLine("Accepted new connection...");
                    await Task.Run( async () => await HandleClientAsync(clientConnected));
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                    exit = true;
                }
            }
            Console.WriteLine("Exiting....");
        }
        
        private async Task HandleClientAsync(TcpClient clientSocket)
        {
            var remoteConnectionClosed = false;  
            while (!exit && !remoteConnectionClosed)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                
                try
                {
                    
                    await communication.ReadDataAsync(clientSocket, headerLength, buffer);
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
                        
                        case CommandConstants.DeleteGame:
                            DeleteGame(clientSocket, header);
                            break;
                        
                        case CommandConstants.LookupGame:
                            LookupGame(clientSocket, header);
                            break;
                        
                        case CommandConstants.PublicCalification:
                            PublicCalification(clientSocket, header);
                            break;
                        
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine($"Thread is closing, will not process more data...");
                    remoteConnectionClosed = true;
                }
            }
        }

        private void PublicCalification(TcpClient clientSocket, Header header)
        {

            var receiveGameAndReview = new Byte[header.IDataLength];
            
            communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameAndReview);
            try
            {
                this.gameLogic.PublicReviewInGame(receiveGameAndReview);
                
                OkResponse(clientSocket,CommandConstants.PublicCalification);
            }
            catch (Exception e)
            {
                ErrorResponse(clientSocket,e.Message,CommandConstants.PublicCalification);
            }
        }

        private void DeleteGame(TcpClient clientSocket, Header header)
        {
            var receiveGameNameBuffer = new byte[header.IDataLength];
            
            communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameNameBuffer);

            try
            {
                this.gameLogic.DeleteGame(receiveGameNameBuffer);
                
                OkResponse(clientSocket,CommandConstants.DeleteGame);
                
            }
            catch (Exception e)
            {
                ErrorResponse(clientSocket,e.Message,CommandConstants.DeleteGame);
            }

        }
        private void LookupGame(TcpClient clientSocket, Header header)
        {
            var receiveGameAttributeBuffer = new byte[header.IDataLength];
            
            communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameAttributeBuffer);

            try
            {
                var gameTitle = this.gameLogic.LookupGame(receiveGameAttributeBuffer);
                
                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.GameDetail, gameTitle.Length);
                
                communication.WriteDataAsync(clientSocket,headerResponse,gameTitle);

            }catch (Exception e)
            {
                ErrorResponse(clientSocket,e.Message,CommandConstants.LookupGame);
            }
        }

        private void GetGameDetail(TcpClient clientSocket, Header header)
        {

            var receiveGameNameBuffer = new byte[header.IDataLength];
            
            communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameNameBuffer);
            
            try
            {
                
                var ratingAverage = this.gameLogic.GetRatingAverage(receiveGameNameBuffer);
                
                var gameReviews = this.gameLogic.GetReviews(receiveGameNameBuffer);
                
                var gameInfo = this.gameLogic.GetData(receiveGameNameBuffer);

                var response = ratingAverage + "|" + gameReviews + "|" + gameInfo + "|";

                Console.WriteLine($"{response}");

                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.GameDetail, response.Length);
                
                communication.WriteDataAsync(clientSocket,headerResponse,response);

            }
            catch (Exception e)
            {
                ErrorResponse(clientSocket,e.Message,CommandConstants.GameDetail);
            }
        }
        
        private void ModifyGame(TcpClient clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            communication.ReadDataAsync(clientSocket, header.IDataLength, gameBuffer);

            try
            {
                this.gameLogic.ModifyGame(gameBuffer);
                
                var cover = Encoding.UTF8.GetString(gameBuffer).Split('|')[5];
                
                if (!string.IsNullOrEmpty(cover))
                {
                    communication.ReadFileAsync(clientSocket,serverFilesPath);
                }
                OkResponse(clientSocket,CommandConstants.ModifyGame);
                
            }
            catch(Exception e)
            {
                ErrorResponse(clientSocket,e.Message,CommandConstants.ModifyGame);
            }
        }
        
        private void ProcessGame(TcpClient clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            communication.ReadDataAsync(clientSocket, header.IDataLength, gameBuffer);
            
            try
            {
                this.gameLogic.PublicGame(gameBuffer);
                
                var cover = Encoding.UTF8.GetString(gameBuffer).Split('|')[4];
                
                if (!string.IsNullOrEmpty(cover))
                {
                    communication.ReadFileAsync(clientSocket,serverFilesPath);
                }
                
                OkResponse(clientSocket,CommandConstants.PublicGame);
            }
            catch(Exception e)
            {
                ErrorResponse(clientSocket,e.Message,CommandConstants.PublicGame);
            }
        }

        private void SendImage(TcpClient clientSocket, Header header)
        {
            var dataBuffer = new byte[header.IDataLength];
            
            communication.ReadDataAsync(clientSocket, header.IDataLength, dataBuffer);
            
            var game = Encoding.UTF8.GetString(dataBuffer);
            
            try
            {
                
                var cover = this.serverFilesPath + this.gameLogic.GetCover(game);
                var exists = File.Exists(cover);
                if (exists)
                {
                    OkResponse(clientSocket,CommandConstants.SendImage);
                    communication.WriteFile(clientSocket, cover);
                }
                else
                {
                    ErrorResponse(clientSocket,"No existe la imagen",CommandConstants.SendImage);
                }
            }
            catch (Exception e)
            {
                ErrorResponse(clientSocket,e.Message,CommandConstants.SendImage);
            }
        }

        private void BuyGame(TcpClient clientSocket, Header header)
        {
            var dataBuffer = new byte[header.IDataLength];

            communication.ReadDataAsync(clientSocket, header.IDataLength, dataBuffer);

            var userAndGame = Encoding.UTF8.GetString(dataBuffer).Split('|');
            var user = userAndGame[0];
            var game = userAndGame[1];

            try
            {
                businessLogic.UserLogic.BuyGame(user, game);

                OkResponse(clientSocket, CommandConstants.BuyGame);
            }
            catch (Exception e)
            {
                ErrorResponse(clientSocket, e.Message, CommandConstants.BuyGame);
            }
        }

        private void GetGames(TcpClient clientSocket)
        {
            var gameList = businessLogic.GameLogic.GetGames();
            var games = String.Empty;
            gameList.ForEach(g => games += g.Title + "-");

            var headerToSend = new Header(HeaderConstants.Response, CommandConstants.GetGames,
                games.Length);
            communication.WriteDataAsync(clientSocket, headerToSend, games);
        }
        
        private void ErrorResponse(TcpClient clientSocket, string error,int command)
        {
            var errorMessage = ResponseConstants.Error + error;
            var dataLength = errorMessage.Length;
            var headerResponse = new Header(HeaderConstants.Response, command,
                dataLength);
            communication.WriteDataAsync(clientSocket, headerResponse, errorMessage);
        }
        
        private void OkResponse(TcpClient clientSocket,int command)
        {
            var headerResponse = new Header(HeaderConstants.Response, command,
                ResponseConstants.Ok.Length);
            communication.WriteDataAsync(clientSocket, headerResponse, ResponseConstants.Ok);
        }
    }
}
