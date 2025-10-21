using TMPro;
using UnityEngine;

public class PlayerFaxUI : MonoBehaviour
{

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text chatLog;
    private PlayerFaxHandler playerFaxHandler;

    public void Initialize(PlayerFaxHandler handler)
    {
        playerFaxHandler = handler;
    }

    public void OnSendButtonPressed()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            return;
        }

        playerFaxHandler.SendFaxMessage(inputField.text);
        inputField.text = string.Empty;

    }

    public void AddTextMessage(string text)
    {
        chatLog.text += text + "\n";
    }


}
