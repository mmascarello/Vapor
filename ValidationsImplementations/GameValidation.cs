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
        
        public static string YesNoValidation()
        {
            var isOk = false;
            var input = "";
            
            while(!isOk)
            {
                input = Console.ReadLine();
                if (input == String.Empty && !input.Equals("si") && !input.Equals("no"))
                {
                    Console.WriteLine("El campo debe ser si o no");
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
            var message ="Ingrese un numero entre 0 y 5 para darle permisos a: \n" +
                              "todos\n" + "mayores a 10\n" + "adolecentes\n" + "+ 17\n" + "+ 18\n " + "Pending";
            
            while(!isOk)
            {
                input = Console.ReadLine();
                try
                {
                    var num = Convert.ToInt32(input);
                    if (num < 0 || num > 5)
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
            var message ="Ingrese un numero entre 0 y 5 para darle permisos a: \n" +
                         "todos\n" + "mayores a 10\n" + "adolecentes\n" + "+ 17\n" + "+ 18\n " + "Pending";
            
            while(!isOk)
            {
                input = Console.ReadLine();
                try
                {
                    if (!String.IsNullOrEmpty(input))
                    {
                        var num = Convert.ToInt32(input);
                        if (num < 0 || num > 5)
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