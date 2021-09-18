using System.Net.Sockets;
using StringProtocol;

namespace VaporServer.Endpoint.EndpointFactory
{
    public interface CommandInterface
    {
        public void Send(Socket ourSocket);
        public void Receive(Socket clientSocket,Header header);
    }
}