using FishNet.Object;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 10;
    [SerializeField] private Transform _firePoint; 

    private float _lastShotTime;
    private int _currentAmmo;
    private PlayerNetwork _playerNetwork;

    public override void OnStartNetwork()
    {
        _currentAmmo = _maxAmmo;
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        if (!_playerNetwork.IsAlive.Value) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootServerRpc(_firePoint.position, _firePoint.forward);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir)
    {
        if (_playerNetwork.HP.Value <= 0) return;
        if (_currentAmmo <= 0) return;
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        _currentAmmo--;

        var go = Instantiate(_projectilePrefab, pos, Quaternion.LookRotation(dir));
        base.Spawn(go, base.Owner);
    }
}