using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerEndTurn : NetworkBehaviour, IPointerDownHandler
{

    [SerializeField] private int playerIndex;

    public void OnPointerDown(PointerEventData eventData) {
        if (IsOwner) {
            Debug.Log($"Player {playerIndex} ended their turn.");
            SendEndTurnServerRpc(playerIndex);
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendEndTurnServerRpc(int playerIndex) {
        Debug.Log($"Server received end turn from player {playerIndex}.");
        GameManager.Instance.PlayerHitEndTurn(playerIndex); // Assuming player index 1 for this button
    }

}
