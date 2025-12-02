using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [HideInInspector] public List<GridPosition> Body = new List<GridPosition>();
    public Vector2Int Direction;

    public bool IsPlayerControlled;//camelCase -> inicial minúscula e todas as próximas iniciais maiúsculas
    public bool IsAlive = true;

    public SnakeMovement movementSystem;

    public GameObject SnakeBodyPrefab;
    public Color playerSnakeColor;
    public Color botSnakeColor;
    private void Awake()
    {
        movementSystem = GetComponent<SnakeMovement>();
    }
    public void AddBodySegment(Vector2Int pos)
    {
        GridPosition segmentPosition = movementSystem.CreateSegment(pos);
        Body.Add(segmentPosition);
    }
}