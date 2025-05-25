using Photon.Pun;
using TMPro;
using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviourPunCallbacks
{
    public static WaveSpawner instance;

    public GameObject[] enemyPrefabs;
    public GameObject bossPrefab;
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesText;

    public int waveNumber = 0;
    private int enemiesToSpawn;
    private int enemiesAlive = 0;
    private bool isSpawning = false;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            StartNextWave();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!isSpawning && enemiesAlive == 0)
        {
            StartNextWave();
        }
    }

    void StartNextWave()
    {
        waveNumber++;
        isSpawning = true;
        UpdateUI();
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(2f);

        enemiesToSpawn = waveNumber + Random.Range(1, 3);
        enemiesAlive = 0;

        if (waveNumber % 5 == 0)
        {
            SpawnEnemy(bossPrefab);
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.position, spawnPoint.rotation);
        enemiesAlive++;
        UpdateUI();

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeath += EnemyDied;
        }
    }

    void EnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (waveText != null)
            waveText.text = "Wave: " + waveNumber;

        if (enemiesText != null)
            enemiesText.text = "Enemies Left: " + enemiesAlive;
    }
}
