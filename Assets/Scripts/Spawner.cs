using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float interval = 3.5f;
    [SerializeField] private int maxEnemies = 10;

    private int count = 0;

    void Start()
    {
        StartCoroutine(spawnEnemy(interval, zombiePrefab));
    }

    private IEnumerator spawnEnemy(float interval, GameObject enemy)
    {
        yield return new WaitForSeconds(interval);
        GameObject newEnemy = Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
        StartCoroutine(spawnEnemy(interval, enemy));
        count++;

        if (count == maxEnemies)
        {
            Destroy(gameObject);
        }
    }
}