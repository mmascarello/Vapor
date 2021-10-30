using CommunicationInterface;
using SettingsManagerInterface;
using System;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StringProtocol;
using ValidationsImplementations;
using VaporServer;

namespace VaporCliente.Endpoint
{
    public class Client
    {
        private readonly ICommunication communication;
        private readonly ISettingsManager manager;
        private readonly TcpClient tcpClient;
        private IPEndPoint clientIpAddress;
        private int clientPort;
        private int serverPort;
        private string filesPathRecived;
        private string filesPathToSend;
        private IPAddress clientIP;
        private IPAddress serverIP;
        private string userLogged="";

        public Client(ICommunication communication, ISettingsManager manager)
        {
            this.communication = communication;
            this.manager = manager;
            
            clientPort = int.Parse(manager.ReadSetting((ClientConfig.ClientPortConfigKey)));
            clientIP = IPAddress.Parse(manager.ReadSetting(ClientConfig.ClientIpConfigKey));
            clientIpAddress = new IPEndPoint(clientIP,clientPort);
            tcpClient = new TcpClient(clientIpAddress);
            
            serverIP = IPAddress.Parse(manager.ReadSetting(ClientConfig.ServerIpConfigKey));
            serverPort = int.Parse(manager.ReadSetting((ClientConfig.ServerPortConfigKey)));
        }

        public async Task Start()
        {
            this.filesPathRecived = manager.ReadSetting(ClientConfig.FilePathForRecive);
            this.filesPathToSend = manager.ReadSetting(ClientConfig.FilePathToSend);
            System.IO.Directory.CreateDirectory(filesPathRecived);
            System.IO.Directory.CreateDirectory(filesPathToSend);

            try
            {
                await tcpClient.ConnectAsync(serverIP, serverPort).ConfigureAwait(false);
                Console.WriteLine("Bienvenido al sistema cliente");
                Console.WriteLine("");

                Login(tcpClient);

                Help();

                await HandleClient(tcpClient).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al conectarse al servidor, intentelo mas tarde");
                Console.ReadLine();
            }
        }

        private void Login(TcpClient tcpClient)
        {
            while (userLogged.Equals(""))
            {
                Console.WriteLine("Para continuar debe autenticarse");
                Console.WriteLine("Ingrese nombre de usuario");
                var user = ValidationsImplementations.UserValidation.ValidNotEmpty();
                Console.WriteLine("Ingrese contraseña");
                var password = ValidationsImplementations.UserValidation.ValidNotEmpty();
                Authenticate(tcpClient,user,password);
            }
        }
        
