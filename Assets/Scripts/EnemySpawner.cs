using System.Collections;
using UnityEngine;

/// <summary>
/// 씬의 빈 오브젝트에 붙여 사용. 적이 없어지면 1초 후 다시 소환합니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    private Coroutine _spawnLoop;

    private void Start()
    {
        if (_spawnLoop == null)
            _spawnLoop = StartCoroutine(SpawnLoop());
    }

    private void OnDestroy()
    {
        if (_spawnLoop != null)
        {
            StopCoroutine(_spawnLoop);
            _spawnLoop = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        SpawnOnce();

        while (true)
        {
            while (CountActiveEnemies() > 0)
                yield return null;

            yield return new WaitForSeconds(1f);
            SpawnOnce();
        }
    }

    private static int CountActiveEnemies()
    {
        var enemies = Object.FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        return enemies != null ? enemies.Length : 0;
    }

    private void SpawnOnce()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] enemyPrefab 이 할당되지 않았습니다.");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        Instantiate(enemyPrefab, pos, rot);
    }
}