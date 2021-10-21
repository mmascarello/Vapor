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
        private IPAddress serverIP; 

        public Server(Logic businessLogic,ISettingsManager settingsManager,ICommunication communication)
        {
            this.settingsManager = settingsManager;
            this.businessLogic = businessLogic;
            this.communication = communication;
            this.gameLogic = this.businessLogic.GameLogic;
            
            serverIP = IPAddress.Parse(this.settingsManager.ReadSetting(ServerConfig.ServerIpConfigKey));
            this.serverPort = int.Parse(this.settingsManager.ReadSetting(ServerConfig.SeverPortConfigKey));
            this.serverIpAddress =  new IPEndPoint(serverIP, serverPort);

            tcpListener = new TcpListener(serverIpAddress);
            
            this.backLog = int.Parse(this.settingsManager.ReadSetting(ServerConfig.MaxConnectionConfigKey));
            this.serverFilesPath = this.settingsManager.ReadSetting(ServerConfig.ServerFilePath);
            System.IO.Directory.CreateDirectory(serverFilesPath);
        }

        public async Task Start()
        {
            Console.WriteLine($"ip: {serverIpAddress} - puerto {serverPort} - backlog {backLog}");
            tcpListener.Start(backLog);
            await Task.Run(()=> ListenForConnectionsAsync(tcpListener)).ConfigureAwait(false);

            ShowMenu();

            await HandleServer(tcpListener);
        }

        private async Task HandleServer(TcpListener tcpListener)
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
                            await fakeSocket.ConnectAsync(serverIP,serverPort).ConfigureAwait(false);
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
                    var clientConnected = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    clients.Add(clientConnected);
                    Console.WriteLine("Accepted new connection...");
                    await Task.Run( async () => await HandleClientAsync(clientConnected)).ConfigureAwait(false);
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
                            await GetGamesAsync(clientSocket);
                            break;
                        
                        case  CommandConstants.BuyGame:
                            await BuyGameAsync(clientSocket, header);
                            break;
                        
                        case CommandConstants.SendImage:
                            await SendImageAsync(clientSocket, header);
                            break;
                        
                        case CommandConstants.PublicGame:
                            await ProcessGameAsync(clientSocket, header);
                            break;
                        
                        case CommandConstants.ModifyGame:
                            await ModifyGameAsync(clientSocket, header);
                            break;
                        
                        case CommandConstants.GameDetail:
                            await GetGameDetailAsync(clientSocket, header);
                            break;
                        
                        case CommandConstants.DeleteGame:
                            await DeleteGameAsync(clientSocket, header);
                            break;
                        
                        case CommandConstants.LookupGame:
                            await LookupGameAsync(clientSocket, header);
                            break;
                        
                        case CommandConstants.PublicCalification:
                            await PublicCalificationAsync(clientSocket, header);
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

        private async Task PublicCalificationAsync(TcpClient clientSocket, Header header)
        {

            var receiveGameAndReview = new Byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameAndReview).ConfigureAwait(false);
            try
            {
                this.gameLogic.PublicReviewInGame(receiveGameAndReview);
                
                await OkResponse(clientSocket,CommandConstants.PublicCalification);
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.PublicCalification);
            }
        }

        private async Task DeleteGameAsync(TcpClient clientSocket, Header header)
        {
            var receiveGameNameBuffer = new byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameNameBuffer).ConfigureAwait(false);

            try
            {
                this.gameLogic.DeleteGame(receiveGameNameBuffer);
                
                await OkResponse(clientSocket,CommandConstants.DeleteGame);
                
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.DeleteGame);
            }

        }
        private async Task LookupGameAsync(TcpClient clientSocket, Header header)
        {
            var receiveGameAttributeBuffer = new byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameAttributeBuffer).ConfigureAwait(false);

            try
            {
                var gameTitle = this.gameLogic.LookupGame(receiveGameAttributeBuffer);
                
                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.GameDetail, gameTitle.Length);
                
                await communication.WriteDataAsync(clientSocket,headerResponse,gameTitle).ConfigureAwait(false);

            }catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.LookupGame);
            }
        }

        private async Task GetGameDetailAsync(TcpClient clientSocket, Header header)
        {

            var receiveGameNameBuffer = new byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameNameBuffer).ConfigureAwait(false);
            
            try
            {
                
                var ratingAverage = this.gameLogic.GetRatingAverage(receiveGameNameBuffer);
                
                var gameReviews = this.gameLogic.GetReviews(receiveGameNameBuffer);
                
                var gameInfo = this.gameLogic.GetData(receiveGameNameBuffer);

                var response = ratingAverage + "|" + gameReviews + "|" + gameInfo + "|";

                Console.WriteLine($"{response}");

                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.GameDetail, response.Length);
                
                await communication.WriteDataAsync(clientSocket,headerResponse,response).ConfigureAwait(false);

            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.GameDetail);
            }
        }
        
        private async Task ModifyGameAsync(TcpClient clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            await communication.ReadDataAsync(clientSocket, header.IDataLength, gameBuffer).ConfigureAwait(false);

            try
            {
                this.gameLogic.ModifyGame(gameBuffer);
                
                var cover = Encoding.UTF8.GetString(gameBuffer).Split('|')[5];
                
                if (!string.IsNullOrEmpty(cover))
                {
                    await communication.ReadFileAsync(clientSocket,serverFilesPath).ConfigureAwait(false);
                }  await OkResponse(clientSocket,CommandConstants.ModifyGame);
                
            }
            catch(Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.ModifyGame);
            }
        }
        
        private async Task ProcessGameAsync(TcpClient clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            await communication.ReadDataAsync(clientSocket, header.IDataLength, gameBuffer).ConfigureAwait(false);
            
            try
            {
                this.gameLogic.PublicGame(gameBuffer);
                
                var cover = Encoding.UTF8.GetString(gameBuffer).Split('|')[4];
                
                if (!string.IsNullOrEmpty(cover))
                {
                   await communication.ReadFileAsync(clientSocket,serverFilesPath).ConfigureAwait(false);
                }
                
                await OkResponse(clientSocket,CommandConstants.PublicGame);
            }
            catch(Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.PublicGame);
            }
        }

        private async Task SendImageAsync(TcpClient clientSocket, Header header)
        {
            var dataBuffer = new byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, dataBuffer).ConfigureAwait(false);
            
            var game = Encoding.UTF8.GetString(dataBuffer);
            
            try
            {
                
                var cover = this.serverFilesPath + this.gameLogic.GetCover(game);
                var exists = File.Exists(cover);
                if (exists)
                {
                    await OkResponse(clientSocket,CommandConstants.SendImage);
                    await communication.WriteFileAsync(clientSocket, cover);
                }
                else
                {
                   await ErrorResponse(clientSocket,"No existe la imagen",CommandConstants.SendImage);
                }
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.SendImage);
            }
        }

        private async Task BuyGameAsync(TcpClient clientSocket, Header header)
        {
            var dataBuffer = new byte[header.IDataLength];

            await communication.ReadDataAsync(clientSocket, header.IDataLength, dataBuffer).ConfigureAwait(false);

            var userAndGame = Encoding.UTF8.GetString(dataBuffer).Split('|');
            var user = userAndGame[0];
            var game = userAndGame[1];

            try
            {
                businessLogic.UserLogic.BuyGame(user, game);

                await OkResponse(clientSocket, CommandConstants.BuyGame);
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket, e.Message, CommandConstants.BuyGame);
            }
        }

        private async Task GetGamesAsync(TcpClient clientSocket)
        {
            var gameList = businessLogic.GameLogic.GetGames();
            var games = String.Empty;
            gameList.ForEach(g => games += g.Title + "-");

            var headerToSend = new Header(HeaderConstants.Response, CommandConstants.GetGames,
                games.Length);
            await communication.WriteDataAsync(clientSocket, headerToSend, games).ConfigureAwait(false);
        }
        
        private async Task ErrorResponse(TcpClient clientSocket, string error,int command)
        {
            var errorMessage = ResponseConstants.Error + error;
            var dataLength = errorMessage.Length;
            var headerResponse = new Header(HeaderConstants.Response, command,
                dataLength);
            await communication.WriteDataAsync(clientSocket, headerResponse, errorMessage).ConfigureAwait(false);
        }
        
        private async Task OkResponse(TcpClient clientSocket,int command)
        {
            var headerResponse = new Header(HeaderConstants.Response, command,
                ResponseConstants.Ok.Length);
            await communication.WriteDataAsync(clientSocket, headerResponse, ResponseConstants.Ok).ConfigureAwait(false);
        }
    }
}
