using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    [SerializeField] GameObject createSession;
    [SerializeField] GameObject joinSession;
    [SerializeField] GameObject sessionCode;
    [SerializeField] GameObject sessionList;
    [SerializeField] GameObject playerList;
    [SerializeField] GameObject leaveSession;
    [SerializeField] GameObject startGame;

    public void OnCreateSession() {
        createSession.SetActive(false);
        joinSession.SetActive(false);
        sessionCode.SetActive(true);
        sessionList.SetActive(false);
        playerList.SetActive(true);
        leaveSession.SetActive(true);
        startGame.SetActive(true);
    }

    public void OnJoinSession() {
        createSession.SetActive(false);
        joinSession.SetActive(false);
        sessionCode.SetActive(true);
        sessionList.SetActive(false);
        playerList.SetActive(true);
        leaveSession.SetActive(true);
        startGame.SetActive(false);
    }

    public void OnLeaveSession() {
        createSession.SetActive(true);
        joinSession.SetActive(true);
        sessionCode.SetActive(false);
        sessionList.SetActive(true);
        playerList.SetActive(false);
        leaveSession.SetActive(false);
        startGame.SetActive(false);
    }

    

}
