using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Estrutura para representar a posição de um segmento da cobra no grid
public struct GridPosition
{
    public Vector2Int Position;
    public GameObject Segment;
}

// Estrutura para armazenar o estado de cada cobra (Player e CPU)
public class Snake
{
    public List<GridPosition> Body = new List<GridPosition>();
    public Vector2Int Direction;
    public KeyCode Up, Down, Left, Right; // Controles de entrada (apenas para o jogador)
    public bool IsPlayerControlled;
    public Color SnakeColor;
    public bool IsAlive = true;
}

public class CompetitiveSnake : MonoBehaviour
{
    [Header("Configurações do Jogo")]
    public int GridSize = 20; // O grid será GridSize x GridSize
    public float GameSpeed = 0.2f; // Tempo entre cada movimento da cobra (em segundos)
    private float _timer;
    private Vector2Int _foodPosition;

    [Header("Assets para Visualização")]
    public GameObject SnakeBodyPrefab;
    public GameObject FoodPrefab;
    public Material PlayerMaterial;
    public Material CPUMaterial;
    public Material FoodMaterial;

    // Lista que armazena as duas cobras
    private List<Snake> _snakes = new List<Snake>();

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (_snakes.Any(s => s.IsAlive))
        {
            HandlePlayerInput();

            _timer += Time.deltaTime;
            if (_timer >= GameSpeed)
            {
                _timer = 0f;
                MoveSnakes();
            }
        }
    }

    // --- 1. Inicialização ---
    void InitializeGame()
    {
        // Limpa a lista de cobras se já houver algum jogo rodando
        _snakes.Clear();

        // Configura a cobra do Jogador 1 (Controle manual)
        Snake playerSnake = new Snake
        {
            IsPlayerControlled = true,
            SnakeColor = PlayerMaterial.color,
            Direction = Vector2Int.right,
            Up = KeyCode.W,
            Down = KeyCode.S,
            Left = KeyCode.A,
            Right = KeyCode.D
        };
        _snakes.Add(playerSnake);

        // Configura a cobra da CPU (Controle por IA)
        Snake cpuSnake = new Snake
        {
            IsPlayerControlled = false,
            SnakeColor = CPUMaterial.color,
            Direction = Vector2Int.left,
        };
        _snakes.Add(cpuSnake);

        // Define posições iniciais no grid
        playerSnake.Body.Add(CreateSegment(new Vector2Int(5, 5), playerSnake));
        cpuSnake.Body.Add(CreateSegment(new Vector2Int(GridSize - 6, GridSize - 6), cpuSnake));

        PlaceFood();
    }

    // Cria o objeto visual para o segmento da cobra
    GridPosition CreateSegment(Vector2Int position, Snake owner)
    {
        GameObject segment = Instantiate(SnakeBodyPrefab, GetWorldPosition(position), Quaternion.identity);
        segment.GetComponent<Renderer>().material.color = owner.SnakeColor;

        return new GridPosition { Position = position, Segment = segment };
    }

    // Converte a posição do grid para a posição no mundo Unity
    Vector3 GetWorldPosition(Vector2Int position)
    {
        return new Vector3(position.x - GridSize / 2f + 0.5f, position.y - GridSize / 2f + 0.5f, 0);
    }

    // Coloca comida em uma posição vazia
    void PlaceFood()
    {
        Vector2Int newPos;
        bool isOccupied;

        do
        {
            newPos = new Vector2Int(Random.Range(0, GridSize), Random.Range(0, GridSize));
            isOccupied = false;

            // Checa se a nova posição está ocupada por alguma cobra
            foreach (var snake in _snakes)
            {
                if (snake.Body.Any(segment => segment.Position == newPos))
                {
                    isOccupied = true;
                    break;
                }
            }
        } while (isOccupied);

        _foodPosition = newPos;

        // Destrói comida antiga se existir
        GameObject existingFood = GameObject.FindGameObjectWithTag("Food");
        if (existingFood != null) Destroy(existingFood);

        // Cria novo objeto de comida
        GameObject foodObject = Instantiate(FoodPrefab, GetWorldPosition(_foodPosition), Quaternion.identity);
        foodObject.GetComponent<Renderer>().material.color = FoodMaterial.color;
        foodObject.tag = "Food";
    }

    // --- 2. Input do Jogador ---
    void HandlePlayerInput()
    {
        Snake player = _snakes.First(s => s.IsPlayerControlled);
        if (!player.IsAlive) return;

        Vector2Int newDirection = player.Direction;

        if (Input.GetKeyDown(player.Up) && player.Direction.y == 0)
            newDirection = Vector2Int.up;
        else if (Input.GetKeyDown(player.Down) && player.Direction.y == 0)
            newDirection = Vector2Int.down;
        else if (Input.GetKeyDown(player.Left) && player.Direction.x == 0)
            newDirection = Vector2Int.left;
        else if (Input.GetKeyDown(player.Right) && player.Direction.x == 0)
            newDirection = Vector2Int.right;

        player.Direction = newDirection;
    }

    // --- 3. Movimento Principal ---
    void MoveSnakes()
    {
        foreach (var snake in _snakes)
        {
            if (snake.IsAlive)
            {
                // A IA move a cobra da CPU
                if (!snake.IsPlayerControlled)
                {
                    snake.Direction = GetCpuNextMove(snake);
                }

                // Calcula a nova posição da cabeça
                Vector2Int newHeadPosition = snake.Body[0].Position + snake.Direction;

                // 1. Checa Colisões
                if (CheckCollision(newHeadPosition))
                {
                    snake.IsAlive = false;
                    Debug.Log($"A cobra {(snake.IsPlayerControlled ? "do Jogador" : "da CPU")} morreu! Fim de jogo.");
                    return;
                }

                // 2. Cria a nova cabeça
                GridPosition newHead = CreateSegment(newHeadPosition, snake);
                snake.Body.Insert(0, newHead);

                // 3. Checa Comida
                if (newHeadPosition == _foodPosition)
                {
                    // Comeu a comida, a cobra cresce
                    PlaceFood();
                }
                else
                {
                    // Não comeu, remove a cauda
                    Destroy(snake.Body.Last().Segment);
                    snake.Body.RemoveAt(snake.Body.Count - 1);
                }
            }
        }
    }

    // Checa colisões com bordas do grid, com o próprio corpo ou com a outra cobra
    bool CheckCollision(Vector2Int headPos)
    {
        // Colisão com as bordas
        if (headPos.x < 0 || headPos.x >= GridSize || headPos.y < 0 || headPos.y >= GridSize)
            return true;

        // Colisão com outras cobras ou com o próprio corpo
        foreach (var snake in _snakes)
        {
            // Colisão com o próprio corpo (ignora a cabeça atual)
            if (snake.Body.Count > 1 && snake.Body.Any(s => s.Position == headPos))
                return true;

            // Colisão com a outra cobra
            foreach (var otherSnake in _snakes.Where(s => s != snake))
            {
                if (otherSnake.Body.Any(s => s.Position == headPos))
                    return true;
            }
        }

        return false;
    }

    // --- 4. Lógica da IA da CPU (Greedy Pathfinding Simplificado) ---
    Vector2Int GetCpuNextMove(Snake cpu)
    {
        Vector2Int currentHead = cpu.Body[0].Position;
        List<Vector2Int> possibleDirections = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Remove a direção oposta para evitar auto-colisão imediata
        Vector2Int oppositeDirection = -cpu.Direction;
        possibleDirections.Remove(oppositeDirection);

        // Filtra movimentos que resultariam em colisão imediata (paredes/corpos)
        List<Vector2Int> safeDirections = new List<Vector2Int>();
        foreach (var dir in possibleDirections)
        {
            Vector2Int nextPos = currentHead + dir;
            if (!CheckCollision(nextPos))
            {
                safeDirections.Add(dir);
            }
        }

        // Se não houver movimentos seguros, a cobra irá colidir (isso é inevitável)
        if (safeDirections.Count == 0)
        {
            return possibleDirections.First(); // Retorna qualquer direção para o fim inevitável
        }

        // Estratégia Greedy: Prioriza a direção que leva mais perto da comida
        Vector2Int bestDirection = safeDirections[0];
        float minDistance = float.MaxValue;

        foreach (var dir in safeDirections)
        {
            Vector2Int nextPos = currentHead + dir;
            float distance = Vector2Int.Distance(nextPos, _foodPosition);

            if (distance < minDistance)
            {
                minDistance = distance;
                bestDirection = dir;
            }
        }

        return bestDirection;
    }
}