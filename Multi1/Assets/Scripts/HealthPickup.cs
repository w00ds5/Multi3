using Unity.Netcode;
using UnityEngine;

public class HealthPickup : NetworkBehaviour
{
    [SerializeField] private int _healAmount = 40;

    private PickupManager _manager;
    private Vector3 _spawnPosition;

    // Этот метод вызовет PickupManager ТОЛЬКО на сервере в момент создания аптечки
    public void Init(PickupManager manager)
    {
        _manager = manager;
        _spawnPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Подбор обрабатывает только сервер
        if (!IsServer) return;
        if (!IsSpawned) return; // Защита от багов тайминга (как со снарядом)

        var player = other.GetComponent<PlayerNetwork>();
        if (player == null) return;

        // 1. Мёртвый не подбирает
        if (!player.IsAlive.Value) return;

        // 2. Не лечить при полном HP
        if (player.HP.Value >= 100) return;

        // 3. Восстанавливаем здоровье (не больше 100)
        player.HP.Value = Mathf.Min(100, player.HP.Value + _healAmount);

        // 4. Говорим менеджеру: "Я подобрана, запускай таймер"
        if (_manager != null)
        {
            _manager.OnPickedUp(_spawnPosition);
        }

        // 5. Удаляем аптечку из сети
        NetworkObject.Despawn(destroy: true);
    }
}