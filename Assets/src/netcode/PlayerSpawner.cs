using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{

    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;

    [Rpc(SendTo.Server)]
    public void SpawnPlayerServerRpc(ulong clientId, int prefabId) {
        if (prefabId == 0) {
            var player = Instantiate(player1Prefab, spawnPoint1.position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        } else if (prefabId == 1) {
            var player = Instantiate(player2Prefab, spawnPoint2.position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        } else {
            Debug.LogError($"Invalid prefabId {prefabId} in SpawnPlayerRpc");
        }
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientID) {
        Debug.Log($"Client connected: {clientID}");
        // For testing, alternate between player 1 and player 2 prefabs
        //int prefabId = (clientID % 2 == 1) ? 0 : 1;
        SpawnPlayerServerRpc(clientID, (int)clientID);
    }

}
