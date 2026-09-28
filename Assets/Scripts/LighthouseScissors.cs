/// <summary>
/// The day 3 scissors. They go missing from the light room, as if something in the walls took
/// them, and turn up wedged in the hole behind the loose brick on climb floor 4, only visible on
/// the way up. They stay there, blocking the rest of
/// the climb, until you pull them out.
/// </summary>
public static class LighthouseScissors
{
    // The climb floor whose wall hides them.
    public const int Floor = 4;

    public const string CloseUpOpenKey = "scissors_closeup_open";

    // Gone missing and not yet pulled out of the wall.
    public static bool Stuck =>
        GameState.Get<int>("day") == 3
        && GameState.Get<bool>("scissorsDrop", false)
        && !GameState.Get<bool>("scissors_found", false);

    public static bool CloseUpOpen => GameState.Get(CloseUpOpenKey, false);

    // Climbing up past the hole isn't allowed until the scissors are out of it.
    public static bool BlocksClimbUp(int floor, bool goingUp)
    {
        return floor == Floor && goingUp && Stuck;
    }

    public static void SetCloseUpOpen(bool open)
    {
        GameState.Set(CloseUpOpenKey, open);
    }

    public static void PullOut()
    {
        GameState.Set("scissorsDrop", false);
        GameState.Set("scissors_found", true);
        MessageBus.Instance.Publish("CompleteTask", "task_find_scissors");
    }
}
