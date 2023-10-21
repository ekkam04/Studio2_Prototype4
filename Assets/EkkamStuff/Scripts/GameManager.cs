using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public List<Player> players = new List<Player>();
    public bool gameStarted = false;
    [SerializeField] Color32[] playerColors = new Color32[4];
    public Transform[] playerSpawnPoints = new Transform[4];

    void Start()
    {
        // Invoke("StartRound", 6f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartRound();
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public async void AddPlayer(PlayerInput playerInput)
    {
        Player player = playerInput.gameObject.GetComponent<Player>();
        player.allowMovement = false;
        players.Add(player);
        player.playerNumber = players.Count;
        player.ChangePlayerColor(playerColors[players.Count - 1]);
        await Task.Delay(100);
        player.Teleport(playerSpawnPoints[player.playerNumber - 1].position);
        print("Spawned player " + player.playerNumber + " at " + playerSpawnPoints[players.Count - 1].position);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        print("GameManager - Scene loaded: " + scene.name);
        SendAllPlayersToSpawn();
        Invoke("StartLevel", 3f);
    }

    void StartLevel()
    {
        gameStarted = true;
        // check scene name in switch case
        switch(SceneManager.GetActiveScene().name)
        {
            case "HexagonLevel":

                print("GameManager - Starting HexagonLevel");
                foreach (Player player in players)
                {
                    player.allowMovement = true;
                }
                TileManager[] tileManagers = FindObjectsOfType<TileManager>();
                foreach (TileManager tileManager in tileManagers)
                {
                    tileManager.crumbleTiles = true;
                }
                break;

            default:
                break;
        }
    }

    void StartRound()
    {
        foreach (Player player in players)
        {
            player.allowMovement = false;
        }
        SceneManager.LoadScene("HexagonLevel");
    }

    void SendAllPlayersToSpawn()
    {
        if (GameObject.Find("SpawnPoints") != null)
        {
            playerSpawnPoints = GameObject.Find("SpawnPoints").GetComponentsInChildren<Transform>();
            playerSpawnPoints = playerSpawnPoints[1..];
        }
        foreach (Player player in players)
        {
            player.Teleport(playerSpawnPoints[player.playerNumber - 1].position);
        }
    }
}
