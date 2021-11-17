using System;
using AdministratorWebApi.GrpcClient;
using Grpc.Net.Client;

namespace AdministratorWebApi.Connection
{
    public class GrpcConnection
    {
        private static GrpcConnection Instance = null;
        private static readonly object ObjectLock = new object();
        private Greeter.GreeterClient Client;
        private GrpcConnection()
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost:6001");
            Client = new Greeter.GreeterClient(channel);
        }

        public Greeter.GreeterClient GetClient()
        {
            return Client;
        }
        
        public static GrpcConnection GetGrpcConnectionInstance()
        {
            if (GrpcConnection.Instance == null)
            {
                lock (GrpcConnection.ObjectLock)
                {
                    if (GrpcConnection.Instance == null)
                    {
                        GrpcConnection.Instance = new GrpcConnection();
                    }
                }
            }

            return GrpcConnection.Instance;
        }
    }
}