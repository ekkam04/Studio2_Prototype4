using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
    public bool loadingLevel = false;

    public List<string> levelNames = new List<string>();
    public int currentLevelIndex = 0;

    public RectTransform[] blackPanels;
    public RectTransform countdownText;
    public TMP_Text modeText;
    public TMP_Text roundNameText;
    public GameObject lobbyPanel;
    public GameObject roundEndPanel;
    public TMP_Text roundEndText;

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
        roundNameText.text = levelNames[currentLevelIndex];
        print("GameManager - Start");

        // disable buttons from being focused with controller
        Button[] buttons = FindObjectsOfType<Button>();
        if (buttons != null)
        {
            foreach (Button button in buttons)
            {
                button.navigation = new Navigation() { mode = Navigation.Mode.None };
            }
        }
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.E))
    //     {
    //         StartRound();
    //     }
    // }

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
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
            {
                players[i] = player;
                player.playerNumber = i + 1;
                if (modeText.text == "Solo")
                {
                    player.ChangePlayerColor(playerColors[i]);
                }
                else
                {
                    player.ChangePlayerColor(teamPlayerColors[i]);
                }
                player.Teleport(playerSpawnPoints[i].position);
                if (GameObject.Find("JoinPrompts") != null)
                {
                    foreach (GameObject joinPrompt in joinPrompts)
                    {
                        if (joinPrompt.name.Contains("_" + (i + 1)))
                        {
                            joinPrompt.SetActive(false);
                        }
                    }
                }
                break;
            }
        }

        await Task.Delay(100);
        print("Spawned player " + player.playerNumber + " at " + playerSpawnPoints[GetNumberOfPlayers() - 1].position);
    }

    public void RemovePlayer(PlayerInput playerInput)
    {
        print("GameManager - Removing player " + playerInput.gameObject.GetComponent<Player>().playerNumber);
        Player player = playerInput.gameObject.GetComponent<Player>();
        foreach (Player p in players)
        {
            if (p == player)
            {
                players[p.playerNumber - 1] = null;
                if (GameObject.Find("JoinPrompts") != null)
                {
                    foreach (GameObject joinPrompt in joinPrompts)
                    {
                        if (joinPrompt.name.Contains("_" + p.playerNumber))
                        {
                            joinPrompt.SetActive(true);
                        }
                    }
                }
                break;
            }
        }
        Destroy(playerInput.gameObject);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        print("GameManager - Scene loaded: " + scene.name);
        SendAllPlayersToSpawn();
        HideBlackPanels();
        roundEndPanel.SetActive(false);
        loadingLevel = false;
        RumbleManager.instance.StopAllRumble();

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

            case "LakshyaScene":

                print("GameManager - Starting LakshyaScene");
                InitializeLevel();

                Rotatingbeam[] rotatingBeams = FindObjectsOfType<Rotatingbeam>();
                foreach (Rotatingbeam rotatingBeam in rotatingBeams)
                {
                    rotatingBeam.enabled = true;
                }
                break;

            case "Lobby":

                print("GameManager - Starting Lobby");
                gameStarted = false;
                if (GameObject.Find("JoinPrompts") != null) joinPrompts = GameObject.FindGameObjectsWithTag("JoinPrompt");
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i] == null) continue;
                    if (GameObject.Find("JoinPrompts") != null)
                    {
                        foreach (GameObject joinPrompt in joinPrompts)
                        {
                            if (joinPrompt.name.Contains("_" + (i + 1)))
                            {
                                joinPrompt.SetActive(false);
                            }
                        }
                    }
                }
                lobbyPanel.SetActive(true);
                foreach (Player player in players)
                {
                    if (player == null) continue;
                    player.allowMovement = false;
                    player.allowFall = true;
                    player.isEliminated = false;
                    player.isReady = false;
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
            player.allowFall = true;
            player.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void StartRound()
    {
        ShowBlackPanels();
        lobbyPanel.SetActive(false);
        foreach (Player player in players)
        {
            if (player == null) continue;
            player.readyText.gameObject.SetActive(false);
        }
        Invoke("LoadSelectedLevel", 2f);
    }

    public async void EndRound()
    {
        roundEndPanel.SetActive(true);

        bool winnerFound = false;
        foreach (Player player in players)
        {
            if (player == null) continue;
            if (!player.isEliminated && modeText.text == "Solo")
            {
                roundEndText.color = playerColors[player.playerNumber - 1];
                player.allowFall = false;
                winnerFound = true;
                switch (player.playerNumber)
                {
                    case 1:
                        roundEndText.text = "Red Wins!";
                        break;
                    case 2:
                        roundEndText.text = "Green Wins!";
                        break;
                    case 3:
                        roundEndText.text = "Blue Wins!";
                        break;
                    case 4:
                        roundEndText.text = "Yellow Wins!";
                        break;
                    default:
                        break;
                }
            }
            else if (!player.isEliminated && modeText.text == "Duos")
            {
                player.allowFall = false;
                if (winnerFound) continue;
                winnerFound = true;
                switch (player.playerNumber)
                {
                    case 1:
                        roundEndText.color = teamPlayerColors[0];
                        roundEndText.text = "Red Team Wins!";
                        break;
                    case 2:
                        roundEndText.color = teamPlayerColors[0];
                        roundEndText.text = "Red Team Wins!";
                        break;
                    case 3:
                        roundEndText.color = teamPlayerColors[2];
                        roundEndText.text = "Blue Team Wins!";
                        break;
                    case 4:
                        roundEndText.color = teamPlayerColors[2];
                        roundEndText.text = "Blue Team Wins!";
                        break;
                    default:
                        break;
                }
            }
        }

        if (!winnerFound)
        {
            roundEndText.color = Color.white;
            roundEndText.text = "Draw!";
        }

        LeanTween.scale(roundEndText.gameObject, new Vector3(0f, 0f, 0f), 0f);
        // LeanTween.scale(roundEndText.gameObject, new Vector3(0.5f, 0.5f, 0.5f), 0.5f).setEaseOutCubic();
        // await Task.Delay(500);
        LeanTween.scale(roundEndText.gameObject, new Vector3(1f, 1f, 1f), 0.5f).setEaseInCubic();   

        await Task.Delay(3000);
        LeanTween.scale(roundEndText.gameObject, new Vector3(0f, 0f, 0f), 0.5f).setEaseInCubic();

        ShowBlackPanels();
        foreach (Player player in players)
        {
            if (player == null) continue;
            player.allowFall = false;
            player.anim.SetBool("isJumping", false);
            player.anim.SetBool("isMoving", false);
            player.allowMovement = false;
            player.isEliminated = false;
            player.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            player.rb.velocity = Vector3.zero;
        }
        Invoke("GoBackToLobby", 2f);
    }

    void GoBackToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    void LoadSelectedLevel()
    {
        if (levelNames[currentLevelIndex] == "Crumbly Bois")
        {
            SceneManager.LoadScene("HexagonLevel");
        }
        else if (levelNames[currentLevelIndex] == "Spinny Bois")
        {
            SceneManager.LoadScene("LakshyaScene");
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
        }
    }

    public void PlayerPressedReady()
    {
        if (loadingLevel) return;
        if (modeText.text == "Solo")
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
                    loadingLevel = true;
                    Invoke("StartRound", 1f);
                }
            }
        }
        else
        {
            if (GetNumberOfPlayers() > 3)
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
                    loadingLevel = true;
                    Invoke("StartRound", 1f);
                }
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

    public void ToggleMode()
    {
        // Unready all players
        foreach (Player player in players)
        {
            if (player == null) continue;
            player.isReady = false;
            player.UpdateReadyText();
        }

        if (modeText.text == "Solo")
        {
            modeText.text = "Duos";
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;
                players[i].ChangePlayerColor(teamPlayerColors[i]);
            }
        }
        else
        {
            modeText.text = "Solo";
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;
                players[i].ChangePlayerColor(playerColors[i]);
            }
        }
    }

    public void CycleNextRoundName()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levelNames.Count)
        {
            currentLevelIndex = 0;
        }
        roundNameText.text = levelNames[currentLevelIndex];
    }

    public void CyclePreviousRoundName()
    {
        currentLevelIndex--;
        if (currentLevelIndex < 0)
        {
            currentLevelIndex = levelNames.Count - 1;
        }
        roundNameText.text = levelNames[currentLevelIndex];
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void OnApplicationQuit() {
        countdownText.GetComponent<TMPro.TextMeshProUGUI>().text = "";
    }

}
