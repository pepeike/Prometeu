using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public struct GridPosition //* voltar aqui depois
{
    // Estrutura para representar a posição de um segmento da cobra no grid e na memória
    public Vector2Int Position;
    public GameObject Segment;
}

public class CompetitiveSnake : MonoBehaviour//GAME MANAGER
{
    //Declaração de Variável sempre no topo da classe
    [Header("Configurações do Jogo")]
    public int GridSize = 20; // O grid será GridSize x GridSize
    public static float GameSpeed = 0.33f; // Tempo entre cada movimento da cobra (em segundos)
    public float newGameSpeed = 0.33f;
    public Vector2Int _foodPosition;

    public int posicaoInicialX = 5;
    public int posicaoInicialY = 5;

    //declaração de variável:
    //palavra-chave de acesso -> tipo de dado/variável -> NomeDoDado ;

    [Header("Assets para Visualização")]
    public GameObject FoodPrefab;
    public GameObject gridTilePrefab;
    public GameObject snakePrefab;
    //lembrar de verificar a cor da Food
    [Header("Tile Prefab Container")]
    public Transform gridTileContainer;

    // Lista que armazena as duas cobras
    private List<Snake> currentSnakes = new List<Snake>();

    //awake e enable preenchem referência e carregam
    void Start()//start inicializa e preenche valores
    {
        SetSceneryTiles();
        InitializeGame();
        GameSpeed = newGameSpeed;
    }
    public void StartGameButton()
    {
        SetSceneryTiles();
        InitializeGame();
        GameSpeed = newGameSpeed;
    }
    public void InitializeGame()
    {
        currentSnakes.Clear();

        //int playerAmount = 4;
        //for (int i = 0; i < playerAmount; i++)
        //{
        //    Snake playerSnake = Instantiate(snakePrefab, Vector3.up, Quaternion.identity).GetComponent<Snake>();
        //    currentSnakes.Add(playerSnake);
        //    playerSnake.AddBodySegment(new Vector2Int(posicaoInicialX, posicaoInicialY));
        //    //mesma linha de raciocinio e só assinala isPlayerControlled = false para as cobras do Bot
        //}

        Snake playerSnake = Instantiate(snakePrefab, Vector3.up, Quaternion.identity).GetComponent<Snake>();
        currentSnakes.Add(playerSnake);
        playerSnake.AddBodySegment(new Vector2Int(posicaoInicialX, posicaoInicialY));

        Snake cpuSnake = Instantiate(snakePrefab, Vector3.down, Quaternion.identity).GetComponent<Snake>();
        cpuSnake.IsPlayerControlled = false;
        //para entregar essa cpuSnake para OUTRO jogador, isPlayerControlled é true e ela recebe
        //o input Local pelo snakeMovement do cliente do outro player
        currentSnakes.Add(cpuSnake);
        cpuSnake.AddBodySegment(new Vector2Int(GridSize - 6, GridSize - 6));

        PlaceFood();
    }
    void SetSceneryTiles()
    {
        for (int i = 0; i < GridSize; i++)//i passa a representar linha
        {
            for (int j = 0; j < GridSize; j++)//ja representa coluna
            {
                GameObject gridTile = Instantiate(gridTilePrefab, GetWorldPosition(new Vector2Int(i, j)), Quaternion.identity);
                gridTile.transform.SetParent(gridTileContainer, true);
            }
        }
    }
    public Vector3 GetWorldPosition(Vector2Int position)
    {
        return new Vector3(position.x - GridSize / 2f + 0.5f, position.y - GridSize / 2f + 0.5f, 0);
    }
    public void PlaceFood()
    {
        //variáveis dentro do método são chamadas de variáveis com Escopo local
        Vector2Int newPos;
        bool isOccupied = false;
        do
        {
            newPos = new Vector2Int(Random.Range(0, GridSize), Random.Range(0, GridSize));
            isOccupied = false;

            // Checa se a nova posição está ocupada por alguma cobra
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
    public bool CheckCollision(Vector2Int headPos)
    {
        //caso a cabeça da cobra atinja a borda da grid e o próximo movimento
        //seja obrigatoriamente sair da grid
        if (headPos.x < 0 || headPos.x >= GridSize || headPos.y < 0 || headPos.y >= GridSize)
        {
            return true;
        }
        //percorre a lista de cobras
        foreach (var snake in currentSnakes)//cobra1
        {
            //verifica primeiro se a cobra é maior que só a cabeça
            //e verifica se ela não tentou instanciar um corpo na mesma posição da cabeça
            if (snake.Body.Count > 1 && snake.Body.Any(s => s.Position == headPos))
            {
                return true;
            }
            //verifica as outras cobras e pergunta se o corpo de alguma delas colidiu com quem
            //verificou (a cobra anterior)
            foreach (var otherSnake in currentSnakes.Where(s => s != snake))//as outras cobras em relação à cobra1
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