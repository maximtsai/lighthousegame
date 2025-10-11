using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneTransition", menuName = "Scriptable Objects/SceneTransition")]
public class SceneTransition : ScriptableObject
{
    public string destination_scene = "";
    public AudioClip travelSound;
    // variables for checking GameState before transitioning
    public string gamestate_key = "";
    public string gamestate_value = "";
    public Dialogue dialogue_on_stay = null; // Dialogue to play if no transition occurs
}