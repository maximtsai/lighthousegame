using System.Collections;
using UnityEngine;

/// <summary>
/// Day 4 at the dock: the wreck has washed in with the weather. Picking through the scraps of it
/// is the survey; after that the floating bodies are picked up one at a time and dropped into the
/// mass grave (see BurialScript). The first he carries up there himself; after that the trip is
/// skipped with a fade. One of them is a bride.
/// Once they're all buried a fifth turns up: the mermaid. She's breathing, so instead of the
/// grave she's carried inside to Camborne's bed (see MiscObjectClick.ClickPartnerBed).
/// </summary>
public class Day4Dock : MonoBehaviour
{
    private const string SeenKey = Day4Beach.BodiesSeenKey;
    private const string MermaidSeenKey = "day4_dock_mermaid_seen";
    private const int BrideIndex = 2;

    // What he notices picking each one up. Body i is "corpse i+1 float dock".
    private static readonly string[][] PickUpLines =
    {
        new[] { "His eyes are still open." },
        new[] { "His coat is full of seawater." },
        new[] { "Her dress.##.##.## a wedding dress.", "She died on her wedding day." },
        new[] { "Younger than me." },
    };

    // Where each floating sheet's body is in its 660x400 frames (pixels from the top-left, covering
    // every frame of its bob), for the click boxes. Body i is "corpse i+1 float dock".
    private static readonly RectInt[] BodyBoxes =
    {
        new RectInt(10, 289, 170, 76),
        new RectInt(13, 224, 103, 52),
        new RectInt(133, 229, 101, 60),
        new RectInt(494, 235, 162, 55),
    };
    // She floats where the first body was.
    private static readonly RectInt MermaidBox = new RectInt(10, 242, 221, 122);
    // The survey: the washed-up planks are in the bottom-right corner, but the click covers the
    // whole frame down to the Return strip along the bottom, so it's hard to miss.
    private static readonly RectInt ScrapsBox = new RectInt(0, 0, 660, 336);
    // It sits in front of the bodies' boxes, so clicking a body surveys first, but stays behind the
    // dialogue box's click catcher (z -3 or nearer) so dialogues still advance.
    private const float ScrapsBoxForward = 0.5f;
    // Picking something up: a quick dip to black, shorter than putting a body in the grave
    private const float PickUpFade = 0.25f;
    private const float PickUpHold = 0.15f;

    private SpriteRenderer background;
    private FloatingLayer driftwood;
    private PierMiscLogic.FloatSheet[] bodySheets;
    private PierMiscLogic.FloatSheet mermaidSheet;
    private Sprite[] carrySprites;
    private Sprite mermaidSprite;

    private bool busy;

    public void Init(SpriteRenderer background, FloatingLayer driftwood, PierMiscLogic.FloatSheet[] bodySheets,
        PierMiscLogic.FloatSheet mermaidSheet, Sprite[] carrySprites, Sprite mermaidSprite)
    {
        this.background = background;
        this.driftwood = driftwood;
        this.bodySheets = bodySheets;
        this.mermaidSheet = mermaidSheet;
        this.carrySprites = carrySprites;
        this.mermaidSprite = mermaidSprite;
    }

    void Start()
    {
        if (!Day4Beach.WreckageWashedUp)
            return;

        if (!Day4Beach.Surveyed)
            WireScraps();

        if (Day4Beach.Buried >= Day4Beach.BodyCount)
        {
            if (!Day4Beach.MermaidRescued)
            {
                SpawnMermaid();
                // Back from the grave with the last of them in it, and someone's still out there
                if (!GameState.Get<bool>(MermaidSeenKey, false))
                    StartCoroutine(MermaidFirstSightRoutine());
            }
            return;
        }

        for (int i = 0; i < Day4Beach.BodyCount && i < BodyBoxes.Length; i++)
        {
            if (!Day4Beach.BodyBuried(i) && Day4Beach.CarriedBody != i)
                SpawnBody(i);
        }

        if (!GameState.Get<bool>(SeenKey, false))
            StartCoroutine(FirstSightRoutine());
    }

