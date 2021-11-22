using System;
using System.Text;

namespace FileProtocol.Protocol
{
    public class FileHeader
    {

        public static int GetLength()
        {
            return Specification.FixedFileNameLength + Specification.FixedFileSizeLength;
        }

        public bool Decode(byte[] buffer)
        {
            
            
            return true;
        }
        
        public byte[] Create(string fileName, long fileSize)
        {
            var header =
                new byte[GetLength()]; 
            var fileNameData =
                BitConverter.GetBytes(Encoding.UTF8.GetBytes(fileName).Length); 
            
            if (fileNameData.Length != Specification.FixedFileNameLength)
                throw new Exception("There is something wrong with the file name");
            
            var fileSizeData = BitConverter.GetBytes(fileSize); 

            Array.Copy(fileNameData, 0,
                header, 0, Specification.FixedFileNameLength);
            Array.Copy(fileSizeData, 0, header,
                Specification.FixedFileNameLength, Specification.FixedFileSizeLength); 

            return header;
        }
        
        
    }
}