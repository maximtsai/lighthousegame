using UnityEngine;

/// <summary>
/// Day 5 state shared between scenes. She's in Camborne's bed with worms coming out of her legs:
/// tending to her comes first (MermaidScene's Day 5 treatment), then the day is spent on her
/// instead of the lighthouse. The work task gets swapped for picking her flowers the first time he
/// steps outside, the fish are collected for her, and the lighthouse door stays shut all day.
/// </summary>
public static class Day5
{
    public const int Day = 5;
    // Day 6 goes the same way (with Day 6's own treatment in MermaidScene): her first, flowers
    // instead of the lighthouse, fish for her. The "care day" members below cover both days.
    public const int LastCareDay = 6;
    // Set once "Do Your Job" has been crossed off for "Pick Flowers For Her" (one key per day)
    public static string WorkReplacedKey => $"day{GameState.Get<int>("day")}_work_replaced";
    // Set (and written into the save) once the dream's scare has played, so Continue doesn't replay it
    public const string DreamSeenKey = "day5_dream_seen";

    public const string TendHerFirstLine = "She needs tending first.";
    // The fish net before she's been tended on Days 5 and 6
    public const string HerFirstLine = "Her first.";

    // The day the skin fish was last set aside for her at the fish net. The next morning's
    // treatment uses it (MiscObjectClick.ClickPartnerBed says so on the first click).
    public const string FishSetAsideKey = "fish_set_aside_day";
    // Bedroom exits are shut until she's tended; the first try gets its own line
    public const string LeaveBlockedKey = "day5_tried_to_leave";
    // FlowerMinigame.KeptKey as it was on Day 5: how many flowers he kept for her. The field has 10
    // (7 healthy, 3 with bugs), and Day 6's treatment has 10 spots to put them, one each.
    public const string FlowersKeptKey = "flowers_day5_kept";
    // Day 6: none of Day 5's flowers were kept, so nothing went on her
    public static bool NoFlowersOnHer =>
        GameState.Get<int>("day") == 6 && GameState.Get<int>(FlowersKeptKey, 0) <= 0;
    // Day 6: he gave her his breakfast (StoveScript), and the bowl sits by her bed
    public const string BreakfastForHerKey = "day6_breakfast_for_her";
    // How many times he's walked up to the lighthouse door today
    public static string DoorClicksKey => $"day{GameState.Get<int>("day")}_lighthouse_door_clicks";

    // Each click on the lighthouse door gets the next line, then the last one repeats. He talks
    // himself further out of it as the days go on.
    private static readonly string[] Day5DoorLines =
    {
        "She's waiting for me.",
        "It'll be fine for one night."
    };
    private static readonly string[] Day6DoorLines =
    {
        "She's waiting for me.",
        "It was fine last night.",
        "Nobody's out there anyway."
    };

    public static string NextLighthouseLine()
    {
        string[] lines = GameState.Get<int>("day") == Day ? Day5DoorLines : Day6DoorLines;
        int clicks = GameState.Get<int>(DoorClicksKey, 0);
        GameState.Set(DoorClicksKey, clicks + 1);
        return lines[Mathf.Min(clicks, lines.Length - 1)];
    }

    public static bool IsToday => GameState.Get<int>("day") == Day;
    public static bool IsCareDay
    {
        get
        {
            int day = GameState.Get<int>("day");
            return day >= Day && day <= LastCareDay;
        }
    }
    public static bool CareTreated => IsCareDay && GameState.Get<bool>(Day4Beach.MermaidTreatedKey, false);
    public static bool FlowersPicked => GameState.Get<bool>(FlowerMinigame.CompleteKey, false);
}
