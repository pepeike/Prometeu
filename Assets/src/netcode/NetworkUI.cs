using Unity.Netcode;
using UnityEngine;

public class NetworkUI : NetworkBehaviour
{

    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private Camera initCamera;
    [SerializeField] private GameObject NetworkingBtns;

    public void StartHost() {
        NetworkManager.Singleton.StartHost();
        //playerSpawner.SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 0);
        initCamera.gameObject.SetActive(false);
        NetworkingBtns.SetActive(false);
    }
    public void StartClient() {
        NetworkManager.Singleton.StartClient();
        //playerSpawner.SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 1);
        initCamera.gameObject.SetActive(false);
        NetworkingBtns.SetActive(false);
    }
    public void StartServer() {
        NetworkManager.Singleton.StartServer();
    }

}
