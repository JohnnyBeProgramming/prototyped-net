using Newtonsoft.Json;
using Prototyped.NetEdge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Cmd.EdgeJS
{
    class Program
    {
        static void Main(string[] args)
        {
            var col = Console.ForegroundColor;
            try
            {
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.WriteLine(" - EdgeJS and Microsoft.net ");
                Console.WriteLine("-------------------------------------------------------------------------------");

                // Define some test data
                dynamic data = new
                {
                    ident = "C#.NET",
                    start = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                };

                // Create an EdgeJS connection to node
                using (var edge = AppEdge.Create())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" - [ C# ] EdgeJS is now active!");
                    Console.ForegroundColor = col;

                    Console.WriteLine(" - [ C# ] NodeJS <= \r\n" + JsonConvert.SerializeObject(data, Formatting.Indented));
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    
                    // Call the async task and wait for it to complete                    
                    var task = edge.Ping(data);
                    task.Wait();


                    Console.ForegroundColor = col;
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.WriteLine(" - [ C# ] Result => \r\n" + JsonConvert.SerializeObject(task.Result, Formatting.Indented));
                }

                Console.WriteLine("-------------------------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.Red;
                var i = 0;
                while (ex != null)
                {
                    Console.WriteLine(" - [ " + i + " ] Error: " + ex.Message);
                    ex = ex.InnerException;
                    i++;
                }
                Console.ForegroundColor = col;
                Console.WriteLine("-------------------------------------------------------------------------------");
            }
            finally
            {
                Console.ForegroundColor = col;
            }
            Console.ReadKey();
        }
    }
}
