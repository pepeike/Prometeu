using TMPro;
using UnityEngine;

public class TurnUI : MonoBehaviour
{
    [SerializeField] private TMP_Text turnText;

    public static TurnUI Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }

    public void SetTurnText(string text) {
        turnText.text = text;
    }
}
