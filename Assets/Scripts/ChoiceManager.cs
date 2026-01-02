using System;
using TMPro;
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    [SerializeField] private GameObject Button1; // left
    [SerializeField] private GameObject Button2; // right
    [SerializeField] private GameObject Button3; // top
    [SerializeField] private GameObject ClickBlocker; // top
    [SerializeField] private GameObject DialogBG; // top
    [SerializeField] private GameObject DialogText; // top

    private Action callback1;
    private Action callback2;
    private Action callback3;

    private void Awake()
    {
        SubscribeToMessages();
        HookupButtons();
    }

    private void Start()
    {


    }


    private void SubscribeToMessages()
    {

        // Example use:
        /*
        MessageBus.Instance.Publish(
            "ShowOneChoice",
            "Option 1 text",
            (Action)(() =>
            {
                // Option 1
                Debug.Log("do stuff!");
            })
        );
        */

        MessageBus.Instance.Subscribe("ShowOneChoice", OnShowOneChoice, this);
        MessageBus.Instance.Subscribe("ShowTwoChoice", OnShowTwoChoice, this);
        MessageBus.Instance.Subscribe("ShowThreeChoice", OnShowThreeChoice, this);

        MessageBus.Instance.Subscribe("ShowChoiceDialog", SetChoiceDialog, this);
    }

    private void HookupButtons()
    {
        Button1.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnButton1Clicked);
        Button2.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnButton2Clicked);
        Button3.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnButton3Clicked);
    }

    // Handlers
    private void OnShowOneChoice(object[] args)
    {
        // Expected:
        // [0] string
        // [1] Action

        Button3.SetActive(true);

        if (args.Length < 2)
        {
            Debug.LogError("ShowOneChoice: Invalid argument count.");
            return;
        }

        string label = args[0] as string;
        callback1 = args[1] as Action;

        // Debug.Log($"[ChoiceManager] One choice: {label}");

    }

    private void OnShowTwoChoice(object[] args)
    {
        // Expected:
        // [0] string
        // [1] string
        // [2] Action
        // [3] Action

        Button1.SetActive(true);
        Button2.SetActive(true);

        if (args.Length < 4)
        {
            Debug.LogError("ShowTwoChoice: Invalid argument count.");
            return;
        }

        string choice1 = args[0] as string;
        string choice2 = args[1] as string;

        callback1 = args[2] as Action;
        callback2 = args[3] as Action;

        Debug.Log($"[ChoiceManager] Two choices:");
        Debug.Log($"  1) {choice1}");
        Debug.Log($"  2) {choice2}");

        // Demo execution
    }

    private void OnShowThreeChoice(object[] args)
    {
        // Expected:
        // [0] string
        // [1] string
        // [2] string
        // [3] Action
        // [4] Action
        // [5] Action

        Button1.SetActive(true);
        Button2.SetActive(true);
        Button3.SetActive(true);

        if (args.Length < 6)
        {
            Debug.LogError("ShowThreeChoice: Invalid argument count.");
            return;
        }

        string choice1 = args[0] as string;
        string choice2 = args[1] as string;
        string choice3 = args[2] as string;

        callback1 = args[3] as Action;
        callback2 = args[4] as Action;
        callback3 = args[5] as Action;

        Debug.Log($"[ChoiceManager] Three choices:");
        Debug.Log($"  1) {choice1}");
        Debug.Log($"  2) {choice2}");
        Debug.Log($"  3) {choice3}");

        // Demo execution
    }

    private void OnButton1Clicked()
    {
        callback1?.Invoke();
        ClearChoices();
    }

    private void OnButton2Clicked()
    {
        callback2?.Invoke();
        ClearChoices();
    }

    private void OnButton3Clicked()
    {
        callback3?.Invoke();
        ClearChoices();
    }

    private void ClearChoices()
    {
        callback1 = null;
        callback2 = null;
        callback3 = null;

        Button1.SetActive(false);
        Button2.SetActive(false);
        Button3.SetActive(false);

        CloseDialog();
    }

    public void SetChoiceDialog(object[] args)
    {
        string str = args[0] as string;
        ClickBlocker.SetActive(true);
        DialogBG.SetActive(true);
        DialogText.SetActive(true);
        DialogText.GetComponent<TMP_Text>().SetText(str);
    }

    private void CloseDialog()
    {
        ClickBlocker.SetActive(false);
        DialogBG.SetActive(false);
        DialogText.SetActive(false);
    }


}