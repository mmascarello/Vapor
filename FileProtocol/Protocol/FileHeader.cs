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
                new byte[GetLength()]; // Creo un array de bytes de largo Specification.FixedFileNameLength + Specification.FixedFileSizeLength;
            var fileNameData =
                BitConverter.GetBytes(Encoding.UTF8.GetBytes(fileName).Length); // Obtengo largo del nombre
            
            if (fileNameData.Length != Specification.FixedFileNameLength)
                throw new Exception("There is something wrong with the file name");
            
            var fileSizeData = BitConverter.GetBytes(fileSize); // Obtengo tamaño del archivo en array de bytes

            Array.Copy(fileNameData, 0,
                header, 0, Specification.FixedFileNameLength); // Copio al array destino XXXX a partir de la posicion 0
            Array.Copy(fileSizeData, 0, header,
                Specification.FixedFileNameLength, Specification.FixedFileSizeLength); // Copio al array de destino YYYYYYYY a partir de la posicion 4

            return header;
        }
        
        
    }
}