using Unity.Netcode;
using UnityEngine;

public class PlayerFaxHandler : NetworkBehaviour
{

    [SerializeField] private PlayerFaxUI playerFaxUI;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerFaxUI.Initialize(this);
        }
    }

    public void SendFaxMessage(string msg)
    {
        SendFaxMessageServerRpc(msg);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendFaxMessageServerRpc(string msg, ServerRpcParams rpcParams = default)
    {
        BroadcastFaxMessageClientRpc($"Player {OwnerClientId}: {msg}");
        Debug.Log(msg);
    }

    [ClientRpc]
    private void BroadcastFaxMessageClientRpc(string msg)
    {
        var faxUI = PlayerFaxUI.Instance;
        if (faxUI != null)
        {
            faxUI.AddTextMessage(msg);
            Debug.Log("Received: " + msg);
        }
    }

}
