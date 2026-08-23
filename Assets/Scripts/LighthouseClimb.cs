using UnityEngine;

public static class LighthouseClimb
{
    public const int FloorCount = 4;
    public const string FloorKey = "lh_floor";
    public const string GoingUpKey = "lh_going_up";

    // Which of the 8 climb screens is showing: floor 1-4, seen while moving up or down.
    public static int Floor => Mathf.Clamp(GameState.Get(FloorKey, 1), 1, FloorCount);
    public static bool GoingUp => GameState.Get(GoingUpKey, true);

    public static void Reset()
    {
        EnterFromGround();
    }

    public static void EnterFromGround()
    {
        GameState.Set(FloorKey, 1);
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
        if (next < 1)
            return GameConsts.LHFLOORSCENE;

        GameState.Set(FloorKey, next);
        GameState.Set(GoingUpKey, false);
        return GameConsts.LHCLIMBSCENE;
    }
}
