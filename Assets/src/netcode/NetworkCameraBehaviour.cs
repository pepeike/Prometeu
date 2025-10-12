using Unity.Netcode;
using UnityEngine;

public class NetworkCameraBehaviour : NetworkBehaviour
{

    [SerializeField] private Camera cam;

    public override void OnNetworkSpawn() {

        //base.OnNetworkSpawn();

        if (IsOwner) {
            cam.enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;
        } else {
            cam.enabled = false;
            cam.GetComponent<AudioListener>().enabled = false;
        }

        if (IsClient) {
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} spawned camera. IsOwner: {IsOwner}");
        }

    }

}
