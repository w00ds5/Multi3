using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 10;

    // Точка, откуда вылетает снаряд (создадим её чуть позже)
    [SerializeField] private Transform _firePoint; 

    private float _lastShotTime;
    private int _currentAmmo;
    private PlayerNetwork _playerNetwork;

    public override void OnNetworkSpawn()
    {
        _currentAmmo = _maxAmmo;
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        // Клиент считывает нажатие пробела
        if (!IsOwner) return;
        if (!_playerNetwork.IsAlive.Value) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Отправляем запрос на сервер
            ShootServerRpc(_firePoint.position, _firePoint.forward);
        }
    }

    // Этот метод выполняется ТОЛЬКО НА СЕРВЕРЕ
    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpc = default)
    {
        // --- ВАЛИДАЦИЯ (Сервер проверяет, можно ли стрелять) ---

        // 1. Жив ли игрок? (Пока проверяем просто по HP)
        if (_playerNetwork.HP.Value <= 0) return;

        // 2. Есть ли патроны?
        if (_currentAmmo <= 0) return;

        // 3. Прошёл ли кулдаун?
        if (Time.time < _lastShotTime + _cooldown) return;

        // --- ЕСЛИ ПРОВЕРКИ ПРОЙДЕНЫ, СТРЕЛЯЕМ ---
        _lastShotTime = Time.time;
        _currentAmmo--;

        // Создаем объект на сервере
        var go = Instantiate(_projectilePrefab, pos, Quaternion.LookRotation(dir));
        var networkObject = go.GetComponent<NetworkObject>();
        
        // Спавним объект в сеть и назначаем ВЛАДЕЛЬЦЕМ того, кто вызвал ServerRpc.
        // Это нужно, чтобы в OnTriggerEnter сработало правило "не бить самого себя".
        networkObject.SpawnWithOwnership(rpc.Receive.SenderClientId);
    }
}