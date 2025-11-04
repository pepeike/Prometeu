using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


public class LobbyHandler : NetworkBehaviour
{
    [SerializeField] GameObject startGameBtn;
    [SerializeField] string nextSceneName;
    

    public override void OnNetworkSpawn() {
        if (IsHost) {
            startGameBtn.SetActive(true);
            startGameBtn.GetComponent<Button>().onClick.AddListener(OnStartGameClicked);
        } else {
            startGameBtn.SetActive(false);
        }
    }

    

    private void OnStartGameClicked() {
        
        NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

}
