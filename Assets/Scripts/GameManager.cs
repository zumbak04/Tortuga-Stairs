using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    #region Public Fields
    public static GameManager instance = null;
    public GameObject player;
    public Text scoreText;
    public Button restartButton;
    public Button startButton;
    public InputField nameField;
    public Dictionary<string, int> leaderboard;
    public Text leaderboardText;
    [System.NonSerialized]
    public int jumpsBeyondScreen = 8;
    [SerializeField]
    #endregion

    #region Private Fields
    private int playerJumps;
    private List<GameObject> stairsList;
    private Vector3 stairsOffset;
    #endregion

    #region Properties
    public int PlayerJumps {
        get => playerJumps;
        set
        {
            playerJumps = value;
            scoreText.text = $"—чет: {playerJumps}";
        }
    }
    public float SpawnDelay
    {
        get
        {
            float spawnDelay = Mathf.Max(1f - (float)PlayerJumps / 40, 0);
            spawnDelay = Mathf.Max(spawnDelay, 0.3f);

            // Give you more time if you stay still
            if (player != null && player.TryGetComponent<Player>(out Player playerScript) && !playerScript.IsJumpingForward)
            {
                spawnDelay *= 3;
            }
            return spawnDelay;
        }
    }
    #endregion

    #region Private Methods
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        leaderboard = new Dictionary<string, int>();
        InitGame();
    }

    private void SpawnStairs(Vector3 position)
    {
        stairsList.Add(Instantiate(GameAssets.instance.stairs, position, Quaternion.identity));
    }
    private void SpawnPlayer(Vector3 position)
    {
        player = Instantiate(GameAssets.instance.player, position, Quaternion.identity);
    }
    private void DestroyStairs(GameObject stairs)
    {
        stairsList.Remove(stairs);
        Destroy(stairs);
    }
    private IEnumerator SpawnEnemy()
    {
        while (player != null)
        {
            Vector3 spawnPoint = Vector3.one * (PlayerJumps + jumpsBeyondScreen);
            spawnPoint.x = Random.Range(-1, 2);
            Instantiate(GameAssets.instance.enemy, spawnPoint, Quaternion.identity);
            yield return new WaitForSeconds(SpawnDelay);
        }
    }
    private void InitGame()
    {
        nameField.gameObject.SetActive(false);
        leaderboardText.gameObject.SetActive(false);

        stairsList = new List<GameObject>();
        stairsOffset = new Vector3(1.5f, -4.3f, -3.5f);
        PlayerJumps = 0;

        SpawnStairs(stairsOffset);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartGame();
    }
    private void ShowLeaderboard()
    {
        leaderboardText.gameObject.SetActive(true);
        UpdateLeaderboard();
    }
    private void UpdateLeaderboard()
    {
        leaderboardText.text = "Ћидеры:\n";
        List<KeyValuePair<string, int>> leaderboardList = leaderboard.ToList();
        leaderboardList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
        for (int i = 0; i < 3 && i < leaderboardList.Count; i++)
        {
            var leader = leaderboardList[i];
            leaderboardText.text += $"{leader.Key}: {leader.Value}\n";
        }
    }
    #endregion

    #region Public Methods
    public void MoveForward()
    {
        PlayerJumps++;
        if(PlayerJumps % jumpsBeyondScreen == 0)
        {
            Vector3 spawnPosition = Vector3.one * PlayerJumps + stairsOffset;
            spawnPosition.x = stairsOffset.x;
            SpawnStairs(spawnPosition);
            DestroyStairs(stairsList[0]);
        }
    }
    public void GameOver()
    {
        Destroy(player);
        StopCoroutine("SpawnEnemy");
        restartButton.gameObject.SetActive(true);
        nameField.gameObject.SetActive(true);
        ShowLeaderboard();
    }
    public void StartGame()
    {
        restartButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(true);

        InitGame();
        SpawnPlayer(new Vector3(0, 0, 0));
        StartCoroutine("SpawnEnemy");
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
    public void EnterName()
    {
        leaderboard.Add(nameField.text, PlayerJumps);
        UpdateLeaderboard();
        nameField.gameObject.SetActive(false);
    }
    #endregion
}