        private async Task HandleClient(TcpClient tcpClient)
        {
            var connected = true;

            while (connected)
            {   
                Console.WriteLine("");
                Console.WriteLine("Esperando comando... (help para ver menu)");
                var opcion = Console.ReadLine();
                
                switch (opcion)
                {
                    case "exit":
                        tcpClient.GetStream().Close();
                        tcpClient.Close();
                        connected = false;
                        break;
                    
                    case "obtener caratula":
                        
                        await GetCoverPageAsync(tcpClient,String.Empty).ConfigureAwait(false);

                        break;
                    
                    case "obtener juegos":
                        await GetGamesAsync(tcpClient).ConfigureAwait(false);
                        break;
                    
                    case "adquirir juego":
                        await BuyGameAsync(tcpClient).ConfigureAwait(false);
                        break;
                    
                    case "publicar juego":
                        await PublicGameAsync(tcpClient).ConfigureAwait(false);
                        break;
                    
                    case "modificar juego":
                        
                        await ModifyGameAsync(tcpClient).ConfigureAwait(false);

                        break;
                    
                    case "ver detalle juego":
                        await GetGameDetailsAsync(tcpClient).ConfigureAwait(false);
                        break;
                    
                    case "borrar juego":
                        await DeleteGameAsync(tcpClient).ConfigureAwait(false);
                        break;
                    
                    case "buscar juegos":
                        await LookUpForGameAsync(tcpClient).ConfigureAwait(false);
                        break;
                    
                    case "publicar calificacion":
                        await PublicCalificationAsync(tcpClient).ConfigureAwait(false);
                        break;
                    
                    case "help":
                        Help();
                        break;
                    
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }

        private void Help()
        {
            Console.WriteLine("Los posibles comandos son:\n");
            Console.WriteLine(" 'obtener juegos' ");
            Console.WriteLine(" 'adquirir juego' ");
            Console.WriteLine(" 'publicar juego' ");
            Console.WriteLine(" 'modificar juego' ");
            Console.WriteLine(" 'ver detalle juego' ");
            Console.WriteLine(" 'borrar juego'  ");
            Console.WriteLine(" 'buscar juegos' ");
            Console.WriteLine(" 'obtener caractula'");
            Console.WriteLine(" 'publicar calificacion'");
            Console.WriteLine(" 'exit' (si desea desconectarse del servidor)\n");
            Console.WriteLine("Ingrese su opcion  de preferencia.");
            Console.WriteLine("Recomendacion: simepre tenga desactivado las mayusculas");
        }

        private async Task Authenticate(TcpClient tcpClient, string user, string password)
        {
            var data = user+"|"+password+"|";
            var header = new Header(HeaderConstants.Request, CommandConstants.Login ,data.Length);
            await communication.WriteDataAsync(tcpClient, header, data).ConfigureAwait(false);

            var response = await GetResponse(tcpClient);
            if (response.Equals(ResponseConstants.Ok))
            {
                userLogged = user;
            }
            else
            {
                Console.WriteLine(response); 
            }
            
        }
        
        private async Task PublicCalificationAsync(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese un juego de los siguientes:");
            await  GetGamesAsync(tcpClient).ConfigureAwait(false);
            var game = ValidationsImplementations.GameValidation.ValidNotEmpty();
            
            Console.WriteLine("Ingrese un rating entre 1 al 5");
            var rating = ValidationsImplementations.GameValidation.ValidateRating();
            
            Console.WriteLine("Ingrese una review (opinion del juego)");
            var review = ValidationsImplementations.GameValidation.ValidNotEmpty();
            
            var data = game+"|"+rating+"|"+review+"|";
            
            var header = new Header(HeaderConstants.Request, CommandConstants.PublicCalification ,data.Length);
            
            await communication.WriteDataAsync(tcpClient, header, data).ConfigureAwait(false);
            
            try
            {
                var response = await GetResponse(tcpClient).ConfigureAwait(false);
                Console.WriteLine(response);
            }
            catch (Exception)
            {
                Console.WriteLine("Error intentelo nuevamente");
            }

        }
        
        private async Task DeleteGameAsync(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese el título del juego a borrar");
            var game = GameValidation.ValidNotEmpty();
           
            var header = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, game.Length);

            await communication.WriteDataAsync(tcpClient, header, game).ConfigureAwait(false);

            var response = await GetResponse(tcpClient).ConfigureAwait(false);
            
            Console.WriteLine(response);
            
        }
        
        private async Task LookUpForGameAsync(TcpClient tcpClient)
        {
            Console.WriteLine("Ingresar atributo a buscar: titulo / genero / clasificacion");
            var lookupAtribute = GameValidation.ValidateValue() ;
            
            Console.WriteLine($"Ingrese {lookupAtribute}:");

            var lookupValue = GameValidation.ValidNotEmpty();

            var lookup = lookupAtribute + "|" + lookupValue + "|";
            
            var header = new Header(HeaderConstants.Request, CommandConstants.LookupGame, lookup.Length);
            
            await communication.WriteDataAsync(tcpClient, header, lookup).ConfigureAwait(false);

            var gameTitle = await GetResponse(tcpClient).ConfigureAwait(false);
            
            if(gameTitle.Contains(ResponseConstants.Error))
            {
                Console.WriteLine(gameTitle);
            }
            else
            {
                Console.WriteLine($"Se encontro el juego:{gameTitle}");
            }
        }

        private async Task GetGameDetailsAsync(TcpClient tcpClient)
        {
            Console.WriteLine("La lista de Juegos disponibles es:");
            await GetGamesAsync(tcpClient).ConfigureAwait(false);

            Console.WriteLine("Ingrese el nombre de un Juego:");
            var gameName = GameValidation.ValidNotEmpty();
            
            var header = new Header(HeaderConstants.Request, CommandConstants.GameDetail, gameName.Length);
            
            await communication.WriteDataAsync(tcpClient, header, gameName).ConfigureAwait(false);

            try
            {
                var response = await GetResponse(tcpClient).ConfigureAwait(false);
                
                if(!response.Contains(ResponseConstants.Error)){

                    var getInformation = response.Split('|');
                    var ratingAverage = getInformation[0];
                    var gameReviews = getInformation[1];
                    var gameGender = getInformation[2];
                    var gameSinopsis = getInformation[3];
                    var gameESRB = getInformation[4];

                    Console.WriteLine("Mi rating es: "+ratingAverage);
                    Console.WriteLine("Mis reviews: "+gameReviews);
                    Console.WriteLine("Genero: "+gameGender);
                    Console.WriteLine("Sinopsis: "+gameSinopsis);
                    Console.WriteLine("Esrb edad permitida : "+gameESRB);
                    
                    Console.WriteLine("");
                    Console.WriteLine("Desea descargar la imagen?: si/no");
                    var answer = GameValidation.YesNoValidation();
                    if (answer.Equals("si"))
                    {
                        await GetCoverPageAsync(tcpClient,gameName).ConfigureAwait(false);
                    }
                }
                else
                {
                    Console.WriteLine("El titulo ingresado no es correcto");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        
        private async Task ModifyGameAsync(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese el titulo del juego a modificar");
            var gameToModify = GameValidation.ValidNotEmpty();
            
            Console.WriteLine("Si no desea modificar la informacion del campo, dejelo vacio y presione enter");
            
            Console.WriteLine("Ingrese un titulo");
            var title = Console.ReadLine();

            Console.WriteLine("Ingrese un genero:");
            var gender = Console.ReadLine();

            Console.WriteLine("Ingrese una calificacion del 0 a 5");
            var esbr = GameValidation.ModifyESRB();
            
            Console.WriteLine("Haga una breve descripcion del juego");
            var sinopsis = Console.ReadLine();

            Console.WriteLine("Ingrese una caratula");
            var coverPage = ImageValidation.ValidCover(filesPathToSend);
            
            var publicGame = gameToModify +  "|" + title + "|" + gender + "|" + esbr + "|" + sinopsis + "|" + coverPage +'|';
            
            var header = new Header(HeaderConstants.Request, CommandConstants.ModifyGame, publicGame.Length);

            await communication.WriteDataAsync(tcpClient, header, publicGame).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(coverPage))
            {
                var fileToSend = filesPathToSend + coverPage;
                await communication.WriteFileAsync(tcpClient, fileToSend).ConfigureAwait(false);
            }

            var response = await GetResponse(tcpClient).ConfigureAwait(false);
            
            Console.WriteLine(response);
        }
        
        private async Task PublicGameAsync(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese un titulo");
            var title = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese un genero:");
            var gender = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese una calificacion del 0 al 5");
            var esbr = GameValidation.ValidESBR();
            
            Console.WriteLine("Haga una breve descripcion del juego");
            var sinopsis = GameValidation.ValidNotEmpty();

            Console.WriteLine("Ingrese una caratula, en caso de no querer agregar una, presione enter");
            var coverPage = ImageValidation.ValidCover(filesPathToSend);

            var publicGame = title + "|" + gender + "|" + esbr + "|" + sinopsis + "|" + coverPage +'|';
            
            var header = new Header(HeaderConstants.Request, CommandConstants.PublicGame, publicGame.Length);

            await communication.WriteDataAsync(tcpClient, header, publicGame).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(coverPage))
            {
                var fileToSend = filesPathToSend + coverPage;
                
                await communication.WriteFileAsync(tcpClient, fileToSend).ConfigureAwait(false);
            }
            
            var response = await GetResponse(tcpClient);
            
            Console.WriteLine(response);
            
        }

        private async Task BuyGameAsync(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese un usuario");
            var userName = GameValidation.ValidNotEmpty();
            
            Console.WriteLine("Ingrese un juego");
            var gameName = GameValidation.ValidNotEmpty();
            var userAndGame = userName + "|" + gameName + "|";

            var header = new Header(HeaderConstants.Request, CommandConstants.BuyGame ,userAndGame.Length);
            
            await communication.WriteDataAsync(tcpClient, header, userAndGame).ConfigureAwait(false);
           
            try
            {
                var response = await GetResponse(tcpClient);
                Console.WriteLine(response);
            }
            catch (Exception)
            {
                Console.WriteLine("Error intentelo nuevamente");
            }
        }
        

        private async Task GetGamesAsync(TcpClient tcpClient)
        {
            var headerToSend = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);

            await communication.WriteDataAsync(tcpClient, headerToSend, String.Empty).ConfigureAwait(false);
            
            var respuesta = await GetResponse(tcpClient);
            Console.WriteLine(respuesta);
        }

        private async Task GetCoverPageAsync(TcpClient tcpClient, string gameName)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                Console.WriteLine("Ingrese el nombre del juego:");
                gameName = GameValidation.ValidNotEmpty();
            }; 
            
