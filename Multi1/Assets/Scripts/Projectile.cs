using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 18f;
    [SerializeField] private int _damage = 20;

    private void Update()
    {
        // Снаряд летит вперед. Так как это происходит в Update, 
        // летит он визуально плавно на всех клиентах.
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // УРОН НАНОСИТ ТОЛЬКО СЕРВЕР! Клиенты просто игнорируют коллизию.
        if (!IsServer) return;
        if (!NetworkObject.IsSpawned) return;

        // Проверяем, попали ли мы в игрока
        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;

        // Снаряд не должен наносить урон тому, кто его выпустил
        // OwnerClientId - это ID владельца снаряда (мы зададим его при спавне)
        if (target.OwnerClientId == OwnerClientId) return;

        // Вычитаем здоровье (не опускаем ниже нуля)
        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;

        Debug.Log($"Попадание! HP цели: {target.HP.Value}");

        // Уничтожаем снаряд по сети
        GetComponent<NetworkObject>().Despawn(destroy: true);
    }
}