using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab; // Сюда положим префаб Аптечки
    [SerializeField] private Transform[] _spawnPoints;       // Сюда перетащим точки со сцены
    [SerializeField] private float _respawnDelay = 10f;

    private void Start()
    {
        // Подписываемся на событие старта сервера.
        // Это надежнее, чем просто проверять IsServer в Start(), 
        // так как Start может вызваться до того, как мы нажмем кнопку "Start Host".
        NetworkManager.Singleton.OnServerStarted += SpawnAll;
    }

    private void SpawnAll()
    {
        foreach (var point in _spawnPoints)
        {
            SpawnPickup(point.position);
        }
    }

    public void OnPickedUp(Vector3 position)
    {
        // Запускаем корутину таймера респавна
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnPickup(position);
    }

    private void SpawnPickup(Vector3 position)
    {
        // Создаем объект
        var go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
        
        // Настраиваем ссылку на этот менеджер
        go.GetComponent<HealthPickup>().Init(this);
        
        // Спавним в сеть (объект появится у всех клиентов)
        go.GetComponent<NetworkObject>().Spawn();
    }
}