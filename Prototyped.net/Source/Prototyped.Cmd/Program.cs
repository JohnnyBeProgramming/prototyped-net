using Prototyped.Data;
using Prototyped.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Cmd
{
    public class Program
    {
        /// <summary>
        /// The main Entry point for the console application
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Remember current color
            var oldCol = Console.ForegroundColor;
            try
            {
                Interop.Start(args);
            }
            catch (Exception ex)
            {
                // Something went wrong
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ReadKey();
                throw;
            }
            finally
            {
                // Reset to old color
                Console.ForegroundColor = oldCol;
            }
        }
    }
}
