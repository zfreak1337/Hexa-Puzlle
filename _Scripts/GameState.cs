public class GameState
{
    public static int chosenWorld = 1;
    public static int chosenLevel = 1;
    public static bool canPlay = true;

    public static StoredValue<int> hint = new StoredValue<int>("hint", 5);
}