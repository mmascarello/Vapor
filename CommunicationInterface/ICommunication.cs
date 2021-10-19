using System.Net.Sockets;
using System.Threading.Tasks;
using StringProtocol;


namespace CommunicationInterface
{
    public interface ICommunication
    { 
        Task ReadDataAsync(TcpClient tcpClient, int length, byte[] buffer);
        Task WriteDataAsync(TcpClient tcpClient, Header header, string data);
        Task WriteFile(TcpClient tcpClient, string path);
        Task ReadFileAsync(TcpClient tcpClient, string path);
    }
}