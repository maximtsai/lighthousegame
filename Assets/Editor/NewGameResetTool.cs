using UnityEditor;
using UnityEngine;

/// <summary>
/// Edit Mode only. Resets GameState like New Game and writes it to the save,
/// so the next Play session starts clean (day 1, treasures back, etc.).
/// Menu: Tools / Lighthouse / Reset Game State
/// </summary>
public static class NewGameResetTool
{
    [MenuItem("Tools/Lighthouse/Reset Game State %#n")] // Ctrl/Cmd+Shift+N
    public static void ResetGameState()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Reset Game State",
                "Exit Play Mode first.\n\nThis is an Edit Mode reset you run before playing.",
                "OK");
            return;
        }

        GameState.FullReset();
        SaveManager.Save();

        Debug.Log("Reset Game State: GameState.FullReset() + SaveManager.Save(). Ready to Play.");
        EditorUtility.DisplayDialog(
            "Reset Game State",
            "Game state fully reset and saved (day 1, flags cleared).\n\nYou can enter Play Mode now.",
            "OK");
    }

    [MenuItem("Tools/Lighthouse/Reset Game State %#n", true)]
    public static bool ResetGameStateValidate()
    {
        return !Application.isPlaying;
    }
}
