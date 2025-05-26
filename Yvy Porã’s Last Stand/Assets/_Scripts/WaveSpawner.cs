using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance;

    public GameObject[] enemyPrefabs; // Prefabs dos inimigos comuns
    public GameObject bossPrefab; // Prefab do inimigo boss
    public Transform[] spawnPoints; // Locais onde os inimigos podem spawnar
    public TextMeshProUGUI waveText; // Texto para exibir a wave
    public TextMeshProUGUI enemiesText; // Texto para exibir inimigos restantes

    public int waveNumber = 0;
    private int enemiesToSpawn;
    private int enemiesAlive = 0;
    private bool isSpawning = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        Debug.Log("WaveSpawner ativo no objeto: " + gameObject.name);

        // Adota todos os inimigos já existentes na cena (spawnados por outros scripts)
        Enemy[] existingEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy e in existingEnemies)
        {
            e.OnDeath += EnemyDied;
            enemiesAlive++;
        }

        UpdateUI();

        // Inicia a primeira wave apenas se não houver inimigos ativos
        if (enemiesAlive == 0)
        {
            StartNextWave();
        }
    }

    void Update()
    {
        // Inicia a próxima wave somente se todos os inimigos morrerem
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
        yield return new WaitForSeconds(2f); // Pequeno atraso antes da wave

        enemiesToSpawn = waveNumber + Random.Range(1, 3);

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
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        enemiesAlive++;
        Debug.Log("Inimigo spawnado. Total vivos: " + enemiesAlive);
        UpdateUI();

        Enemy enemyScript = enemy.GetComponentInChildren<Enemy>(); // mais seguro para prefabs com hierarquia
        if (enemyScript != null)
        {
            enemyScript.OnDeath += EnemyDied;
        }
        else
        {
            Debug.LogError("Componente Enemy não encontrado no inimigo instanciado!");
        }
    }

    void EnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        Debug.Log("Inimigo morreu. Restantes: " + enemiesAlive);
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
