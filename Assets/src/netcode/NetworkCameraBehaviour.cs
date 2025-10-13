using Unity.Netcode;
using UnityEngine;

public class NetworkCameraBehaviour : NetworkBehaviour
{

    [SerializeField] private Camera cam;

    public override void OnNetworkSpawn() {

        //base.OnNetworkSpawn();
        bool isLocalPlayer = IsOwner;
        cam.enabled = isLocalPlayer;
        if (cam.GetComponent<AudioListener>() != null) {
            cam.GetComponent<AudioListener>().enabled = isLocalPlayer;
        }

        if (IsClient) {
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} spawned camera. IsOwner: {IsOwner}");
        }

    }

}
