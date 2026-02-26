using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour // Se encargara de generar enemigos en el juego, se le asignara un prefab de enemigo y se encargara de instanciarlo cada cierto tiempo
{
    [Header("Settings")]
    public GameObject dementorPrefab;
    public Transform playerTransform;

    [Header("Spawn Area")]
    public float minDistanceToPlayer = 3f;
    public float maxDistanceX = 10f;
    public float minY = 0.05f;
    public float maxY = 5f;

    private Coroutine spawnCoroutine;
    private float currentDelay = 2000f; // En milisegundos para ser compatible con tu lógica anterior

    // El GameManager llamará a esto para empezar el nivel
    public void StartSpawning(float initialDelay)
    {
        currentDelay = initialDelay;
        if (spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    // El GameManager llamará a esto en el GameOver o Pausa
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // Esta función actualiza el delay basándose en tu lógica de dificultad
    public void UpdateSpawnRate(float newDelay)
    {
        currentDelay = newDelay;
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnEnemy();
            // Convertimos ms a segundos para WaitForSeconds
            yield return new WaitForSeconds(currentDelay / 1000f);
        }
    }

    private void SpawnEnemy()
    {
        // Aplicamos tu lógica de rango aleatorio pero más limpia
        float spawnDirection = Random.value > 0.5f ? 1f : -1f;
        float randomX = playerTransform.position.x + (spawnDirection * Random.Range(minDistanceToPlayer, maxDistanceX));
        float randomY = Random.Range(minY, maxY);

        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);
        Instantiate(dementorPrefab, spawnPosition, Quaternion.identity);
    }

}
