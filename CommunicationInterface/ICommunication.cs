using System.Net.Sockets;
using StringProtocol;

namespace CommunicationInterface
{
    public interface ICommunication
    {
        void ReceiveData(Socket clientSocket, int length, byte[] buffer);
        void SendData(Socket ourSocket, Header header, string data);
    }
}