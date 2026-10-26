using UnityEngine;

// Saved stench locations appear after grafting. An optional flower clears its own marker.
public class MermaidStench : MonoBehaviour
{
    [SerializeField] private MermaidGraft graft;
    [SerializeField] private MermaidBandage flower;
    [SerializeField] private SpriteRenderer art;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 10f;
    [SerializeField] private int startFrame;
    private bool visibilityKnown;
    private bool showing;
    private Sprite shownFrame;

    void LateUpdate()
    {
        if (art == null) return;
        bool visible = graft != null && graft.IsComplete && (flower == null || !flower.IsApplied);
        visible = visible && frames != null && frames.Length > 0;
        if (!visibilityKnown || showing != visible)
        {
            art.enabled = visible;
            showing = visible;
            visibilityKnown = true;
        }
        if (!visible) return;

        Sprite frame = frames[(startFrame + (int)(Time.time * framesPerSecond)) % frames.Length];
        if (frame != shownFrame)
        {
            art.sprite = frame;
            shownFrame = frame;
        }
    }

    void OnDisable()
    {
        if (art != null) art.enabled = false;
        visibilityKnown = false;
    }
}
