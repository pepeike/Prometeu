using System.Collections;
using TMPro;
using UnityEngine;

public enum TurnState {
    START,
    PLAYERS_TURN,
    ENEMY_TURN,
    WON,
    LOST
}

public enum PlayerCharacter {
    HYPERION,
    TENET
}

public class Player {
    public int playerIndex;
    public PlayerCharacter character;
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public TurnState turn;

    public TextMeshProUGUI turnText;

    public bool player1Ready = false;
    public bool player2Ready = false;

    public void PlayerHitEndTurn(int index) {
        switch (index) {
            case 1:
                player1Ready = true;
                break;
            case 2:
                player2Ready = true;
                break;
        }
        CheckPlayersFinished();
    }

    private void CheckPlayersFinished() {
        if (player1Ready && player2Ready) {
            EndPlayerTurn();
            player1Ready = false;
            player2Ready = false;
        }
    }


    private void EndPlayerTurn() {
        if (turn == TurnState.PLAYERS_TURN) {
            turn = TurnState.ENEMY_TURN;
            Debug.Log("Enemy's Turn");
            turnText.text = "ENEMY'S TURN";
            // Enemy logic here
            // After enemy turn ends, switch back to player's turn
            StartCoroutine(EndEnemyTurn());
        }
    }

    private IEnumerator EndEnemyTurn() {
        yield return new WaitForSeconds(2f);
        turn = TurnState.PLAYERS_TURN;
        Debug.Log("Player's Turn");
        turnText.text = "PLAYER'S TURN";
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
        

        turn = TurnState.START;
        turnText.text = "START";
    }

    private void FixedUpdate() {
        if (turn == TurnState.START) {
            turn = TurnState.PLAYERS_TURN;
            Debug.Log("Player's Turn");
            turnText.text = "PLAYER'S TURN";
        }
    }

}
