using System.Collections;
using UnityEngine;

/// <summary>
/// 씬의 빈 오브젝트에 붙여 사용. 적이 없어지면 1초 후 다시 소환합니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;        //소환할 적 프리팹
    [SerializeField] private Transform spawnPoint;          // 적이 소환될 위치. 할당하지 않으면 스포너 오브젝트의 위치에서 소환
    private Coroutine _spawnLoop;                           // 코루틴을 담아둘 변수

    private void Start()
    {
        // 게임이 시작되면 무한 소환 루프 시작.
        if (_spawnLoop == null)
            _spawnLoop = StartCoroutine(SpawnLoop());
    }

    private void OnDestroy()
    {
        // 이 소환 오브젝트가 파괴될 때, 돌고 있던 루프도 정지
        if (_spawnLoop != null)
        {
            StopCoroutine(_spawnLoop);
            _spawnLoop = null;
        }
    }

    // 적을 소환하고, 씬에 적이 남아있는 동안 대기하는 루프
    private IEnumerator SpawnLoop()
    {
        SpawnOnce();        // 시작하자마자 한 마리 소환

        // 계속 반복
        while (true)
        {
            // 씬에 적이 한 마리라도 남아있는 동안 대기. CountActiveEnemies()가 0이 될 때까지 매 프레임마다 null을 반환하여 대기
            while (CountActiveEnemies() > 0)
                yield return null;

            // 적이 한 마리도 남아있지 않으면 1초 기다렸다가 다시 소환
            yield return new WaitForSeconds(1f);
            SpawnOnce();
        }
    }

    // 씬에 존재하는 활성화된 적의 수를 세는 함수. 
    // Todo : 추후에 오브젝트풀링을 도입한다면 변경가능성 존재
    private static int CountActiveEnemies()
    {
        // 씬에 존재하는 Enemy 컴포넌트를 가진 활성화된 오브젝트들을 모두 찾아 배열로 반환.
        var enemies = Object.FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,    // 비활성화된 오브젝트는 제외
            FindObjectsSortMode.None);      // 정렬 방식은 신경쓰지 않음

        // 적이 있으면 해당 숫자(Lenght)를 반환하고, 없으면 0을 반환
        return enemies != null ? enemies.Length : 0;
    }

    // 실제로 적 프리팹을 화면에 만들어내는 함수
    private void SpawnOnce()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] enemyPrefab 이 할당되지 않았습니다.");
            return;
        }

        // 소환 위치와 회전값 설정
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // 프리팹을 해당 위치와 회전값으로 인스턴스화하여 씬에 소환
        Instantiate(enemyPrefab, pos, rot);
    }
}