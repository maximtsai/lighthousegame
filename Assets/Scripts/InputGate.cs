/// <summary>
/// Whether clicks and hovers on world objects should be ignored right now. OnMouse* events come
/// from physics raycasts and don't know about UI, so anything that covers the scene (dialogue,
/// choices, the pause popup, minigames, the weather log) has to be checked here instead.
/// </summary>
public static class InputGate
{
    public static bool WorldBlocked =>
        GameState.Get<bool>("task_list_open", false)
        || GameState.Get<bool>("minigame_open", false)
        || GameState.Get<bool>("treasure_inspect_open", false)
        || GameState.Get<bool>("navigationBlocked", false)
        || GameState.Get<bool>("is_recording_weather", false)
        || GameState.Get<bool>("picking_choice", false)
        || GameState.Get<bool>("pause_open", false)
        || DialogueManager.DialogueIsOpen();
}
