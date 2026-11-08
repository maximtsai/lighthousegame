/// <summary>
/// Day 5 state shared between scenes. She's in Camborne's bed with worms coming out of her legs:
/// tending to her comes first (MermaidScene's Day 5 treatment), then the day is spent on her
/// instead of the lighthouse. The work task gets swapped for picking her flowers the first time he
/// steps outside, the fish are collected for her, and the lighthouse door stays shut all day.
/// </summary>
public static class Day5
{
    public const int Day = 5;
    // Set once "Do Your Job" has been crossed off for "Pick Flowers For Her"
    public const string WorkReplacedKey = "day5_work_replaced";
    // Set (and written into the save) once the dream's scare has played, so Continue doesn't replay it
    public const string DreamSeenKey = "day5_dream_seen";

    public const string LighthouseLine = "She's waiting for me.";
    public const string TendHerFirstLine = "She needs tending first.";

    public static bool IsToday => GameState.Get<int>("day") == Day;
    public static bool Treated => IsToday && GameState.Get<bool>(Day4Beach.MermaidTreatedKey, false);
    public static bool FlowersPicked => GameState.Get<bool>(FlowerMinigame.CompleteKey, false);
}
