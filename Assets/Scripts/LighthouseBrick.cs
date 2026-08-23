/// <summary>
/// The loose brick on climb floor 4. It shakes free from the wall the first time you climb
/// past it each day, then stays wherever you leave it: back in the wall if you push it in,
/// otherwise on the floor, where it is still lying the next morning.
/// </summary>
public static class LighthouseBrick
{
    // What the brick did as you arrived, so the caller knows which sound to start.
    public enum Arrival
    {
        Nothing,
        Fell,
        AlreadyDown,
    }

    // The only climb floor with a brick.
    public const int Floor = 4;

    public const string InWallKey = "brick_in_wall";
    public const string FellTodayKey = "brick_fell_today";

    public static bool InWall => GameState.Get(InWallKey, true);
    public static bool OnFloor => !InWall;

    // Call straight after a climb move so the result lands over the fade. The brick only comes
    // loose once a day: push it back in and it stays in until tomorrow.
    public static Arrival Arrive(int floor, bool goingUp)
    {
        if (floor != Floor || !goingUp)
            return Arrival.Nothing;

        if (OnFloor)
            return Arrival.AlreadyDown;

        if (GameState.Get(FellTodayKey, false))
            return Arrival.Nothing;

        GameState.Set(InWallKey, false);
        GameState.Set(FellTodayKey, true);
        return Arrival.Fell;
    }

    public static void PutInWall()
    {
        GameState.Set(InWallKey, true);
    }

    // Only the daily fall resets. Where the brick is sitting carries over, so one left on the
    // floor is still there and one pushed back in falls again.
    public static void ResetForNewDay()
    {
        GameState.Set(FellTodayKey, false);
    }
}
