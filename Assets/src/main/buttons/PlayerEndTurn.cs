using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerEndTurn : MonoBehaviour, IPointerDownHandler
{

    [SerializeField] private int playerIndex;

    public void OnPointerDown(PointerEventData eventData) {
        GameManager.Instance.PlayerHitEndTurn(playerIndex); // Assuming player index 1 for this button
    }

}
