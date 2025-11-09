using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{

    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;

    

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            return;
        }

        if (CharacterSelectManager.FinalCharacterChoices == null) {
            Debug.LogError("Character selection data is missing! Cannot spawn players.");
            return;
        }

        Debug.Log($"Spawning {CharacterSelectManager.FinalCharacterChoices.Count} players...");

        foreach (var entry in CharacterSelectManager.FinalCharacterChoices) {
            ulong clientId = entry.Key;
            int characterId = entry.Value;

            Transform spawnPosition = null;
            GameObject prefabToSpawn = null;
            if (characterId == 1) {
                prefabToSpawn = player1Prefab;
                spawnPosition = spawnPoint1;
            } else if (characterId == 2) {
                prefabToSpawn = player2Prefab;
                spawnPosition = spawnPoint2;
            }

            if (prefabToSpawn == null) {
                Debug.LogWarning($"No prefab found for character ID {characterId} for client {clientId}.");
                continue;
            }

            GameObject playerInstance = Instantiate(prefabToSpawn, spawnPosition.position, Quaternion.identity);

            NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
            if (netObj != null) {
                netObj.SpawnWithOwnership(clientId);
            } else {
                Debug.LogError($"Prefab {prefabToSpawn.name} is missing a NetworkObject component.");
                Destroy(playerInstance);
            }
        }

    }

    

}
