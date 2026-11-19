public static class GameProgress
{
    public static int Coins { get; private set; }
    public static int LevelIndex { get; set; }

    public static void AddCoins(int amount) => Coins += amount;

    public static void Apply(SaveData data)
    {
        Coins = data.coins;
        LevelIndex = data.levelIndex;
    }

    public static SaveData ToData() => new SaveData
    {
        coins = Coins,
        levelIndex = LevelIndex,
    };

    public static void Reset()
    {
        Coins = 0;
        LevelIndex = 0;
    }
}