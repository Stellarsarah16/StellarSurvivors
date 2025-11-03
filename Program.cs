using  StellarSurvivors.Core;

namespace StellarSurvivors
{
// We'll put everything in a class. This is the standard C# way.
    class Program
    {
        public static void Main()
        {

            Game gameInstance = new Game(1600, 600);

            gameInstance.Run();
        }
    }
}