            var headerToSendImg = new Header(HeaderConstants.Request, CommandConstants.SendImage, gameName.Length);

            await communication.WriteDataAsync(tcpClient, headerToSendImg, gameName).ConfigureAwait(false);
            
            var response = await GetResponse(tcpClient);
            
            if (response.Equals(ResponseConstants.Ok))
            {
                try
                {
                    await  communication.ReadFileAsync(tcpClient,filesPathRecived).ConfigureAwait(false);
                    Console.WriteLine("Imagen recibida");
                }
                catch (Exception)
                {
                    Console.WriteLine("Error intentelo nuevamente");
                }
            }
            else
            {
                Console.WriteLine(response);
            }
           
        }

        private async Task<string> GetResponse(TcpClient tcpClient)
        {
            var newHeaderLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                                  HeaderConstants.DataLength;

            var newBuffer = new Byte[newHeaderLength];
            await communication.ReadDataAsync(tcpClient, newHeaderLength, newBuffer).ConfigureAwait(false);
            
            var newHeader = new Header();
            newHeader.DecodeData(newBuffer);

            var newBufferData = new byte[newHeader.IDataLength];
            await communication.ReadDataAsync(tcpClient, newHeader.IDataLength, newBufferData).ConfigureAwait(false);
 
            var message = Encoding.UTF8.GetString(newBufferData);
 
            return message;
        }
    }
}