using System.Net.Sockets;
using StringProtocol;


namespace CommunicationInterface
{
    public interface ICommunication
    { 
        void ReadData(TcpClient tcpClient, int length, byte[] buffer);
        void WriteData(TcpClient tcpClientt, Header header, string data);
        void WriteFile(TcpClient tcpClient, string path);
        void ReadFile(TcpClient tcpClient, string path);
    }
}