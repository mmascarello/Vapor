using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StringProtocol;

namespace VaporClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            socket.Connect("127.0.0.1", 20000);
            
            var connected = true;
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("message -> envia un mensaje al server");
            Console.WriteLine("login -> autentica el usuario en el sistema");
            Console.WriteLine("user-list -> obtiene el listado de usuarios en el sistema");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
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
                    case "message":
                        Console.WriteLine("Ingrese el mensaje a enviar:");
                        var mensaje = Console.ReadLine();
                        send(mensaje,CommandConstants.Message , socket);

                        break;
                    
                    case "login":
                        Console.WriteLine("Login:");
                        var usuario = Console.ReadLine();
                        send(usuario, CommandConstants.Login, socket);
                        break;
                    
                    case "user-list":
                        
                        break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }
        
        private static void send(string data, int command, Socket socket)
        {
            var headerLogin = new Header(HeaderConstants.Request, command, data.Length);
            var dataLogin = headerLogin.GetRequest();
                        
            var sentBytesLogin = 0;
            while (sentBytesLogin < dataLogin.Length)
            {
                sentBytesLogin += socket.Send(dataLogin, sentBytesLogin, dataLogin.Length - sentBytesLogin, SocketFlags.None);
            }

            sentBytesLogin = 0;
                        
            var bytesMessageLogin = Encoding.UTF8.GetBytes(data);
            while (sentBytesLogin < bytesMessageLogin.Length)
            {
                sentBytesLogin += socket.Send(bytesMessageLogin, sentBytesLogin, bytesMessageLogin.Length - sentBytesLogin,
                    SocketFlags.None);
            }
        }
        
        
    }
}