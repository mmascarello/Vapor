using System;

namespace ValidationsImplementations
{
    public static class GameValidation
    {
        public static string ValidNotEmpty()
        {
            var isOk = false;
            var input = "";
            
            while(!isOk)
            {
                input = Console.ReadLine();
                if (input == String.Empty)
                {
                    Console.WriteLine("El campo no puede ser vacio");
                }
                else
                {
                    isOk = true;
                }
            }

            return input.ToLower();
        }
        
        public static string ValidCalification()
        {
            var isOk = false;
            var input = "";
            var message = "El campo debe ser un numero entre 1 y 5 entero";
            
            while(!isOk)
            {
                input = Console.ReadLine();
                try
                {
                    var num = Convert.ToInt32(input);
                    if (num <1 || num > 5 )
                    {
                        Console.WriteLine(message);
                    }
                    else
                    {
                        isOk = true;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(message);
                }
                
            }

            return input.ToLower();
        }
        
        public static string ModifyCalification()
        {
            var isOk = false;
            var input = "";
            var message = "Si ingresa un número debe ser entero entre 1 y 5 ";
            
            while(!isOk)
            {
                input = Console.ReadLine();
                try
                {
                    if (!String.IsNullOrEmpty(input))
                    {
                        var num = Convert.ToInt32(input);
                        if (num < 1 || num > 5)
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
                catch (Exception e)
                {
                    Console.WriteLine(message);
                }
                
            }

            return input.ToLower();
        }
        
        
    }
}