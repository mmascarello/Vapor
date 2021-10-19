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
        private readonly TcpClient tcpClient;
        private IPEndPoint clientIpAddress;
        private int clientPort;
        private IPEndPoint severIpAddress;
        private int serverPort;
        private string filesPathRecived;
        private string filesPathToSend;
        private string clientIP;
        private string serverIP;

        public Client(ICommunication communication, ISettingsManager manager)
        {
            this.communication = communication;
            this.manager = manager;
            clientPort = int.Parse(manager.ReadSetting((ClientConfig.ClientPortConfigKey)));
            //clientIP = manager.ReadSetting(ClientConfig.ClientIpConfigKey);
            clientIpAddress = new IPEndPoint(IPAddress.Loopback,clientPort);
            tcpClient = new TcpClient(clientIpAddress);
            //serverIP = manager.ReadSetting((ClientConfig.ServerIpConfigKey));
            serverPort = int.Parse(manager.ReadSetting((ClientConfig.ServerPortConfigKey)));
            severIpAddress = new IPEndPoint(IPAddress.Loopback, serverPort);
        }

        public void Start()
        {
            /*serverPort = int.Parse(manager.ReadSetting((ClientConfig.ServerPortConfigKey)));*/
            this.filesPathRecived = manager.ReadSetting(ClientConfig.FilePathForRecive);
            this.filesPathToSend = manager.ReadSetting(ClientConfig.FilePathToSend);
            System.IO.Directory.CreateDirectory(filesPathRecived);
            System.IO.Directory.CreateDirectory(filesPathToSend);

            try
            {
                tcpClient.Connect(severIpAddress);
                Console.WriteLine("Bienvenido al sistema cliente");
                Console.WriteLine("");

                Help();

                HandleClient(tcpClient);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al conectarse al servidor, intentelo mas tarde");
                Console.ReadLine();
            }
        }

        private void HandleClient(TcpClient tcpClient)
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
                        
                        GetCoverPage(tcpClient,String.Empty);

                        break;
                    
                    case "obtener juegos":
                        GetGames(tcpClient);
                        break;
                    
                    case "adquirir juego":
                        BuyGame(tcpClient);
                        break;
                    
                    case "publicar juego":
                        PublicGame(tcpClient);
                        break;
                    
                    case "modificar juego":
                        
                        ModifyGame(tcpClient);

                        break;
                    
                    case "ver detalle juego":
                        GetGameDetails(tcpClient);
                        break;
                    
                    case "borrar juego":
                        DeleteGame(tcpClient);
                        break;
                    
                    case "buscar juegos":
                        LookUpForGame(tcpClient);
                        break;
                    
                    case "publicar calificacion":
                        PublicCalification(tcpClient);
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

        private void PublicCalification(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese un juego de los siguientes:");
            GetGames(tcpClient);
            var game = ValidationsImplementations.GameValidation.ValidNotEmpty();
            
            Console.WriteLine("Ingrese un rating entre 1 al 5");
            var rating = ValidationsImplementations.GameValidation.ValidateRating();
            
            Console.WriteLine("Ingrese una review (opinion del juego)");
            var review = ValidationsImplementations.GameValidation.ValidNotEmpty();
            
            var data = game+"|"+rating+"|"+review+"|";
            
            var header = new Header(HeaderConstants.Request, CommandConstants.PublicCalification ,data.Length);
            
            communication.WriteDataAsync(tcpClient, header, data);
            
            try
            {
                var response = GetResponse(tcpClient);
                Console.WriteLine(response);
            }
            catch (Exception)
            {
                Console.WriteLine("Error intentelo nuevamente");
            }

        }
        
        private void DeleteGame(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese el título del juego a borrar");
            var game = GameValidation.ValidNotEmpty();
           
            var header = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, game.Length);

            communication.WriteDataAsync(tcpClient, header, game);
            
            var response = GetResponse(tcpClient);
            
            Console.WriteLine(response);
            
        }
        
        private void LookUpForGame(TcpClient tcpClient)
        {
            Console.WriteLine("Ingresar atributo a buscar: titulo / genero / clasificacion");
            var lookupAtribute = GameValidation.ValidateValue() ;
            
            Console.WriteLine($"Ingrese {lookupAtribute}:");

            var lookupValue = GameValidation.ValidNotEmpty();

            var lookup = lookupAtribute + "|" + lookupValue + "|";
            
            var header = new Header(HeaderConstants.Request, CommandConstants.LookupGame, lookup.Length);
            
            communication.WriteDataAsync(tcpClient, header, lookup);

            var gameTitle = GetResponse(tcpClient);
            if (gameTitle.Contains(ResponseConstants.Error))
            {
                Console.WriteLine(gameTitle);
            }
            else
            {
                Console.WriteLine($"Se encontro el juego:{gameTitle}");
            }
        }

        private void GetGameDetails(TcpClient tcpClient)
        {
            Console.WriteLine("La lista de Juegos disponibles es:");
            GetGames(tcpClient);

            Console.WriteLine("Ingrese el nombre de un Juego:");
            var gameName = GameValidation.ValidNotEmpty();
            
            var header = new Header(HeaderConstants.Request, CommandConstants.GameDetail, gameName.Length);
            
            communication.WriteDataAsync(tcpClient, header, gameName);

            try
            {

                var response = GetResponse(tcpClient);
                
                if(!response.Contains(ResponseConstants.Error)){

                    var getInformation = response.Split('|');
                    var ratingAverage = getInformation[0];
                    var gameReviews = getInformation[1];
                    var gameGender = getInformation[2];
                    var gameSinopsis = getInformation[3];
                    var gameESRB = getInformation[4];

                    Console.WriteLine("Mi rating es: "+ratingAverage);
                    Console.WriteLine("Mis reviews: "+gameReviews);
                    Console.WriteLine("Soy un juego de: "+gameGender);
                    Console.WriteLine("Sinopsis: "+gameSinopsis);
                    Console.WriteLine("Esrb edad permitida : "+gameESRB);
                    
                    Console.WriteLine("");
                    Console.WriteLine("Desea descargar la imagen?: si/no");
                    var answer = GameValidation.YesNoValidation();
                    if (answer.Equals("si"))
                    {
                        GetCoverPage(tcpClient,gameName);
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
        
        
        private void ModifyGame(TcpClient tcpClient)
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

            communication.WriteDataAsync(tcpClient, header, publicGame);

            if (!string.IsNullOrEmpty(coverPage))
            {
                var fileToSend = filesPathToSend + coverPage;
                communication.WriteFile(tcpClient, fileToSend);
            }
            
            var response = GetResponse(tcpClient);
            
            Console.WriteLine(response);
        }
        
        private void PublicGame(TcpClient tcpClient)
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

            communication.WriteDataAsync(tcpClient, header, publicGame);

            if (!string.IsNullOrEmpty(coverPage))
            {
                var fileToSend = filesPathToSend + coverPage;
                
                communication.WriteFile(tcpClient, fileToSend);
            }
            
            var response = GetResponse(tcpClient);
            
            Console.WriteLine(response);
            
        }

        private void BuyGame(TcpClient tcpClient)
        {
            Console.WriteLine("Ingrese un usuario");
            var userName = GameValidation.ValidNotEmpty();
            
            Console.WriteLine("Ingrese un juego");
            var gameName = GameValidation.ValidNotEmpty();
            var userAndGame = userName + "|" + gameName + "|";

            var header = new Header(HeaderConstants.Request, CommandConstants.BuyGame ,userAndGame.Length);
            
            communication.WriteDataAsync(tcpClient, header, userAndGame);
           
            try
            {
                var response = GetResponse(tcpClient);
                Console.WriteLine(response);
            }
            catch (Exception)
            {
                Console.WriteLine("Error intentelo nuevamente");
            }
        }
        

        private void GetGames(TcpClient tcpClient)
        {
            var headerToSend = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);

            communication.WriteDataAsync(tcpClient, headerToSend, String.Empty);

            var respuesta = GetResponse(tcpClient);
            Console.WriteLine(respuesta);
            
        }

        private void GetCoverPage(TcpClient tcpClient, string gameName)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                Console.WriteLine("Ingrese el nombre del juego:");
                gameName = GameValidation.ValidNotEmpty();
            }; 
            
            var headerToSendImg = new Header(HeaderConstants.Request, CommandConstants.SendImage, gameName.Length);

            communication.WriteDataAsync(tcpClient, headerToSendImg, gameName); 
            
            var response = GetResponse(tcpClient);
            if (response.Equals(ResponseConstants.Ok))
            {
                try
                {
                    communication.ReadFileAsync(tcpClient,filesPathRecived);
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

        private string GetResponse(TcpClient tcpClient)
        {
            var newHeaderLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                                  HeaderConstants.DataLength;

            var newBuffer = new Byte[newHeaderLength];
            communication.ReadDataAsync(tcpClient, newHeaderLength, newBuffer);

            var newHeader = new Header();
            newHeader.DecodeData(newBuffer);

            var newBufferData = new byte[newHeader.IDataLength];
            communication.ReadDataAsync(tcpClient, newHeader.IDataLength, newBufferData);

            var message = Encoding.UTF8.GetString(newBufferData);
            return message;
        }
    }
}