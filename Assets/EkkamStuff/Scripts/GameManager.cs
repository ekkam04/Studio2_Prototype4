using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player[] players = new Player[4];
    public bool gameStarted = false;
    [SerializeField] Color32[] playerColors = new Color32[4];
    [SerializeField] Color32[] teamPlayerColors = new Color32[4];
    public Transform[] playerSpawnPoints = new Transform[4];
    public GameObject[] joinPrompts = new GameObject[4];

    LevelManager levelManager;

    public RectTransform[] blackPanels;
    public RectTransform countdownText;

    void Start()
    {
        gameStarted = false;
        foreach (Player player in players)
        {
            if (player == null) continue;
            player.allowMovement = false;
            player.isReady = false;
            player.UpdateReadyText();
            player.readyText.gameObject.SetActive(true);
        }
        print("GameManager - Start");
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
            GetComponent<PlayerInputManager>().enabled = true;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
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
        // add player to players array in the next available slot
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
            {
                players[i] = player;
                break;
            }
        }
        player.playerNumber = GetNumberOfPlayers();
        player.ChangePlayerColor(playerColors[GetNumberOfPlayers() - 1]);
        if (GameObject.Find("JoinPrompts") != null)
        {
            foreach (GameObject joinPrompt in joinPrompts)
            {
                if (joinPrompt.name.Contains("_" + GetNumberOfPlayers()))
                {
                    joinPrompt.SetActive(false);
                }
            }
        }  

        await Task.Delay(100);

        player.Teleport(playerSpawnPoints[player.playerNumber - 1].position);
        print("Spawned player " + player.playerNumber + " at " + playerSpawnPoints[GetNumberOfPlayers() - 1].position);
    }

    public void RemovePlayer(PlayerInput playerInput)
    {
        print("GameManager - Removing player " + playerInput.gameObject.GetComponent<Player>().playerNumber);
        Player player = playerInput.gameObject.GetComponent<Player>();
        // remove player from players array
        foreach (Player p in players)
        {
            if (p == player)
            {
                players[p.playerNumber - 1] = null;
                break;
            }
        }
        if (GameObject.Find("JoinPrompts") != null)
        {
            foreach (GameObject joinPrompt in joinPrompts)
            {
                if (joinPrompt.name.Contains("_" + GetNumberOfPlayers()))
                {
                    joinPrompt.SetActive(true);
                }
            }
        }
        Destroy(playerInput.gameObject);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        print("GameManager - Scene loaded: " + scene.name);
        SendAllPlayersToSpawn();
        HideBlackPanels();

        if (GameObject.Find("LevelManager") != null)
        {
            print("GameManager - Starting LevelManager Camera");
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            levelManager.StartMoving();
        }

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            StartLevel();
        }
        else
        {
            Invoke("CountdownAndStartLevel", 2f);
        }
    }

    async void CountdownAndStartLevel()
    {
        // use countdownText to countdown from 3 to 1 with leantween animations
        await Task.Delay(500);
        for (int i = 3; i > 0; i--)
        {
            countdownText.GetComponent<TMPro.TextMeshProUGUI>().text = i.ToString();
            countdownText.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            LeanTween.scale(countdownText, new Vector3(1f, 1f, 1f), 0.5f).setEaseOutCubic();
            await Task.Delay(500);
            LeanTween.scale(countdownText, new Vector3(0.5f, 0.5f, 0.5f), 0.5f).setEaseInCubic();
            await Task.Delay(500);
        }
        // show "GO!" text
        countdownText.GetComponent<TMPro.TextMeshProUGUI>().text = "GO!";
        LeanTween.scale(countdownText, new Vector3(1f, 1f, 1f), 0.5f).setEaseOutCubic();
        StartLevel();
        await Task.Delay(1000);
        LeanTween.scale(countdownText, new Vector3(0f, 0f, 0f), 0.5f).setEaseInCubic();   
    }

    void StartLevel()
    {

        switch(SceneManager.GetActiveScene().name)
        {
            case "HexagonLevel":

                print("GameManager - Starting HexagonLevel");
                InitializeLevel();
                
                TileManager[] tileManagers = FindObjectsOfType<TileManager>();
                foreach (TileManager tileManager in tileManagers)
                {
                    tileManager.crumbleTiles = true;
                }
                break;

            case "Lobby":

                print("GameManager - Starting Lobby");
                gameStarted = false;
                if (GameObject.Find("JoinPrompts") != null) joinPrompts = GameObject.FindGameObjectsWithTag("JoinPrompt");
                foreach (Player player in players)
                {
                    if (player == null) continue;
                    player.allowMovement = false;
                    player.isEliminated = false;
                    player.isReady = false;
                    player.rb.useGravity = true;
                    player.UpdateReadyText();
                    player.readyText.gameObject.SetActive(true);
                    player.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.rb.velocity = Vector3.zero;
                }
                break;

            default:
                break;
        }
    }

    void InitializeLevel()
    {
        gameStarted = true;
        foreach (Player player in players)
        {
            if (player == null) continue;
            player.allowMovement = true;
            player.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            player.rb.useGravity = true;
        }
    }

    void StartRound()
    {
        ShowBlackPanels();
        foreach (Player player in players)
        {
            if (player == null) continue;
            player.allowMovement = false;
        }
        Invoke("LoadRandomLevel", 2f);
    }

    public void EndRound()
    {
        ShowBlackPanels();
        foreach (Player player in players)
        {
            if (player == null) continue;
            player.allowMovement = false;
            player.isEliminated = false;
            player.rb.useGravity = false;
            player.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            player.rb.velocity = Vector3.zero;
        }
        Invoke("GoBackToLobby", 2f);
    }

    void GoBackToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    void LoadRandomLevel()
    {
        int randomLevel = Random.Range(0, 2);
        switch (randomLevel)
        {
            case 0:
                SceneManager.LoadScene("HexagonLevel");
                break;
            case 1:
                SceneManager.LoadScene("HexagonLevel");
                break;
            default:
                break;
        }
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
            if (player == null) continue;
            player.Teleport(playerSpawnPoints[player.playerNumber - 1].position);
            player.readyText.gameObject.SetActive(false);

        }
    }

    public void PlayerPressedReady()
    {
        if (GetNumberOfPlayers() > 1)
        {
            bool allPlayersReady = true;
            foreach (Player p in players)
            {
                if (p == null) continue;
                if (!p.isReady)
                {
                    allPlayersReady = false;
                }
            }
            if (allPlayersReady)
            {
                StartRound();
            }
        }
    }

    async public void ShowBlackPanels()
    {
        // move the black panels to the right of the screen
        for (int i = 0; i < blackPanels.Length; i++)
        {
            blackPanels[i].anchoredPosition = new Vector2(1930, blackPanels[i].anchoredPosition.y);
            blackPanels[i].gameObject.SetActive(true);
        }
        // move the black panels to the left of the screen
        for (int i = 0; i < blackPanels.Length; i++)
        {
            LeanTween.moveX(blackPanels[i], 0, 1f).setEaseOutCubic();
            await Task.Delay(50);
        }
    }

    async public void HideBlackPanels()
    {
        // move the black panels to the right of the screen
        for (int i = 0; i < blackPanels.Length; i++)
        {
            LeanTween.moveX(blackPanels[i], 1930, 1f).setEaseInCubic();
            await Task.Delay(50);
        }
        await Task.Delay(1000);
        for (int i = 0; i < blackPanels.Length; i++)
        {
            blackPanels[i].gameObject.SetActive(false);
        }
    }

    int GetNumberOfPlayers()
    {
        int numberOfPlayers = 0;
        foreach (Player p in players)
        {
            if (p == null) continue;
            numberOfPlayers++;
        }
        return numberOfPlayers;
    }

    void OnApplicationQuit() {
        countdownText.GetComponent<TMPro.TextMeshProUGUI>().text = "";
    }

}
