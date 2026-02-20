namespace Janthus.Game;

public static class Program
{
    [STAThread]
    static void Main()
    {
        using var game = new JanthusGame();
        game.Run();
    }
}
