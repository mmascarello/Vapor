using System;
using Grpc.Net.Client;

namespace AdministrationServerWebApi.GrpcClient
{
    public class GrpcConnection
    {
        private static GrpcConnection Instance = null;
        private static readonly object ObjectLock = new object();

        private GrpcConnection()
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Greeter.GreeterClient(channel);
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