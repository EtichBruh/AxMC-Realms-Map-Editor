using System;
using System.IO;

namespace AxMC_Realms_ME
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.BufferWidth = 400;
            Console.WindowWidth = 40;
                using (var game = new Game1())
                    game.Run();
        }
    }
}
