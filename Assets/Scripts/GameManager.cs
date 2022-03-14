using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using static Firebase.Extensions.TaskExtension;
using System;
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
    public Text leaderboardText;
    [System.NonSerialized]
    public int jumpsBeyondScreen = 8;
    [SerializeField]
    #endregion

    #region Private Fields
    private int playerJumps;
    private List<GameObject> stairsList;
    private Vector3 stairsOffset;
    private DatabaseReference leaderboard;
    private readonly int maxScores = 3;
    private readonly float spawnDelay = 0.2f;
    private readonly float startNoSpawnRadius = 4f;
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
    private float NoSpawnRadius
    {
        get
        {
            float noSpawnRadius = Mathf.Max(startNoSpawnRadius - startNoSpawnRadius * (float)PlayerJumps / 40, 0);
            noSpawnRadius = Mathf.Max(noSpawnRadius, 1.2f);
            return noSpawnRadius;
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
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception is null)
            {
                leaderboard = FirebaseDatabase.DefaultInstance.GetReference("Leaders");
                leaderboard.ValueChanged += UpdateLeaderboard;
            }
            else
            {
                Debug.LogError("Firebase не инициализирована");
            }
        });

        SceneManager.sceneLoaded += OnSceneLoaded;
        stairsOffset = new Vector3(1.5f, -4.3f, -3.5f);
        stairsList = new List<GameObject>();
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
            spawnPoint.x = UnityEngine.Random.Range(-1, 2);
            Debug.LogWarning(NoSpawnRadius);
            if (!Physics.CheckSphere(spawnPoint, NoSpawnRadius))
            {
                Instantiate(GameAssets.instance.enemy, spawnPoint, Quaternion.identity);
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }
    private void InitGame()
    {
        nameField.gameObject.SetActive(false);
        leaderboardText.gameObject.SetActive(false);

        stairsList.Clear();
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
    }
    private void UpdateLeaderboard(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

		leaderboardText.text = "Ћидеры:\n";
        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.ChildrenCount > 0)
        {

            var sortedLeaders = snapshot.Children.OrderByDescending(key => key.Value);
            foreach (DataSnapshot leader in sortedLeaders)
            {
                leaderboardText.text += $"{leader.Key}: {leader.Value}\n";
            }
        }
    }
    private void AddScoreToLeaders(string nickname, int score, DatabaseReference leaderBoardRef)
    {
        leaderBoardRef.RunTransaction( mutableData =>
        {
            Dictionary<string, object> leaders = mutableData.Value as Dictionary<string, object>;
    
            if (leaders is null)
            {
                leaders = new Dictionary<string, object>();
            }
            else if (mutableData.ChildrenCount >= maxScores)
            {
                int minScore = int.MaxValue;
                string minLeader = null;
                foreach (var leader in leaders)
                {
                    int leaderScore = Convert.ToInt32(leader.Value);
                    if (leaderScore < minScore)
                    {
                        minScore = leaderScore;
                        minLeader = leader.Key;
                    }
                }
                if (minScore > score)
                {
                    return TransactionResult.Abort();
                }
                else if(!(minLeader is null))
                {
                    leaders.Remove(minLeader);
                }
            }

            leaders.Add(nickname, score);
            mutableData.Value = leaders;
            return TransactionResult.Success(mutableData);
        });
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
        if(!(leaderboard is null))
        {
            nameField.gameObject.SetActive(true);
            ShowLeaderboard();
        }
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
        if (nameField.text != "")
        {
            if (!(leaderboard is null))
            {
                AddScoreToLeaders(nameField.text, PlayerJumps, leaderboard);
            }

            nameField.gameObject.SetActive(false);
        }
    }
    #endregion
}
