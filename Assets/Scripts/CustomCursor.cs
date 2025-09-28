using UnityEngine;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{
    private static CustomCursor instance;

    [SerializeField] private Vector2 offset = new Vector2(15f, -15f); // show in Inspector

    [Header("Cursor Sprites")]
    public Sprite normalCursor;
    public Sprite pointerCursor;
    public Sprite dialogCursor;

    private Image cursorImage;

    void Awake()
    {
        // Ensure only one instance exists across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // keep across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Cursor.visible = false; // hide system cursor
        cursorImage = GetComponent<Image>(); // assumes this object has an Image
        if (normalCursor != null)
            cursorImage.sprite = normalCursor;
    }

    void Update()
    {
        // Just follow mouse position
        transform.position = (Vector2)Input.mousePosition + offset;
    }

    // --- Static functions accessing the singleton instance ---
    public static void SetCursorToPointer()
    {
        if (instance != null && instance.pointerCursor != null)
            instance.cursorImage.sprite = instance.pointerCursor;
    }

    public static void SetCursorToNormal()
    {
        if (instance != null && instance.normalCursor != null)
            instance.cursorImage.sprite = instance.normalCursor;
    }

    public static void SetCursorToDialog()
    {
        if (instance != null && instance.dialogCursor != null)
            instance.cursorImage.sprite = instance.dialogCursor;
    }
}