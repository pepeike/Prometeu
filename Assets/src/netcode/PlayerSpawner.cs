using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{

    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId, int prefabId) {
        if (prefabId == 0) {
            var player = Instantiate(player1Prefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        } else if (prefabId == 1) {
            var player = Instantiate(player2Prefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        } else {
            Debug.LogError($"Invalid prefabId {prefabId} in SpawnPlayerRpc");
        }
    }

}
