using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeMovement : MonoBehaviour//Movimentação + Input
{
    Snake snakeComponent;
    [SerializeField] CompetitiveSnake gameManager;
    private float _timer = 0f;

    //Awake -> OnEnable -> Start
    //Racing Condition

    private void Awake()
    {
        snakeComponent = GetComponent<Snake>();
        gameManager = FindAnyObjectByType<CompetitiveSnake>().GetComponent<CompetitiveSnake>();
    }
    private void Update()//Evento da Unity que atualiza o jogo
    {
        if (snakeComponent.IsAlive == true)
        {
            HandlePlayerInput();
            _timer += Time.deltaTime;
            if (_timer >= CompetitiveSnake.GameSpeed)
            {
                _timer = 0f;
                if (snakeComponent.Body.Count > 0)
                {
                    MoveSnakes();
                }
            }
        }
    }
    private void HandlePlayerInput()
    {
        /*
         usar o InputSystem para lidar com o controle para multiplayer local
         */

        if (!snakeComponent.IsAlive)//se o jogador já foi derrotado encerra a atualização do
                                    //controle desse jogador
        {
            return;
        }

        Vector2Int newDirection = snakeComponent.Direction;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) && snakeComponent.Direction.y == 0)
        {
            newDirection = Vector2Int.up;
            //newDirection = new Vector2Int(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) && snakeComponent.Direction.y == 0)
        {
            newDirection = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) && snakeComponent.Direction.x == 0)
        {
            newDirection = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) && snakeComponent.Direction.x == 0)
        {
            newDirection = Vector2Int.right;
        }

        snakeComponent.Direction = newDirection;
    }

    private void MoveSnakes()
    {
        if (snakeComponent.IsAlive == false)
        {
            return;
        }
        if (snakeComponent.IsPlayerControlled == false)
        {
            snakeComponent.Direction = GetCpuNextMove();
        }
        Vector2Int newHeadPosition = snakeComponent.Body[0].Position + snakeComponent.Direction;

        if (gameManager.CheckCollision(newHeadPosition))
        {
            snakeComponent.IsAlive = false;
            //Debug.Log($"A cobra {(snakeComponent.IsPlayerControlled ? "do Jogador" : "da CPU")} morreu! Fim de jogo.");
            //aqui vai a lógica de começar uma partida nova ou de dar respawn no cara que morreu depois de bláblá segundos
            return;
        }

        GridPosition newHead = CreateSegment(newHeadPosition);
        snakeComponent.Body.Insert(0, newHead);

        if (newHeadPosition == gameManager._foodPosition)
        {
            gameManager.PlaceFood();
        }
        else
        {
            Destroy(snakeComponent.Body.Last().Segment);
            snakeComponent.Body.RemoveAt(snakeComponent.Body.Count - 1);
        }
    }
    private Vector2Int GetCpuNextMove()
    {
        Vector2Int currentHead = snakeComponent.Body[0].Position;
        List<Vector2Int> possibleDirections = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        Vector2Int oppositeDirection = -snakeComponent.Direction;
        possibleDirections.Remove(oppositeDirection);

        List<Vector2Int> safeDirections = new List<Vector2Int>();
        foreach (var dir in possibleDirections)
        {
            Vector2Int nextPos = currentHead + dir;
            if (!gameManager.CheckCollision(nextPos))
            {
                safeDirections.Add(dir);
            }
        }
        if (safeDirections.Count == 0)
        {
            return possibleDirections.First(); // Retorna qualquer direção para o fim inevitável
        }

        Vector2Int bestDirection = safeDirections[0];
        float minDistance = float.MaxValue;

        foreach (var dir in safeDirections)
        {
            Vector2Int nextPos = currentHead + dir;
            float distance = Vector2Int.Distance(nextPos, gameManager._foodPosition);

            if (distance < minDistance)
            {
                minDistance = distance;
                bestDirection = dir;
            }
        }
        return bestDirection;
    }
    public GridPosition CreateSegment(Vector2Int position)
    {
        GameObject segment = Instantiate(snakeComponent.SnakeBodyPrefab, gameManager.GetWorldPosition(position), Quaternion.identity);

        if (snakeComponent.IsPlayerControlled)
        {
            segment.GetComponent<SpriteRenderer>().color = snakeComponent.playerSnakeColor;
            //se for de outro player, dá outra cor
        }
        else
        {
            segment.GetComponent<SpriteRenderer>().color = snakeComponent.botSnakeColor;
        }

        return new GridPosition { Position = position, Segment = segment };
    }
}