    private IEnumerator FirstSightRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        GameState.Set(SeenKey, true);
        MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
        MessageBus.Instance.Publish("PlusSanity", -10);
        DialogueManager.ShowDialogueFromText(new string[]
        {
            "Bodies.## Floating in the water.",
            "The tide keeps bringing them in."
        });
    }

    private IEnumerator MermaidFirstSightRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        GameState.Set(MermaidSeenKey, true);
        DialogueManager.ShowDialogueFromText(new string[] { "Wait.## There's one more." });
    }

    private void SpawnBody(int index)
    {
        PierMiscLogic.FloatSheet sheet = bodySheets != null && index < bodySheets.Length ? bodySheets[index] : null;
        FloatingLayer layer = Spawn("day4_floating_body_" + index, sheet, BodyBoxes[index], out ClickTarget target);
        if (layer == null) return;

        GameObject body = layer.gameObject;
        target.onClick = () => OnBodyClicked(index, body);
    }

    // A floating sheet with its click box, lit up with its yellow outline while hovered
    private FloatingLayer Spawn(string name, PierMiscLogic.FloatSheet sheet, RectInt clickBox, out ClickTarget target)
    {
        target = null;
        FloatingLayer layer = FloatingLayer.Spawn(name, background, sheet != null ? sheet.frames : null);
        if (layer == null) return null;

        layer.SetOutlineFrames(sheet.outlineFrames);
        target = layer.AddClickBox(clickBox).gameObject.AddComponent<ClickTarget>();
        target.onHover = layer.SetHovered;
        return layer;
    }

    private Sprite CarrySprite(int index)
    {
        if (carrySprites == null || carrySprites.Length == 0) return null;
        return carrySprites[index % carrySprites.Length];
    }

    private void OnBodyClicked(int index, GameObject body)
    {
        if (busy) return;

        if (!Day4Beach.Surveyed)
        {
            DialogueManager.ShowDialogueFromText(new string[] { "I should see what else washed up first." });
            return;
        }

        if (Day4Beach.CarryingBody)
        {
            DialogueManager.ShowDialogueFromText(new string[] { "One at a time." });
            return;
        }

        StartCoroutine(PickUpRoutine(index, body));
    }

    // Out of the water and into his arms, in a quick dip to black. The first one he carries up to the
    // grave himself (the strip follows him there); after that the walk is skipped and it fades
    // straight to the grave.
    private IEnumerator PickUpRoutine(int index, GameObject body)
    {
        busy = true;
        GameState.Set("navigationBlocked", true);
        bool firstTrip = Day4Beach.Buried == 0;

        yield return Day4CarryStrip.InDark(PickUpFade, PickUpHold, () =>
        {
            Day4Beach.SetCarriedBody(index);
            Destroy(body);
            Day4CarryStrip.Show(CarrySprite(index));
        });

        GameState.Set("navigationBlocked", false);
        busy = false;

        string[] lines = index < PickUpLines.Length ? PickUpLines[index] : new[] { "Heavier than they look." };
        Dialogue d = DialogueManager.ShowDialogueFromText(lines);
        if (!firstTrip)
            d.onDialogueEnd.AddListener(() => Navigation.GoToScene(GameConsts.BURIALSCENE));
    }

    private void WireScraps()
    {
        if (driftwood == null) return;

        BoxCollider2D box = driftwood.AddClickBox(ScrapsBox);
        box.transform.localPosition += new Vector3(0f, 0f, -ScrapsBoxForward);
        box.gameObject.AddComponent<ClickTarget>().onClick = () =>
        {
            // Only once: the scraps go back to just floating there
            Destroy(box.gameObject);
            Survey();
        };
    }

    private void Survey()
    {
        GameState.Set(Day4Beach.SurveyedKey, true);
        // Rummaging through the wreckage (placeholder until there's a proper sound)
        MessageBus.Instance.Publish("PlaySound", "drop_item");
        DialogueManager.ShowDialogueFromText(new string[]
        {
            "You pick through the scraps on the shore.",
            "A can of corn chowder.## A different brand.",
            "Be grateful."
        });
        MessageBus.Instance.Publish("CompleteTask", "task_survey_beach");
    }

    private void SpawnMermaid()
    {
        FloatingLayer layer = Spawn("day4_floating_mermaid", mermaidSheet, MermaidBox, out ClickTarget target);
        if (layer == null) return;

        GameObject body = layer.gameObject;
        target.onClick = () => OnMermaidClicked(body);
    }

    private void OnMermaidClicked(GameObject body)
    {
        if (busy) return;

        Dialogue d = DialogueManager.ShowDialogueFromText(new string[]
        {
            "She's not like the others.",
            "Her chest.##.##.## it's moving.",
            "She's alive.## She can be saved."
        });
        d.onDialogueEnd.AddListener(() => StartCoroutine(CarryInsideRoutine(body)));
    }

    // Not to the grave: up to the house, and Camborne's empty bed. Picked up the same way as the
    // bodies; Navigation keeps him on the way to the bedroom, with her in his arms on screen.
    private IEnumerator CarryInsideRoutine(GameObject body)
    {
        busy = true;
        GameState.Set("navigationBlocked", true);

        Sprite carry = mermaidSprite != null ? mermaidSprite : CarrySprite(0);
        yield return Day4CarryStrip.InDark(PickUpFade, PickUpHold, () =>
        {
            GameState.Set(Day4Beach.MermaidRescuedKey, true);
            if (body != null) Destroy(body);
            Day4CarryStrip.Show(carry);
        });

        // There are more important things than the lighthouse now
        MessageBus.Instance.Publish("CompleteTask", "task_work");

        GameState.Set("navigationBlocked", false);
        busy = false;
        MessageBus.Instance.Publish("AddTaskStringImportant", "generic/task_mermaid_bed");
    }
}
