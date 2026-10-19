using UnityEngine;

public static class LighthouseClimb
{
    public const int FloorCount = 4;
    public const string FloorKey = "lh_floor";
    public const string GoingUpKey = "lh_going_up";

    // Which of the 8 climb screens is showing: floor 1-4, seen while moving up or down.
    public static int Floor => Mathf.Clamp(GameState.Get(FloorKey, 1), 1, FloorCount);
    public static bool GoingUp => GameState.Get(GoingUpKey, true);

    // The climb is short at first and gets longer: one flight on Day 1, two on Day 2, three on
    // Day 3 until the scissors drop, then all four from there on. The missing flights are the
    // lowest ones, so floor 4 (the loose brick, the scissors) is always there.
    public static int FlightCount
    {
        get
        {
            int day = GameState.Get<int>("day", 1);
            if (day >= 4) return FloorCount;
            if (day == 3)
            {
                bool scissorsDropped = GameState.Get<bool>("scissorsDrop", false)
                    || GameState.Get<bool>("scissors_found", false);
                return scissorsDropped ? FloorCount : 3;
            }
            return day == 2 ? 2 : 1;
        }
    }

    // The first flight above the ground floor.
    public static int LowestFloor => FloorCount - FlightCount + 1;

    public static void Reset()
    {
        EnterFromGround();
    }

    public static void EnterFromGround()
    {
        GameState.Set(FloorKey, LowestFloor);
        GameState.Set(GoingUpKey, true);
    }

    public static void EnterFromLightRoom()
    {
        GameState.Set(FloorKey, FloorCount);
        GameState.Set(GoingUpKey, false);
    }

    public static string StepUp()
    {
        int next = Floor + 1;
        if (next > FloorCount)
            return GameConsts.LHSCENE;

        GameState.Set(FloorKey, next);
        GameState.Set(GoingUpKey, true);
        return GameConsts.LHCLIMBSCENE;
    }

    public static string StepDown()
    {
        int next = Floor - 1;
        if (next < LowestFloor)
            return GameConsts.LHFLOORSCENE;

        GameState.Set(FloorKey, next);
        GameState.Set(GoingUpKey, false);
        return GameConsts.LHCLIMBSCENE;
    }
}
