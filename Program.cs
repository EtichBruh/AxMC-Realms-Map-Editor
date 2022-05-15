using System;
using System.IO;

namespace AxMC_Realms_ME
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try {
                using (var game = new Game1())
                    game.Run();
            }
            catch(Exception e) { File.WriteAllText("SendThisToNekoT", e.Message); }
        }
    }
}
