using System.Net.Sockets;
using StringProtocol;

namespace CommunicationInterface
{
    public interface ICommunication
    {
        static void ReceiveData(Socket clientSocket, int length, byte[] buffer);
       static void SendData(Socket ourSocket, Header header, string data);
    }
}