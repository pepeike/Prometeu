using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Estrutura para representar a posição de um segmento da cobra no grid e na memória
public struct GridPosition
{
    public Vector2Int Position;
    public GameObject Segment;
}

public class CompetitiveSnake : MonoBehaviour // GAME MANAGER
{
    // --- 1. CONFIGURAÇÃO DE DIMENSÕES RETANGULARES (20x14) ---
    [Header("Configurações do Jogo")]
    public int GridWidth = 20;  // Largura (Eixo X) = 20 células
    public int GridHeight = 14; // Altura (Eixo Y) = 14 células
    public static float GameSpeed = 0.33f;
    public float newGameSpeed = 0.33f;
    public Vector2Int _foodPosition;

    public int posicaoInicialX = 5;
    public int posicaoInicialY = 5;

    [Header("Assets para Visualização")]
    public GameObject FoodPrefab;
    public GameObject gridTilePrefab;
    public GameObject snakePrefab;

    [Header("Tile Prefab Container")]
    public Transform gridTileContainer;

    private List<Snake> currentSnakes = new List<Snake>();

    void Start()
    {
        // Certifique-se de que SetSceneryTiles() esteja comentado/removido se estiver usando um sprite de fundo
        InitializeGame();
        GameSpeed = newGameSpeed;
    }

    public void StartGameButton()
    {
        // Certifique-se de que SetSceneryTiles() esteja comentado/removido se estiver usando um sprite de fundo
        InitializeGame();
        GameSpeed = newGameSpeed;
    }

    public void InitializeGame()
    {
        currentSnakes.Clear();

        Snake playerSnake = Instantiate(snakePrefab, Vector3.up, Quaternion.identity).GetComponent<Snake>();
        currentSnakes.Add(playerSnake);
        // Garante que a cobra nasça dentro do limite jogável (x=5, y=5)
        playerSnake.AddBodySegment(new Vector2Int(posicaoInicialX, posicaoInicialY));

        Snake cpuSnake = Instantiate(snakePrefab, Vector3.down, Quaternion.identity).GetComponent<Snake>();
        cpuSnake.IsPlayerControlled = false;
        currentSnakes.Add(cpuSnake);

        // Posição inicial da CPU ajustada para o limite do grid (GridWidth/Height - 6 para evitar a borda)
        cpuSnake.AddBodySegment(new Vector2Int(GridWidth - 6, GridHeight - 6));

        PlaceFood();
    }

    // Método mantido apenas para referência (deve ser ignorado na Opção A)
    void SetSceneryTiles()
    {
        for (int i = 0; i < GridWidth; i++)
        {
            for (int j = 0; j < GridHeight; j++)
            {
                GameObject gridTile = Instantiate(gridTilePrefab, GetWorldPosition(new Vector2Int(i, j)), Quaternion.identity);
                gridTile.transform.SetParent(gridTileContainer, true);
            }
        }
    }

    public Vector3 GetWorldPosition(Vector2Int position)
    {
        // Centraliza a posição da cobra no mundo com base nas dimensões 20x14
        return new Vector3(position.x - GridWidth / 2f + 0.5f, position.y - GridHeight / 2f + 0.5f, 0);
    }

    public void PlaceFood()
    {
        Vector2Int newPos;
        bool isOccupied = false;
        do
        {
            // Gera posição aleatória DENTRO das dimensões do grid
            newPos = new Vector2Int(Random.Range(0, GridWidth), Random.Range(0, GridHeight));
            isOccupied = false;

            // Impede que a comida apareça na moldura preta
            if (IsObstacleTile(newPos))
            {
                isOccupied = true;
                continue;
            }

            foreach (var snake in currentSnakes)
            {
                if (snake.Body.Any(segment => segment.Position == newPos))
                {
                    isOccupied = true;
                    break;
                }
            }
        } while (isOccupied);

        _foodPosition = newPos;

        GameObject existingFood = GameObject.FindGameObjectWithTag("Food");
        if (existingFood != null)
        {
            Destroy(existingFood);
        }
        GameObject foodObject = Instantiate(FoodPrefab, GetWorldPosition(_foodPosition), Quaternion.identity);
    }

    // --- LÓGICA PARA OBSTÁCULOS INTERNOS (Moldura Preta de 1 unidade) ---
    private bool IsObstacleTile(Vector2Int position)
    {
        // borderSize = 1 garante que a linha/coluna 0 e a linha/coluna (MAX - 1) sejam obstáculos.
        int borderSize = 1;

        // Coluna da Esquerda (X = 0)
        if (position.x < borderSize) return true;
        // Coluna da Direita (X = 19)
        if (position.x >= GridWidth - borderSize) return true;

        // Linha de Baixo (Y = 0)
        if (position.y < borderSize) return true;
        // Linha de Cima (Y = 13)
        if (position.y >= GridHeight - borderSize) return true;

        return false;
    }

    // --- 2. COLISÃO AJUSTADA PARA RETANGULAR E OBSTÁCULOS ---
    public bool CheckCollision(Vector2Int headPos)
    {
        // 1. Colisão com as BORDAS EXTERNAS (Evita Index Out of Bounds)
        if (headPos.x < 0 || headPos.x >= GridWidth || headPos.y < 0 || headPos.y >= GridHeight)
        {
            return true;
        }

        // 2. Colisão com a MOLDURA PRETA (Obstáculos Internos)
        // Se a posição da cabeça cair na coordenada X=0, X=19, Y=0, ou Y=13, ela morre.
        if (IsObstacleTile(headPos))
        {
            return true;
        }

        // Colisão com o corpo próprio ou com o corpo da outra cobra
        foreach (var snake in currentSnakes)
        {
            if (snake.Body.Count > 1 && snake.Body.Any(s => s.Position == headPos))
            {
                return true;
            }
            foreach (var otherSnake in currentSnakes.Where(s => s != snake))
            {
                if (otherSnake.Body.Any(s => s.Position == headPos))
                {
                    return true;
                }
            }
        }
        return false;
    }
}