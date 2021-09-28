using System;
using System.IO;

namespace ValidationsImplementations
{
    public static class ImageValidation
    {
        public static string ValidCover(string path)
        {
            var isOk = false;
            var input = "";
            var message = "No existe la imagen, verifique que el nombre del archivo este todo en minuscula";
            
            while(!isOk)
            {
                input = Console.ReadLine();
                input = input.ToLower();
                if (!String.IsNullOrEmpty(input))
                {
                    
                    if (!File.Exists(path+input))
                    {
                        Console.WriteLine(message);
                    }
                    else
                    {
                        isOk = true;
                    }
                }
                else
                {
                    isOk = true;
                }
            }
            return input;
        }
        
        
    }
}