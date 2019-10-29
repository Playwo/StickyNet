using System;
using System.Runtime.InteropServices;

namespace StickyNet
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This application does not support OSX!");
                Console.ReadKey();
                return;
            }


            var startup = new Startup();
            startup.Run(args);
        }
    }
}
