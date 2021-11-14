using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunicationInterface;
using Domain;
using SettingsManagerInterface;
using StringProtocol;
using ValidationsImplementations;
using VaporServer.BusinessLogic;
using VaporServer.MQHandler;

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
        private IPAddress serverIP; 
        private readonly GameLogic gameLogic;
        private readonly UserLogic userLogic;
        private readonly MQProducer logsProducer;
        private User userLogged;

        public Server(Logic businessLogic,ISettingsManager settingsManager,ICommunication communication)
        {
            this.settingsManager = settingsManager;
            this.businessLogic = businessLogic;
            this.communication = communication;
            this.gameLogic = this.businessLogic.GameLogic;
            this.userLogic = this.businessLogic.UserLogic;
            serverIP = IPAddress.Parse(this.settingsManager.ReadSetting(ServerConfig.ServerIpConfigKey));
            this.serverPort = int.Parse(this.settingsManager.ReadSetting(ServerConfig.SeverPortConfigKey));
            this.serverIpAddress =  new IPEndPoint(serverIP, serverPort);

            tcpListener = new TcpListener(serverIpAddress);
            
            this.backLog = int.Parse(this.settingsManager.ReadSetting(ServerConfig.MaxConnectionConfigKey));
            this.serverFilesPath = this.settingsManager.ReadSetting(ServerConfig.ServerFilePath);
            System.IO.Directory.CreateDirectory(serverFilesPath);
            this.logsProducer = MQProducer.Instance;
        }

        public async Task Start()
        {
            Console.WriteLine($"ip: {serverIpAddress} - puerto {serverPort} - backlog {backLog}");
            tcpListener.Start(backLog);

            var task = Task.Run(async ()=> await ListenForConnectionsAsync(tcpListener)).ConfigureAwait(false);
            
            ShowMenu();
            
            var serverTask = Task.Run(async ()=> await HandleServer(tcpListener)).ConfigureAwait(false);

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
                        try
                        {
                            foreach (var client in clients)
                            {
                                client.GetStream().Close();
                                client.Close();
                            }

                            var fakeSocket = new TcpClient();
                            await fakeSocket.ConnectAsync(serverIP, serverPort).ConfigureAwait(false);

                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Cerrando el servidor");
                        }
                        break;
                    
                    case "modificar usuario":

                       await ModifyUser();

                        break;
                    
                    case "crear usuario":

                        await CreateUser();
                        
                        break;
                    
                    case "eliminar usuario":

                        await DeleteUser();
                        
                        break;
                    case "obtener usuarios":

                        GetUsers();
                        
                        break;
                    
                    case "ver detalle usuario":

                        UserDetail();
                        
                        break;
                    
                    case "help":

                        ShowMenu();
                        
                        break;
                        
                    default:
                        Console.WriteLine("Opcion incorrecta ingresada");
                        break;
                }
            }
        }

        private void UserDetail()//ToDo: Ver si hacer log de esto
        {
            try
            {
                Console.WriteLine("Ingrese el nombre de un usuario");
                var user = Console.ReadLine();
                var userinfo = userLogic.UserDetail(user);
                var userifo = userinfo.Split('|');
                Console.WriteLine($"El nombre del usuario es: {userifo[0]}");
                Console.WriteLine($"La contrasena del usuario es: {userifo[1]}");
                Console.WriteLine($"Juego/s: {userifo[2]}");
            }
            catch (Exception e)
            {
                Console.WriteLine("No se encontro el usuario");
            }
            
        }

        private void GetUsers()//ToDo: Ver si hacer log de esto.
        {
            try
            {
                var usuarios = userLogic.GetUsers();
                Console.WriteLine(usuarios);
            }
            catch (Exception e)
            {
                Console.WriteLine("No se encontraron usuarios");
            }
        }

        private async Task DeleteUser()
        {
            try
            {
                Console.WriteLine("Que usuario desdea eliminar?");
                var user = Console.ReadLine();
                userLogic.DeleteUser(user);
                Console.WriteLine("Usuario eliminado con exito.");
               
                await SendLog(CommandConstants.DeleteUserDescription,"", ResponseConstants.Ok+" usuario eliminado").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var message = " Este usuario no exite";
                Console.WriteLine(message);
                await SendLog(CommandConstants.DeleteUserDescription,"", ResponseConstants.Error + message).ConfigureAwait(false);
            }
        }

        private async Task CreateUser()
        {
            Console.WriteLine("Ingrese nombre para el usuario");
            var usuario = UserValidation.ValidNotEmpty().ToLower();
            
            var invalid = userLogic.ExistsUser(usuario);
           
            while (invalid)
            {
                Console.WriteLine("Ingrese el nombre de un usuario NO existente");
                usuario = UserValidation.ValidNotEmpty().ToLower();
                invalid = userLogic.ExistsUser(usuario);//valido si existe retorna true y va a volver a entrar al while sino false y se va. 
            }
            
            Console.WriteLine("Ingrese contrasena para el usuario");
            var pass = UserValidation.ValidNotEmpty().ToLower();
            
            userLogic.CreateUser(usuario,pass);

            Console.WriteLine($"El usuario '{usuario}' fue creado con exito");
            
            await SendLog(CommandConstants.CreateUserDescription,"", ResponseConstants.Ok+"Usuario creado").ConfigureAwait(false);

        }

        private async Task ModifyUser()
        {
            try
            {
                Console.WriteLine("Que usuario desdea modificar?");
                var usuario = UserValidation.ValidNotEmpty().ToLower();

                Console.WriteLine($"Usted va a modificar el usuario{usuario} \n" +
                                  " Si no desea modificar la informacion del campo, dejelo vacio y presione enter");

                Console.WriteLine("Nombre nuevo");
                var newUsuario = Console.ReadLine().ToLower();
                
                var invalid = userLogic.ExistsUser(newUsuario);
                while (invalid)
                {
                    Console.WriteLine("Ingrese el nombre de un usuario NO existente");
                    newUsuario = Console.ReadLine().ToLower();
                    invalid = userLogic.ExistsUser(newUsuario);//valido si existe retorna true y va a volver a entrar al while sino false
                                                               //y se va. 
                }
            
                Console.WriteLine("Ingrese contrasena nueva");
                var pass = Console.ReadLine().ToLower();

                userLogic.ModifyUser(usuario,newUsuario,pass);
                Console.WriteLine("Usuario modificado con exito");
                await SendLog(CommandConstants.ModifyUserDescription,"", ResponseConstants.Ok).ConfigureAwait(false);
                
            }
            catch (Exception e)
            {
                var message = "  No se encontro el usuario a modifcar";
                Console.WriteLine(message);
                await SendLog(CommandConstants.ModifyUserDescription,"", ResponseConstants.Error +  message).ConfigureAwait(false);
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine("Bienvenido al Sistema Server");
            Console.WriteLine(" Ingrese 'help' -> cada vez que desee ver 'opciones validas'.");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine(" exit -> abandonar el programa");
            Console.WriteLine(" obtener usuarios -> para obtener lista de usuarios.");
            Console.WriteLine(" modificar usuario -> para cambiar nombre y/o contrasena");
            Console.WriteLine(" crear usuario -> para crear usuario con nombre y contrasena");
            Console.WriteLine(" eliminar usuario -> para eliminar usuario.");
            Console.WriteLine(" ver detalle usuario -> para ver el nombre, la contrasena, y juegos del usuario.");
            Console.WriteLine("Ingrese su opcion: ");
        }

        private async Task ListenForConnectionsAsync(TcpListener tcpListener)
        {
            while (!exit)
            {
                try
                {
                    var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    clients.Add(tcpClientSocket);
                    Console.WriteLine("Accepted new connection...");
                    var task = Task.Run( async () => await HandleClientAsync(tcpClientSocket)).ConfigureAwait(false);
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

                    await communication.ReadDataAsync(clientSocket, headerLength, buffer).ConfigureAwait(false);
                    var header = new Header();
                    header.DecodeData(buffer);

                    switch (header.ICommand)
                    {
                        case CommandConstants.GetGames:
                            await GetGamesAsync(clientSocket).ConfigureAwait(false);
                            break;

                        case CommandConstants.BuyGame:
                            await BuyGameAsync(clientSocket, header).ConfigureAwait(false);
                            break;

                        case CommandConstants.SendImage:
                            await SendImageAsync(clientSocket, header).ConfigureAwait(false);
                            break;

                        case CommandConstants.PublicGame:
                            await ProcessGameAsync(clientSocket, header).ConfigureAwait(false);
                            break;

                        case CommandConstants.ModifyGame:
                            await ModifyGameAsync(clientSocket, header).ConfigureAwait(false);
                            break;

                        case CommandConstants.GameDetail:
                            await GetGameDetailAsync(clientSocket, header).ConfigureAwait(false);
                            break;

                        case CommandConstants.DeleteGame:
                            await DeleteGameAsync(clientSocket, header).ConfigureAwait(false);
                            break;

                        case CommandConstants.LookupGame:
                            await LookupGameAsync(clientSocket, header).ConfigureAwait(false);
                            break;

                        case CommandConstants.PublicCalification:
                            await PublicCalificationAsync(clientSocket, header).ConfigureAwait(false);
                            break;
                        case CommandConstants.Login:
                            await Login(clientSocket, header).ConfigureAwait(false);
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

        private async Task Login(TcpClient clientSocket, Header header)
        {

            var login = new Byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, login).ConfigureAwait(false);
            try
            {
                userLogged = this.userLogic.Login(login);
                
                await OkResponse(clientSocket,CommandConstants.PublicCalification).ConfigureAwait(false);
                
                await SendLog(CommandConstants.LoginDescription,"",ResponseConstants.Ok).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.PublicCalification).ConfigureAwait(false);
                
                await SendLog(CommandConstants.LoginDescription,"",ResponseConstants.Error+e.Message).ConfigureAwait(false);
            }
        }
        
        private async Task PublicCalificationAsync(TcpClient clientSocket, Header header)
        {

            var receiveGameAndReview = new Byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameAndReview).ConfigureAwait(false);
            
            var info = Encoding.UTF8.GetString(receiveGameAndReview).Split('|');
            var game = info[0];
            
            try
            {
                this.gameLogic.PublicReviewInGame(receiveGameAndReview);
                
                await OkResponse(clientSocket,CommandConstants.PublicCalification).ConfigureAwait(false);
                
                await SendLog(CommandConstants.PublicCalificationDescription,game,ResponseConstants.Ok).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.PublicCalification).ConfigureAwait(false);
                await SendLog(CommandConstants.PublicCalificationDescription,game,ResponseConstants.Error+e.Message).ConfigureAwait(false);
            }
        }

        private async Task DeleteGameAsync(TcpClient clientSocket, Header header)
        {
            var receiveGameNameBuffer = new byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameNameBuffer).ConfigureAwait(false);
            
            var info = Encoding.UTF8.GetString(receiveGameNameBuffer).Split('|');
            var game = info[0];
            
            try
            {
                this.gameLogic.DeleteGame(receiveGameNameBuffer);
                
                await OkResponse(clientSocket,CommandConstants.DeleteGame).ConfigureAwait(false);
                await SendLog(CommandConstants.DeleteGameDescription,game, ResponseConstants.Ok).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.DeleteGame).ConfigureAwait(false);
                await SendLog(CommandConstants.DeleteGameDescription,game,ResponseConstants.Error+e.Message).ConfigureAwait(false);
            }

        }
        private async Task LookupGameAsync(TcpClient clientSocket, Header header)
        {
            var receiveGameAttributeBuffer = new byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameAttributeBuffer).ConfigureAwait(false);

            var info = Encoding.UTF8.GetString(receiveGameAttributeBuffer).Split('|');
            var game = info[0];

            try
            {
                var gameTitle = this.gameLogic.LookupGame(receiveGameAttributeBuffer);
                
                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.GameDetail, gameTitle.Length);
                
                await communication.WriteDataAsync(clientSocket,headerResponse,gameTitle).ConfigureAwait(false);
                
                await SendLog(CommandConstants.LookupGameDescription,game, ResponseConstants.Ok).ConfigureAwait(false);

            }catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.LookupGame).ConfigureAwait(false);
                await SendLog(CommandConstants.LookupGameDescription, game, ResponseConstants.Error + e.Message).ConfigureAwait(false);
            }
        }

        private async Task GetGameDetailAsync(TcpClient clientSocket, Header header)
        {

            var receiveGameNameBuffer = new byte[header.IDataLength];
            
            await communication.ReadDataAsync(clientSocket, header.IDataLength, receiveGameNameBuffer).ConfigureAwait(false);
            
            var info = Encoding.UTF8.GetString(receiveGameNameBuffer).Split('|');
            var game = info[0];
            
            try
            {
                
                var ratingAverage = this.gameLogic.GetRatingAverage(receiveGameNameBuffer);
                
                var gameReviews = this.gameLogic.GetReviews(receiveGameNameBuffer);
                
                var gameInfo = this.gameLogic.GetData(receiveGameNameBuffer);

                var response = ratingAverage + "|" + gameReviews + "|" + gameInfo + "|";

                Console.WriteLine($"{response}");

                var headerResponse = new Header(HeaderConstants.Response, CommandConstants.GameDetail, response.Length);
                
                await communication.WriteDataAsync(clientSocket,headerResponse,response).ConfigureAwait(false);
                await SendLog(CommandConstants.GetGamesDescription,game,ResponseConstants.Ok).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.GameDetail).ConfigureAwait(false);
                await SendLog(CommandConstants.GetGamesDescription,game,ResponseConstants.Error+e.Message).ConfigureAwait(false);
            }
        }
        
        private async Task ModifyGameAsync(TcpClient clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            await communication.ReadDataAsync(clientSocket, header.IDataLength, gameBuffer).ConfigureAwait(false);

            var info = Encoding.UTF8.GetString(gameBuffer).Split('|');
            var game = info[0];
            
            try
            {
                this.gameLogic.ModifyGame(gameBuffer);

                var cover = info[5];
                
                if (!string.IsNullOrEmpty(cover))
                {
                    await communication.ReadFileAsync(clientSocket,serverFilesPath).ConfigureAwait(false);
                } 
                await OkResponse(clientSocket,CommandConstants.ModifyGame).ConfigureAwait(false);
                await SendLog(CommandConstants.ModifyGameDescription,game,ResponseConstants.Ok).ConfigureAwait(false);

            }
            catch(Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.ModifyGame).ConfigureAwait(false);
                await SendLog(CommandConstants.ModifyGameDescription,game, ResponseConstants.Error+e.Message).ConfigureAwait(false);
            }
        }
        
        private async Task ProcessGameAsync(TcpClient clientSocket, Header header)
        {
            var gameBuffer = new byte[header.IDataLength];

            await communication.ReadDataAsync(clientSocket, header.IDataLength, gameBuffer).ConfigureAwait(false);
           
            var info = Encoding.UTF8.GetString(gameBuffer).Split('|');
            var game = info[0];
            
            try
            {
                this.gameLogic.PublicGame(gameBuffer);

                var cover = info[4];
                
                if (!string.IsNullOrEmpty(cover))
                {
                   await communication.ReadFileAsync(clientSocket,serverFilesPath).ConfigureAwait(false);
                }
                
                await OkResponse(clientSocket,CommandConstants.PublicGame).ConfigureAwait(false);
                await SendLog(CommandConstants.PublicGameDescription,game,ResponseConstants.Ok).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.PublicGame).ConfigureAwait(false);
                await SendLog(CommandConstants.PublicGameDescription,game,ResponseConstants.Error+e.Message).ConfigureAwait(false);
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
                    await OkResponse(clientSocket,CommandConstants.SendImage).ConfigureAwait(false);
                    await communication.WriteFileAsync(clientSocket, cover).ConfigureAwait(false);
                    await SendLog(CommandConstants.SendImageDescription,game,ResponseConstants.Ok).ConfigureAwait(false);
                }
                else
                {
                    var message = "No existe la imagen";
                   await ErrorResponse(clientSocket,message,CommandConstants.SendImage).ConfigureAwait(false);
                   await SendLog(CommandConstants.SendImageDescription,game,ResponseConstants.Error+message).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket,e.Message,CommandConstants.SendImage).ConfigureAwait(false);
                await SendLog(CommandConstants.SendImageDescription,game,ResponseConstants.Error+e.Message).ConfigureAwait(false);
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
                await SendLog(CommandConstants.BuyGameDescription,game,ResponseConstants.Ok).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ErrorResponse(clientSocket, e.Message, CommandConstants.BuyGame);
                await SendLog(CommandConstants.BuyGameDescription,game,ResponseConstants.Error+e.Message).ConfigureAwait(false);
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
            
            await SendLog(CommandConstants.GetGamesDescription,"",ResponseConstants.Ok).ConfigureAwait(false);
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

        private async Task SendLog(string command, string game, string response)
        {
            var log = new Log();
            log.Game = game;
            log.User = userLogged.UserLogin;
            log.Action = command;
            log.Response = response;
            log.Date = DateTime.Now;
            
            await logsProducer.SendLog(log).ConfigureAwait(false);
        }
    }
}
