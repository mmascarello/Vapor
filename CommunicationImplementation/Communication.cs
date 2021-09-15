using System;
using System.Net.Sockets;
using System.Text;
using CommunicationInterface;
using StringProtocol;

namespace CommunicationImplementation
{
    public class Communication : ICommunication
    {
        public void ReceiveData(Socket clientSocket,  int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var localRecv = clientSocket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
                    if (localRecv == 0)
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        return;
                    }

                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
                
            }
        }
        
        public void SendData(Socket ourSocket, Header header, string data)
        {
            var dataToSend = header.BuildRequest();
                        
            var sentBytes = 0;
            while (sentBytes < dataToSend.Length)
            {
                sentBytes += ourSocket.Send(dataToSend, sentBytes, dataToSend.Length - sentBytes, SocketFlags.None);
            }

            sentBytes = 0;
                        
            var bytesMessage = Encoding.UTF8.GetBytes(data);
            while (sentBytes < bytesMessage.Length)
            {
                sentBytes += ourSocket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                    SocketFlags.None);
            }
        }
    }
}